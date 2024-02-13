use std::{
    collections::HashMap,
    ffi::{c_char, CString},
    future::IntoFuture,
    io::Write,
    ptr::NonNull,
    str::FromStr,
    sync::Arc,
};

use arrow::{
    ffi_stream::FFI_ArrowArrayStream,
    record_batch::{RecordBatch, RecordBatchIterator, RecordBatchReader},
};
use chrono::{DateTime, Duration, NaiveDateTime, Utc};
use deltalake::{
    arrow::datatypes::Schema,
    datafusion::{
        dataframe::DataFrame,
        datasource::{MemTable, TableProvider},
        execution::context::{SQLOptions, SessionContext},
    },
    kernel::StructType,
    operations::{
        constraints::ConstraintBuilder, delete::DeleteBuilder, merge::MergeBuilder,
        update::UpdateBuilder, vacuum::VacuumBuilder, write::WriteBuilder,
    },
    protocol::SaveMode,
    DeltaOps, DeltaTableBuilder,
};
use libc::c_void;

use crate::{
    error::{DeltaTableError, DeltaTableErrorCode},
    runtime::Runtime,
    schema::PartitionFilterList,
    sql::{extract_table_factor_alias, DeltaLakeParser, Statement},
    ByteArray, ByteArrayRef, CancellationToken, Dictionary, DynamicArray, KeyNullableValuePair,
    Map,
};

macro_rules! run_sync {
    ($runtime: expr, $table:expr, $rt:ident, $tbl:ident, $work: block ) => {{
        let ($rt, $tbl) = unsafe { ($runtime.as_mut(), $table.as_mut()) };
        $work
    }};
}

macro_rules! run_async_with_cancellation {
    ($runtime: expr, $table:expr, $cancellation_token: expr, $rt:ident, $tbl:ident, $work: block, $on_cancel: block ) => {{
        let ($rt, $tbl) = unsafe { ($runtime.as_mut(), $table.as_mut()) };
        let runtime_handle = $rt.handle();
        let cancel_token = $cancellation_token.map(|v| v.token.clone());
        runtime_handle.spawn(async move {
            if let Some(cancel_token) = cancel_token {
                tokio::select! {
                    _ = cancel_token.cancelled() => unsafe {$on_cancel},
                    _ = async $work => {},
                }
            } else {
                (async $work).await
            }
        });
    }};
    ($runtime: expr, $cancellation_token: expr, $rt:ident, $work: block, $on_cancel: block ) => {{
        let $rt = unsafe { $runtime.as_mut() };
        let runtime_handle = $rt.handle();
        let cancel_token = $cancellation_token.map(|v| v.token.clone());
        runtime_handle.spawn(async move {
            if let Some(cancel_token) = cancel_token {
                tokio::select! {
                    _ = cancel_token.cancelled() => unsafe {$on_cancel},
                    _ = async $work => {},
                }
            } else {
                (async $work).await
            }
        });
    }};
}

pub struct RawDeltaTable {
    table: deltalake::DeltaTable,
}

#[repr(C)]
pub struct TableMetadata {
    /// Unique identifier for this table
    id: *const c_char,
    /// User-provided identifier for this table
    name: *const c_char,
    /// User-provided description for this table
    description: *const c_char,
    /// Specification of the encoding for the files stored in the table
    format_provider: *const c_char,
    format_options: Dictionary,
    /// Schema of the table
    schema_string: *const c_char,
    /// Column names by which the data should be partitioned
    partition_columns: *mut *mut c_char,
    partition_columns_count: usize,
    /// The time when this metadata action is created, in milliseconds since the Unix epoch
    created_time: i64,
    /// Configuration options for the metadata action
    configuration: Dictionary,

    release: Option<unsafe extern "C" fn(arg1: *mut TableMetadata)>,
}

impl Drop for TableMetadata {
    fn drop(&mut self) {
        match self.release {
            None => (),
            Some(release) => unsafe { release(self) },
        };
    }
}

unsafe extern "C" fn release_metadata(metadata: *mut TableMetadata) {
    if metadata.is_null() {
        return;
    }
    let metadata = &mut *metadata;

    // take ownership back to release it.
    drop(CString::from_raw(metadata.id as *mut c_char));
    if !metadata.name.is_null() {
        drop(CString::from_raw(metadata.name as *mut c_char));
    }
    if !metadata.description.is_null() {
        drop(CString::from_raw(metadata.description as *mut c_char));
    }

    drop(CString::from_raw(metadata.format_provider as *mut c_char));
    if !metadata.format_options.values.is_null() {
        let format_options = Vec::from_raw_parts(
            metadata.format_options.values,
            metadata.format_options.length,
            metadata.format_options.capacity,
        );

        for fo in format_options.into_iter() {
            let _ = Box::from_raw(fo);
        }
    }

    if !metadata.partition_columns.is_null() {
        let partition_columns = Vec::from_raw_parts(
            metadata.partition_columns,
            metadata.partition_columns_count,
            metadata.partition_columns_count,
        );

        for partition in partition_columns.iter() {
            drop(CString::from_raw(*partition));
        }
    }

    if !metadata.configuration.values.is_null() {
        let configuration = Vec::from_raw_parts(
            metadata.configuration.values,
            metadata.configuration.length,
            metadata.configuration.capacity,
        );
        for opt in configuration.into_iter() {
            let _ = Box::from_raw(opt);
        }
    }

    metadata.release = None;
}

#[repr(C)]
pub struct ProtocolResponse {
    min_reader_version: i32,
    min_writer_version: i32,
    error: *const DeltaTableError,
}

#[repr(C)]
pub struct TableCreatOptions {
    table_uri: ByteArrayRef,
    schema: *const c_void,
    partition_by: *const ByteArrayRef,
    partition_count: usize,
    mode: ByteArrayRef,
    name: ByteArrayRef,
    description: ByteArrayRef,
    configuration: *mut Map,
    storage_options: *mut Map,
    custom_metadata: *mut Map,
}

#[repr(C)]
pub struct TableOptions {
    version: i64,
    storage_options: *mut Map,
    without_files: bool,
    log_buffer_size: libc::size_t,
}

#[repr(C)]
pub struct TableOrFail {
    runtime: *mut RawDeltaTable,
    fail: *mut DeltaTableError,
}

#[repr(C)]
pub struct BytesOrError {
    bytes: *const ByteArray,
    error: *const DeltaTableError,
}

#[repr(C)]
pub struct GenericOrError {
    bytes: *const c_void,
    error: *const DeltaTableError,
}

#[repr(C)]
pub struct MetadataOrError {
    metadata: *const TableMetadata,
    error: *const DeltaTableError,
}

#[repr(C)]
pub struct VacuumOptions {
    dry_run: bool,
    retention_hours: u64,
    enforce_retention_duration: bool,
    custom_metadata: *mut Map,
}

type TableNewCallback =
    unsafe extern "C" fn(success: *mut RawDeltaTable, fail: *const DeltaTableError);

type TableEmptyCallback = unsafe extern "C" fn(fail: *const DeltaTableError);

type GenericErrorCallback =
    unsafe extern "C" fn(success: *const c_void, fail: *const DeltaTableError);

#[no_mangle]
pub extern "C" fn table_uri(table: *const RawDeltaTable) -> *mut ByteArray {
    let table = unsafe { &*table };
    let uri = table.table.table_uri();
    ByteArray::from_utf8(uri).into_raw()
}

#[no_mangle]
pub extern "C" fn table_free(table: NonNull<RawDeltaTable>) {
    unsafe {
        let _ = Box::from_raw(table.as_ptr());
    }
}

#[no_mangle]
pub extern "C" fn create_deltalake(
    mut runtime: NonNull<Runtime>,
    options: NonNull<TableCreatOptions>,
    cancellation_token: Option<&CancellationToken>,
    callback: TableNewCallback,
) {
    let options = unsafe { options.as_ref() };
    let table_uri = options.table_uri.to_owned_string();

    let schema = unsafe { &*(options.schema as *mut arrow::ffi::FFI_ArrowSchema) };
    let schema: Schema = match schema.try_into() {
        Ok(schema) => schema,
        Err(err) => unsafe {
            callback(
                std::ptr::null_mut(),
                Box::into_raw(Box::new(DeltaTableError::new(
                    runtime.as_mut(),
                    DeltaTableErrorCode::Utf8,
                    &err.to_string(),
                ))),
            );
            return;
        },
    };
    let partition_by = unsafe {
        let partition_by =
            std::slice::from_raw_parts(options.partition_by, options.partition_count);
        partition_by
            .iter()
            .map(|b| b.to_owned_string())
            .collect::<Vec<String>>()
    };

    let (name, description, configuration, storage_options, custom_metadata) = unsafe {
        (
            options.name.to_option_string(),
            options.description.to_option_string(),
            Map::into_map(options.configuration),
            Map::into_hash_map(options.storage_options),
            Map::into_hash_map(options.custom_metadata),
        )
    };
    let save_mode = unsafe {
        match SaveMode::from_str(options.mode.to_str()) {
            Ok(save_mode) => save_mode,
            Err(err) => {
                callback(
                    std::ptr::null_mut(),
                    Box::into_raw(Box::new(DeltaTableError::new(
                        runtime.as_mut(),
                        DeltaTableErrorCode::Utf8,
                        &err.to_string(),
                    ))),
                );
                return;
            }
        }
    };
    run_async_with_cancellation!(
        runtime,
        cancellation_token,
        rt,
        {
            match create_delta_table(
                rt,
                table_uri,
                schema,
                partition_by,
                save_mode,
                name,
                description,
                configuration,
                storage_options,
                custom_metadata,
            )
            .await
            {
                Ok(table) => unsafe {
                    callback(
                        Box::into_raw(Box::new(RawDeltaTable::new(table))),
                        std::ptr::null(),
                    );
                },
                Err(err) => unsafe { callback(std::ptr::null_mut(), err.into_raw()) },
            }
        },
        { callback(std::ptr::null_mut(), std::ptr::null()) }
    )
}

#[no_mangle]
pub extern "C" fn table_new(
    mut runtime: NonNull<Runtime>,
    table_uri: NonNull<ByteArrayRef>,
    table_options: NonNull<TableOptions>,
    cancellation_token: Option<&CancellationToken>,
    callback: TableNewCallback,
) {
    let options = unsafe { table_options.as_ref() };
    let table_uri = unsafe {
        let uri = table_uri.as_ref();
        match std::str::from_utf8(uri.to_slice()) {
            Ok(table_uri) => table_uri,
            Err(err) => {
                callback(
                    std::ptr::null_mut(),
                    Box::into_raw(Box::new(DeltaTableError::new(
                        runtime.as_mut(),
                        DeltaTableErrorCode::Utf8,
                        &err.to_string(),
                    ))),
                );
                return;
            }
        }
    };

    let mut builder = match DeltaTableBuilder::from_valid_uri(table_uri) {
        Ok(builder) => builder,
        Err(err) => unsafe {
            callback(
                std::ptr::null_mut(),
                DeltaTableError::from_error(runtime.as_mut(), err).into_raw(),
            );
            return;
        },
    };

    if options.version > 0 {
        builder = builder.with_version(options.version)
    }

    unsafe {
        if let Some(storage_options) = Map::into_hash_map(options.storage_options) {
            builder = builder.with_storage_options(storage_options);
        }
    }

    if options.without_files {
        builder = builder.without_files();
    }

    if options.log_buffer_size > 0 {
        builder = builder
            .with_log_buffer_size(options.log_buffer_size)
            // unwrap is safe because it only errors when the size is negative
            .unwrap();
    }

    run_async_with_cancellation!(
        runtime,
        cancellation_token,
        rt,
        {
            match builder.load().await {
                Ok(table) => unsafe {
                    callback(
                        Box::into_raw(Box::new(RawDeltaTable::new(table))),
                        std::ptr::null(),
                    )
                },
                Err(err) => unsafe {
                    callback(
                        std::ptr::null_mut(),
                        Box::into_raw(Box::new(DeltaTableError::from_error(rt, err))),
                    )
                },
            }
        },
        { callback(std::ptr::null_mut(), std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_file_uris(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    filters: *mut PartitionFilterList,
) -> GenericOrError {
    run_sync!(runtime, table, rt, tbl, {
        match filters.is_null() {
            true => match tbl.table.get_file_uris() {
                Ok(file_uris) => GenericOrError {
                    bytes: Box::into_raw(Box::new(DynamicArray::from_vec_string(
                        file_uris.collect(),
                    ))) as *const c_void,
                    error: std::ptr::null(),
                },
                Err(err) => GenericOrError {
                    bytes: std::ptr::null(),
                    error: DeltaTableError::from_error(rt, err).into_raw(),
                },
            },
            false => {
                let map = unsafe { Box::from_raw(filters) };
                match tbl.table.get_file_uris_by_partitions(&map.filters) {
                    Ok(file_uris) => GenericOrError {
                        bytes: Box::into_raw(Box::new(DynamicArray::from_vec_string(
                            file_uris.into_iter().collect(),
                        ))) as *const c_void,
                        error: std::ptr::null(),
                    },
                    Err(err) => GenericOrError {
                        bytes: std::ptr::null(),
                        error: DeltaTableError::from_error(rt, err).into_raw(),
                    },
                }
            }
        }
    })
}

#[no_mangle]
pub extern "C" fn table_files(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    filters: *mut PartitionFilterList,
) -> GenericOrError {
    run_sync!(runtime, table, rt, tbl, {
        match filters.is_null() {
            true => match tbl.table.get_files_iter() {
                Ok(paths) => GenericOrError {
                    bytes: Box::into_raw(Box::new(DynamicArray::from_vec_string(
                        paths.map(|p| p.to_string()).collect(),
                    ))) as *const c_void,
                    error: std::ptr::null(),
                },
                Err(err) => GenericOrError {
                    bytes: std::ptr::null(),
                    error: DeltaTableError::from_error(rt, err).into_raw(),
                },
            },
            false => {
                let map = unsafe { Box::from_raw(filters) };
                match tbl.table.get_files_by_partitions(&map.filters) {
                    Ok(paths) => GenericOrError {
                        bytes: Box::into_raw(Box::new(DynamicArray::from_vec_string(
                            paths.into_iter().map(|p| p.to_string()).collect(),
                        ))) as *const c_void,
                        error: std::ptr::null(),
                    },
                    Err(err) => GenericOrError {
                        bytes: std::ptr::null(),
                        error: DeltaTableError::from_error(rt, err).into_raw(),
                    },
                }
            }
        }
    })
}

#[no_mangle]
pub extern "C" fn history(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    limit: usize,
    cancellation_token: Option<&CancellationToken>,
    callback: GenericErrorCallback,
) {
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let limit = if limit > 0 { Some(limit) } else { None };
            match tbl.table.history(limit).await {
                Ok(history) => {
                    let json = serde_json::ser::to_vec(&history).unwrap_or(Vec::new());
                    unsafe {
                        callback(
                            ByteArray::from_vec(json).into_raw() as *const c_void,
                            std::ptr::null(),
                        );
                    }
                }
                Err(err) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, err).into_raw(),
                    )
                },
            }
        },
        { callback(std::ptr::null(), std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_update_incremental(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    cancellation_token: Option<&CancellationToken>,
    callback: TableEmptyCallback,
) {
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            match tbl.table.update_incremental(None).await {
                Ok(_) => unsafe {
                    callback(std::ptr::null());
                },
                Err(err) => unsafe {
                    let error = DeltaTableError::from_error(rt, err);
                    callback(Box::into_raw(Box::new(error)))
                },
            };
        },
        { callback(std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_load_version(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    version: i64,
    cancellation_token: Option<&CancellationToken>,
    callback: TableEmptyCallback,
) {
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            match tbl.table.load_version(version).await {
                Ok(_) => unsafe { callback(std::ptr::null()) },
                Err(err) => {
                    let error = DeltaTableError::from_error(rt, err);
                    unsafe { callback(Box::into_raw(Box::new(error))) }
                }
            };
        },
        { callback(std::ptr::null()) }
    )
}

#[no_mangle]
pub extern "C" fn table_load_with_datetime(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    ts_milliseconds: i64,
    cancellation_token: Option<&CancellationToken>,
    callback: TableEmptyCallback,
) -> bool {
    let naive_dt = match NaiveDateTime::from_timestamp_millis(ts_milliseconds) {
        Some(dt) => dt,
        None => return false,
    };

    let dt = DateTime::<Utc>::from_naive_utc_and_offset(naive_dt, Utc);
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            match tbl.table.load_with_datetime(dt).await {
                Ok(_) => unsafe { callback(std::ptr::null()) },
                Err(err) => {
                    let error = DeltaTableError::from_error(rt, err);
                    unsafe { callback(Box::into_raw(Box::new(error))) }
                }
            };
        },
        { callback(std::ptr::null()) }
    );
    true
}

#[no_mangle]
pub extern "C" fn table_merge(
    mut runtime: NonNull<Runtime>,
    mut delta_table: NonNull<RawDeltaTable>,
    query: NonNull<ByteArrayRef>,
    stream: NonNull<c_void>,
    cancellation_token: Option<&CancellationToken>,
    callback: GenericErrorCallback,
) {
    let query_str = unsafe { query.as_ref().to_str() };
    let mut parser = match DeltaLakeParser::new(query_str) {
        Ok(data) => data,
        Err(err) => unsafe {
            callback(
                std::ptr::null(),
                DeltaTableError::new(
                    runtime.as_mut(),
                    DeltaTableErrorCode::Generic,
                    &err.to_string(),
                )
                .into_raw(),
            );
            return;
        },
    };
    let source_df = unsafe {
        match ffi_to_df(
            runtime.as_mut(),
            stream.cast::<FFI_ArrowArrayStream>().as_ptr(),
        ) {
            Ok(source_df) => source_df,
            Err(error) => {
                callback(std::ptr::null(), error.into_raw());
                return;
            }
        }
    };
    match parser.parse_merge() {
        Ok(Statement::MergeStatement {
            into: _,
            table,
            source,
            on,
            clauses,
        }) => run_async_with_cancellation!(
            runtime,
            delta_table,
            cancellation_token,
            rt,
            tbl,
            {
                let snapshot = match tbl.table.snapshot() {
                    Ok(snapshot) => snapshot.clone(),
                    Err(err) => unsafe {
                        callback(
                            std::ptr::null(),
                            DeltaTableError::from_error(rt, err).into_raw(),
                        );
                        return;
                    },
                };
                let mut mb =
                    MergeBuilder::new(tbl.table.log_store(), snapshot, on.to_string(), source_df);
                if let Some(target_alias) = extract_table_factor_alias(table) {
                    mb = mb.with_target_alias(target_alias);
                }

                if let Some(source_alias) = extract_table_factor_alias(source) {
                    mb = mb.with_source_alias(source_alias);
                }

                for clause in clauses {
                    let res = match clause {
                        crate::sql::MergeClause::MatchedUpdate {
                            predicate,
                            assignments,
                        } => mb.when_matched_update(|mut update| {
                            make_update!(update, predicate, assignments)
                        }),
                        crate::sql::MergeClause::MatchedDelete(predicate) => mb
                            .when_not_matched_by_source_delete(|delete| match predicate {
                                Some(predicate) => delete.predicate(predicate.to_string()),
                                None => delete,
                            }),
                        crate::sql::MergeClause::NotMatched {
                            predicate,
                            columns,
                            values,
                        } => mb.when_not_matched_insert(|mut insert| {
                            if let Some(predicate) = predicate {
                                insert = insert.predicate(predicate.to_string());
                            }

                            for row in values.rows {
                                for i in 0..columns.len() {
                                    insert = insert
                                        .set(columns[i].value.to_string(), row[i].to_string());
                                }
                            }
                            insert
                        }),
                        crate::sql::MergeClause::NotMatchedBySourceUpdate {
                            predicate,
                            assignments,
                        } => mb.when_matched_update(|mut update| {
                            make_update!(update, predicate, assignments)
                        }),
                        crate::sql::MergeClause::NotMatchedBySourceDelete(predicate) => mb
                            .when_not_matched_by_source_delete(|delete| match predicate {
                                Some(predicate) => delete.predicate(predicate.to_string()),
                                None => delete,
                            }),
                    };
                    mb = match res {
                        Ok(mb) => mb,
                        Err(error) => unsafe {
                            callback(
                                std::ptr::null(),
                                DeltaTableError::from_error(rt, error).into_raw(),
                            );
                            return;
                        },
                    }
                }
                match mb.await {
                    Ok((delta_table, metrics)) => {
                        tbl.table = delta_table;
                        let serialized = serde_json::ser::to_vec(&metrics).unwrap_or(Vec::new());
                        unsafe {
                            callback(
                                ByteArray::from_vec(serialized).into_raw() as *const c_void,
                                std::ptr::null(),
                            );
                        }
                    }
                    Err(error) => unsafe {
                        callback(
                            std::ptr::null(),
                            DeltaTableError::from_error(rt, error).into_raw(),
                        );
                    },
                };
            },
            { callback(std::ptr::null(), std::ptr::null()) }
        ),
        Err(err) => unsafe {
            callback(
                std::ptr::null(),
                DeltaTableError::new(
                    runtime.as_mut(),
                    DeltaTableErrorCode::Generic,
                    &err.to_string(),
                )
                .into_raw(),
            );
        },
    };
}

#[no_mangle]
pub extern "C" fn table_protocol_versions(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
) -> ProtocolResponse {
    run_sync!(runtime, table, rt, tbl, {
        match tbl.table.protocol() {
            Ok(protocol) => ProtocolResponse {
                min_reader_version: protocol.min_reader_version,
                min_writer_version: protocol.min_writer_version,
                error: std::ptr::null(),
            },
            Err(err) => ProtocolResponse {
                min_reader_version: 0,
                min_writer_version: 0,
                error: DeltaTableError::from_error(rt, err).into_raw(),
            },
        }
    })
}

#[no_mangle]
pub extern "C" fn table_restore(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    version_or_timestamp: i64,
    is_timestamp: bool,
    ignore_missing_files: bool,
    protocol_downgrade_allowed: bool,
    custom_metadata: *mut Map,
    cancellation_token: Option<&CancellationToken>,
    callback: TableEmptyCallback,
) {
    let json_metadata = if !custom_metadata.is_null() {
        let metadata = unsafe { Box::from_raw(custom_metadata) };
        let json_metadata: serde_json::Map<String, serde_json::Value> = metadata
            .data
            .into_iter()
            .map(|(k, v)| (k, v.into()))
            .collect();
        Some(json_metadata)
    } else {
        None
    };
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let snapshot = match tbl.table.snapshot() {
                Ok(snapshot) => snapshot.clone(),
                Err(err) => unsafe {
                    callback(DeltaTableError::from_error(rt, err).into_raw());
                    return;
                },
            };
            let mut cmd = deltalake::operations::restore::RestoreBuilder::new(
                tbl.table.log_store(),
                snapshot,
            );
            if version_or_timestamp > 0 {
                if is_timestamp {
                    let naive_dt = match NaiveDateTime::from_timestamp_millis(version_or_timestamp)
                    {
                        Some(dt) => dt,
                        None => unsafe {
                            callback(
                                DeltaTableError::new(
                                    rt,
                                    DeltaTableErrorCode::GenericError,
                                    "invalid timestamp",
                                )
                                .into_raw(),
                            );
                            return;
                        },
                    };

                    let dt = DateTime::<Utc>::from_naive_utc_and_offset(naive_dt, Utc);
                    cmd = cmd.with_datetime_to_restore(dt);
                } else {
                    cmd = cmd.with_version_to_restore(version_or_timestamp);
                }
            }
            cmd = cmd
                .with_ignore_missing_files(ignore_missing_files)
                .with_protocol_downgrade_allowed(protocol_downgrade_allowed);

            if let Some(js) = json_metadata {
                cmd = cmd.with_metadata(js);
            }

            match cmd.into_future().await {
                Ok((table, _metrics)) => unsafe {
                    tbl.table = table;
                    callback(std::ptr::null())
                },
                Err(err) => {
                    let error = DeltaTableError::from_error(rt, err);
                    unsafe { callback(Box::into_raw(Box::new(error))) }
                }
            };
        },
        { callback(std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_update(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    query: *const ByteArrayRef,
    cancellation_token: Option<&CancellationToken>,
    callback: GenericErrorCallback,
) {
    let query = {
        let query = unsafe { &*query };
        query.to_str()
    };
    let mut parser = match DeltaLakeParser::new(query) {
        Ok(parser) => parser,
        Err(error) => unsafe {
            callback(
                std::ptr::null(),
                DeltaTableError::from_parser_error(runtime.as_mut(), error).into_raw(),
            );
            return;
        },
    };
    let (predicate, assignments) = match parser.parse_update(unsafe { runtime.as_mut() }) {
        Ok(statement) => statement,
        Err(error) => unsafe {
            callback(std::ptr::null(), error.into_raw());
            return;
        },
    };
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let snapshot = match tbl.table.snapshot() {
                Ok(snapshot) => snapshot.clone(),
                Err(err) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, err).into_raw(),
                    );
                    return;
                },
            };

            let mut ub = UpdateBuilder::new(tbl.table.log_store(), snapshot);
            if let Some(predicate) = predicate {
                ub = ub.with_predicate(predicate.to_string());
            }

            for assign in assignments {
                for col in assign.id {
                    ub = ub.with_update(col.to_string(), assign.value.to_string());
                }
            }

            match ub.await {
                Ok((delta_table, metrics)) => {
                    tbl.table = delta_table;
                    let serialized = serde_json::ser::to_vec(&metrics).unwrap_or(Vec::new());
                    unsafe {
                        callback(
                            ByteArray::from_vec(serialized).into_raw() as *const c_void,
                            std::ptr::null(),
                        );
                    }
                }
                Err(error) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, error).into_raw(),
                    );
                },
            };
        },
        { callback(std::ptr::null(), std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_delete(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    predicate: Option<&ByteArrayRef>,
    cancellation_token: Option<&CancellationToken>,
    callback: GenericErrorCallback,
) {
    let predicate = predicate.and_then(|p| p.to_option_string());
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let snapshot = match tbl.table.snapshot() {
                Ok(snapshot) => snapshot.clone(),
                Err(err) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, err).into_raw(),
                    );
                    return;
                },
            };
            let mut db = DeleteBuilder::new(tbl.table.log_store(), snapshot);
            if let Some(predicate) = predicate {
                db = db.with_predicate(predicate);
            }
            match db.await {
                Ok((delta_table, metrics)) => {
                    tbl.table = delta_table;
                    let serialized = serde_json::ser::to_vec(&metrics).unwrap_or(Vec::new());
                    unsafe {
                        callback(
                            ByteArray::from_vec(serialized).into_raw() as *const c_void,
                            std::ptr::null(),
                        );
                    }
                }
                Err(error) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, error).into_raw(),
                    );
                },
            };
        },
        { callback(std::ptr::null(), std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_query(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    query: NonNull<ByteArrayRef>,
    table_name: NonNull<ByteArrayRef>,
    cancellation_token: Option<&CancellationToken>,
    callback: GenericErrorCallback,
) {
    let (query, table_name) = unsafe { (query.as_ref().to_str(), table_name.as_ref().to_str()) };
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let ctx = SessionContext::new();
            let arc = Arc::new(tbl.table.clone());
            if let Err(err) = ctx.register_table(table_name, arc) {
                unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::new(rt, DeltaTableErrorCode::DataFusion, &err.to_string())
                            .into_raw(),
                    );
                }
                return;
            }

            match ctx
                .sql_with_options(
                    query,
                    SQLOptions::new()
                        .with_allow_ddl(false)
                        .with_allow_dml(false)
                        .with_allow_statements(false),
                )
                .await
            {
                Ok(data_frame) => {
                    let schema = Schema::from(data_frame.schema());
                    let records = match data_frame.collect().await {
                        Ok(records) => records,
                        Err(error) => unsafe {
                            callback(
                                std::ptr::null(),
                                DeltaTableError::new(
                                    rt,
                                    DeltaTableErrorCode::DataFusion,
                                    &error.to_string(),
                                )
                                .into_raw(),
                            );
                            return;
                        },
                    };

                    let reader =
                        RecordBatchIterator::new(records.into_iter().map(Ok), Arc::new(schema));
                    // let reader = DataFrameStreamIterator::new(df_stream, Arc::new(schema));
                    let out_stream = arrow::ffi_stream::FFI_ArrowArrayStream::new(Box::new(reader));
                    unsafe {
                        callback(
                            Box::into_raw(Box::new(out_stream)) as *const c_void,
                            std::ptr::null(),
                        );
                    }
                }
                Err(error) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::new(
                            rt,
                            DeltaTableErrorCode::DataFusion,
                            &error.to_string(),
                        )
                        .into_raw(),
                    );
                },
            };
        },
        { callback(std::ptr::null(), std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_insert(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    stream: NonNull<c_void>,
    predicate: Option<&ByteArrayRef>,
    mode: &ByteArrayRef,
    max_rows_per_group: usize,
    overwrite_schema: bool,
    cancellation_token: Option<&CancellationToken>,
    callback: GenericErrorCallback,
) {
    let save_mode = unsafe {
        match SaveMode::from_str((*mode).to_str()) {
            Ok(save_mode) => save_mode,
            Err(err) => {
                callback(
                    std::ptr::null_mut(),
                    DeltaTableError::new(
                        runtime.as_mut(),
                        DeltaTableErrorCode::Utf8,
                        &err.to_string(),
                    )
                    .into_raw(),
                );
                return;
            }
        }
    };
    let predicate = predicate.and_then(|b| b.to_option_string());

    let (batches, _) = match ffi_to_batches(
        unsafe { runtime.as_mut() },
        stream
            .cast::<arrow::ffi_stream::FFI_ArrowArrayStream>()
            .as_ptr(),
    ) {
        Ok(batches) => batches,
        Err(err) => unsafe {
            callback(std::ptr::null(), err.into_raw());
            return;
        },
    };
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let snapshot = match tbl.table.snapshot() {
                Ok(snapshot) => snapshot.clone(),
                Err(err) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, err).into_raw(),
                    );
                    return;
                },
            };
            let mut mb = WriteBuilder::new(tbl.table.log_store(), Some(snapshot))
                .with_write_batch_size(max_rows_per_group)
                .with_input_batches(batches)
                .with_save_mode(save_mode)
                .with_overwrite_schema(overwrite_schema);
            if let Some(predicate) = predicate {
                mb = mb.with_replace_where(predicate);
            }

            if overwrite_schema {
                mb = mb.with_overwrite_schema(overwrite_schema)
            }
            match mb.await {
                Ok(updated) => {
                    tbl.table = updated;
                    unsafe {
                        callback(std::ptr::null(), std::ptr::null());
                    }
                }
                Err(error) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, error).into_raw(),
                    );
                },
            };
        },
        { callback(std::ptr::null(), std::ptr::null()) }
    );
}

/// Must free the error
#[no_mangle]
pub extern "C" fn table_schema(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
) -> GenericOrError {
    run_sync!(runtime, table, rt, tbl, {
        match crate::schema::get_schema(rt, &tbl.table) {
            Ok(schema) => {
                let schema: arrow::ffi::FFI_ArrowSchema = match schema.try_into() {
                    Ok(converted) => converted,
                    Err(err) => {
                        return GenericOrError {
                            bytes: std::ptr::null(),
                            error: DeltaTableError::new(
                                rt,
                                DeltaTableErrorCode::Arrow,
                                &err.to_string(),
                            )
                            .into_raw(),
                        }
                    }
                };
                GenericOrError {
                    bytes: Box::into_raw(Box::new(schema)) as *const c_void,
                    error: std::ptr::null(),
                }
            }
            Err(err) => GenericOrError {
                bytes: std::ptr::null(),
                error: err.into_raw(),
            },
        }
    })
}

#[no_mangle]
pub extern "C" fn table_checkpoint(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    cancellation_token: Option<&CancellationToken>,
    callback: TableEmptyCallback,
) {
    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            match deltalake::checkpoints::create_checkpoint(&tbl.table).await {
                Ok(_) => unsafe {
                    callback(std::ptr::null());
                },
                Err(err) => {
                    let error =
                        DeltaTableError::new(rt, DeltaTableErrorCode::Protocol, &err.to_string());
                    unsafe { callback(error.into_raw()) }
                }
            };
        },
        { callback(std::ptr::null()) }
    );
}

#[no_mangle]
pub extern "C" fn table_vacuum(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    options: *const VacuumOptions,
    callback: GenericErrorCallback,
) {
    let (dry_run, retention_hours, enforce_retention_duration, custom_metadata) = unsafe {
        let options = &*options;
        let retention_hours = if options.retention_hours > 0 {
            Some(options.retention_hours)
        } else {
            None
        };
        let custom_metadata = Map::into_hash_map(options.custom_metadata);
        (
            options.dry_run,
            retention_hours,
            options.enforce_retention_duration,
            custom_metadata,
        )
    };
    run_async_with_cancellation!(
        runtime,
        table,
        None::<&CancellationToken>,
        rt,
        tbl,
        {
            match vacuum(
                &mut tbl.table,
                dry_run,
                retention_hours,
                enforce_retention_duration,
                custom_metadata,
            )
            .await
            {
                Ok(strings) => {
                    let dyn_array = Box::into_raw(Box::new(DynamicArray::from_vec_string(strings)));
                    unsafe {
                        callback(dyn_array as *const c_void, std::ptr::null());
                    }
                }
                Err(err) => {
                    let error = DeltaTableError::from_error(rt, err);
                    unsafe { callback(std::ptr::null_mut(), Box::into_raw(Box::new(error))) }
                }
            }
        },
        { callback(std::ptr::null(), std::ptr::null()) }
    );
}

async fn vacuum(
    table: &mut deltalake::DeltaTable,
    dry_run: bool,
    retention_hours: Option<u64>,
    enforce_retention_duration: bool,
    custom_metadata: Option<HashMap<String, String>>,
) -> Result<Vec<String>, deltalake::DeltaTableError> {
    if table.state.is_none() {
        return Err(deltalake::DeltaTableError::NoMetadata);
    }

    let mut cmd = VacuumBuilder::new(table.log_store(), table.state.clone().unwrap())
        .with_enforce_retention_duration(enforce_retention_duration)
        .with_dry_run(dry_run);

    if let Some(retention_period) = retention_hours {
        cmd = cmd.with_retention_period(Duration::hours(retention_period as i64));
    }

    if let Some(metadata) = custom_metadata {
        let json_metadata: serde_json::Map<String, serde_json::Value> =
            metadata.into_iter().map(|(k, v)| (k, v.into())).collect();
        cmd = cmd.with_metadata(json_metadata);
    };

    let (result, metrics) = cmd.await?;
    table.state = result.state;
    Ok(metrics.files_deleted)
}

#[no_mangle]
pub extern "C" fn table_version(table_handle: NonNull<RawDeltaTable>) -> i64 {
    let table = unsafe { table_handle.as_ref() };
    table.table.version()
}

#[no_mangle]
pub extern "C" fn table_metadata(
    mut runtime: NonNull<Runtime>,
    mut table_handle: NonNull<RawDeltaTable>,
) -> MetadataOrError {
    run_sync!(runtime, table_handle, rt, table, {
        match table.table.metadata() {
            Ok(metadata) => {
                let partition_columns = metadata
                    .partition_columns
                    .clone()
                    .into_iter()
                    .map(|col| CString::new(col).unwrap().into_raw())
                    .collect::<Box<_>>();
                let table_meta = TableMetadata {
                    id: CString::new(metadata.id.clone()).unwrap().into_raw(),
                    name: metadata
                        .name
                        .clone()
                        .map(|m| CString::new(m).unwrap().into_raw())
                        .unwrap_or(std::ptr::null_mut()),
                    description: metadata
                        .description
                        .clone()
                        .map(|m| CString::new(m).unwrap().into_raw())
                        .unwrap_or(std::ptr::null_mut()),
                    format_provider: CString::new(metadata.format.provider.clone())
                        .unwrap()
                        .into_raw(),
                    format_options: Dictionary {
                        values: KeyNullableValuePair::from_optional_hash_map(
                            metadata.format.options.clone(),
                        ),
                        length: metadata.format.options.len(),
                        capacity: metadata.format.options.len(),
                    },
                    schema_string: CString::new(metadata.schema_string.clone())
                        .unwrap()
                        .into_raw(),
                    partition_columns: std::mem::ManuallyDrop::new(partition_columns).as_mut_ptr(),
                    partition_columns_count: metadata.partition_columns.len(),
                    created_time: metadata.created_time.unwrap_or(-1),
                    configuration: Dictionary {
                        values: KeyNullableValuePair::from_optional_hash_map(
                            metadata.configuration.clone(),
                        ),
                        length: metadata.configuration.len(),
                        capacity: metadata.configuration.len(),
                    },
                    release: Some(release_metadata),
                };
                MetadataOrError {
                    metadata: Box::into_raw(Box::new(table_meta)),
                    error: std::ptr::null(),
                }
            }
            Err(err) => MetadataOrError {
                metadata: std::ptr::null(),
                error: DeltaTableError::from_error(rt, err).into_raw(),
            },
        }
    })
}

#[no_mangle]
pub extern "C" fn table_add_constraints(
    mut runtime: NonNull<Runtime>,
    mut table: NonNull<RawDeltaTable>,
    constraints: *mut Map,
    custom_metadata: *mut Map,
    cancellation_token: Option<&CancellationToken>,
    callback: TableEmptyCallback,
) {
    let constraints: HashMap<String, String> = unsafe {
        Box::from_raw(constraints)
            .data
            .into_iter()
            .map(|(k, v)| (k, v.unwrap_or_default()))
            .collect()
    };
    let custom_metadata: Option<HashMap<String, String>> = unsafe {
        if custom_metadata.is_null() {
            None
        } else {
            Some(
                Box::from_raw(custom_metadata)
                    .data
                    .into_iter()
                    .map(|(k, v)| (k, v.unwrap_or_default()))
                    .collect(),
            )
        }
    };

    run_async_with_cancellation!(
        runtime,
        table,
        cancellation_token,
        rt,
        tbl,
        {
            let snapshot = match tbl.table.snapshot() {
                Ok(snapshot) => snapshot.clone(),
                Err(err) => unsafe {
                    callback(DeltaTableError::from_error(rt, err).into_raw());
                    return;
                },
            };
            let mut cmd = ConstraintBuilder::new(tbl.table.log_store(), snapshot);

            for (col_name, expression) in constraints {
                cmd = cmd.with_constraint(col_name.clone(), expression.clone());
            }

            if let Some(metadata) = custom_metadata {
                let json_metadata: serde_json::Map<String, serde_json::Value> =
                    metadata.into_iter().map(|(k, v)| (k, v.into())).collect();
                cmd = cmd.with_metadata(json_metadata);
            };

            match cmd.into_future().await {
                Ok(table) => unsafe {
                    tbl.table = table;
                    callback(std::ptr::null());
                },
                Err(error) => unsafe {
                    callback(DeltaTableError::from_error(rt, error).into_raw());
                },
            }
        },
        { callback(std::ptr::null()) }
    );
}

impl RawDeltaTable {
    fn new(table: deltalake::DeltaTable) -> Self {
        RawDeltaTable { table }
    }
}

#[allow(clippy::too_many_arguments)]
async fn create_delta_table(
    runtime: &mut Runtime,
    table_uri: String,
    schema: Schema,
    partition_by: Vec<String>,
    mode: SaveMode,
    name: Option<String>,
    description: Option<String>,
    configuration: Option<HashMap<String, Option<String>>>,
    storage_options: Option<HashMap<String, String>>,
    custom_metadata: Option<HashMap<String, String>>,
) -> Result<deltalake::DeltaTable, DeltaTableError> {
    let table = DeltaTableBuilder::from_valid_uri(table_uri)
        .map_err(|err| DeltaTableError::from_error(runtime, err))?
        .with_storage_options(storage_options.unwrap_or_default())
        .build()
        .map_err(|error| DeltaTableError::from_error(runtime, error))?;
    let delta_schema = StructType::try_from(&schema).map_err(|error| {
        DeltaTableError::new(runtime, DeltaTableErrorCode::Arrow, &error.to_string())
    })?;
    let mut builder = DeltaOps(table)
        .create()
        .with_columns(delta_schema.fields().clone())
        .with_save_mode(mode)
        .with_partition_columns(partition_by);
    if let Some(name) = &name {
        builder = builder.with_table_name(name);
    };

    if let Some(description) = &description {
        builder = builder.with_comment(description);
    };

    if let Some(config) = configuration {
        builder = builder.with_configuration(config);
    };

    if let Some(metadata) = custom_metadata {
        let json_metadata: serde_json::Map<String, serde_json::Value> =
            metadata.into_iter().map(|(k, v)| (k, v.into())).collect();
        builder = builder.with_metadata(json_metadata);
    };

    let table = builder
        .await
        .map_err(|error| DeltaTableError::from_error(runtime, error))?;
    Ok(table)
}

fn ffi_to_df(
    runtime: &mut Runtime,
    stream: *mut arrow::ffi_stream::FFI_ArrowArrayStream,
) -> Result<DataFrame, DeltaTableError> {
    let (read_batches, schema) = ffi_to_batches(runtime, stream)?;
    let table_provider: Arc<dyn TableProvider> = match MemTable::try_new(schema, vec![read_batches])
    {
        Ok(mem_table) => Arc::new(mem_table),
        Err(error) => {
            return Err(DeltaTableError::new(
                runtime,
                DeltaTableErrorCode::DataFusion,
                &error.to_string(),
            ));
        }
    };
    let ctx = SessionContext::new();
    ctx.read_table(table_provider).map_err(|error| {
        DeltaTableError::new(runtime, DeltaTableErrorCode::Arrow, &error.to_string())
    })
}

fn ffi_to_batches(
    runtime: &mut Runtime,
    stream: *mut arrow::ffi_stream::FFI_ArrowArrayStream,
) -> Result<(Vec<RecordBatch>, Arc<Schema>), DeltaTableError> {
    let reader = unsafe {
        match arrow::ffi_stream::ArrowArrayStreamReader::from_raw(stream) {
            Ok(reader) => reader,
            Err(error) => {
                return Err(DeltaTableError::new(
                    runtime,
                    DeltaTableErrorCode::Arrow,
                    &error.to_string(),
                ));
            }
        }
    };
    let schema = reader.schema();
    let mut read_batches: Vec<RecordBatch> = Vec::new();
    for batch in reader {
        match batch {
            Ok(batch) => read_batches.push(batch),
            Err(error) => {
                return Err(DeltaTableError::new(
                    runtime,
                    DeltaTableErrorCode::Arrow,
                    &error.to_string(),
                ));
            }
        }
    }
    Ok((read_batches, schema))
}

#[cfg(test)]
mod tests {
    use crate::sql::{DeltaLakeParser, Statement};

    #[test]
    fn test_parse_merge() {
        let parser = DeltaLakeParser::new(
            "MERGE INTO people10m
        USING people10mupdates
        ON people10m.id = people10mupdates.id
        WHEN MATCHED THEN
          UPDATE SET
            id = people10mupdates.id,
            firstName = people10mupdates.firstName,
            middleName = people10mupdates.middleName,
            lastName = people10mupdates.lastName,
            gender = people10mupdates.gender,
            birthDate = people10mupdates.birthDate,
            ssn = people10mupdates.ssn,
            salary = people10mupdates.salary
        WHEN NOT MATCHED BY SOURCE THEN DELETE
        WHEN NOT MATCHED BY TARGET
          THEN INSERT (
            id,
            firstName,
            middleName,
            lastName,
            gender,
            birthDate,
            ssn,
            salary
          )
          VALUES (
            people10mupdates.id,
            people10mupdates.firstName,
            people10mupdates.middleName,
            people10mupdates.lastName,
            people10mupdates.gender,
            people10mupdates.birthDate,
            people10mupdates.ssn,
            people10mupdates.salary
          )",
        );
        let mut res = parser.expect("this should parser");
        let stmt = res.parse_merge().expect("the statement should parse");
        match stmt {
            Statement::MergeStatement {
                into: _,
                table: _,
                source: _,
                on: _,
                clauses,
            } => {
                assert!(!clauses.is_empty());
            }
        }
    }
}
