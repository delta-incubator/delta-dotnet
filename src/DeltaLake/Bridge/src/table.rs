use std::{
    collections::HashMap,
    ffi::{c_char, CString},
    str::FromStr,
};

use chrono::{DateTime, Duration, NaiveDateTime, Utc};
use deltalake::{
    arrow::datatypes::Schema, kernel::StructType, operations::vacuum::VacuumBuilder,
    protocol::SaveMode, DeltaOps, DeltaTableBuilder,
};
use libc::c_void;

use crate::{
    error::{DeltaTableError, DeltaTableErrorCode},
    runtime::{map_free, Runtime},
    schema::PartitionFilterList,
    ByteArray, ByteArrayRef, CancellationToken, DynamicArray, Map, SerializedBuffer,
};

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
    format_options: *mut Map,
    /// Schema of the table
    schema_string: *const c_char,
    /// Column names by which the data should be partitioned
    partition_columns: *mut *mut c_char,
    partition_columns_count: usize,
    /// The time when this metadata action is created, in milliseconds since the Unix epoch
    created_time: i64,
    /// Configuration options for the metadata action
    configuration: *mut Map,

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
    drop(Box::from_raw(metadata.format_options));
    let partition_columns = Vec::from_raw_parts(
        metadata.partition_columns,
        metadata.partition_columns_count,
        metadata.partition_columns_count,
    );
    for partition in partition_columns.iter() {
        drop(CString::from_raw(*partition));
    }
    drop(Box::from_raw(metadata.configuration));

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
pub extern "C" fn table_free(table: *mut RawDeltaTable) {
    unsafe {
        let _ = Box::from_raw(table);
    }
}

#[no_mangle]
pub extern "C" fn create_deltalake(
    runtime: *mut Runtime,
    options: *const TableCreatOptions,
    callback: TableNewCallback,
) {
    let (runtime, options) = unsafe { (&mut *runtime, &*options) };
    let table_uri = options.table_uri.to_owned_string();

    let schema = unsafe { &*(options.schema as *mut arrow::ffi::FFI_ArrowSchema) };
    let schema: Schema = match schema.try_into() {
        Ok(schema) => schema,
        Err(err) => unsafe {
            callback(
                std::ptr::null_mut(),
                Box::into_raw(Box::new(DeltaTableError::new(
                    runtime,
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
            options.name.to_option_string(),
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
                        runtime,
                        DeltaTableErrorCode::Utf8,
                        &err.to_string(),
                    ))),
                );
                return;
            }
        }
    };
    runtime.handle().spawn(async move {
        match create_delta_table(
            runtime,
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
            Err(err) => unsafe {
                println!("calling on error");
                callback(std::ptr::null_mut(), err.into_raw())
            },
        }
    });
}

#[no_mangle]
pub extern "C" fn table_new(
    runtime: *mut Runtime,
    table_uri: *const ByteArrayRef,
    table_options: *const TableOptions,
    callback: TableNewCallback,
) {
    let (runtime, options) = unsafe { (&mut *runtime, &*table_options) };
    let table_uri = unsafe {
        let uri = &*table_uri;
        match std::str::from_utf8(uri.to_slice()) {
            Ok(table_uri) => table_uri,
            Err(err) => {
                callback(
                    std::ptr::null_mut(),
                    Box::into_raw(Box::new(DeltaTableError::new(
                        runtime,
                        DeltaTableErrorCode::Utf8,
                        &err.to_string(),
                    ))),
                );
                return;
            }
        }
    };

    let mut builder = DeltaTableBuilder::from_uri(table_uri);

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

    let runtime_handle = runtime.handle();
    runtime_handle.spawn(async move {
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
                    Box::into_raw(Box::new(DeltaTableError::from_error(runtime, err))),
                )
            },
        }
    });
}

#[no_mangle]
pub extern "C" fn table_file_uris(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    filters: *mut PartitionFilterList,
) -> GenericOrError {
    do_with_table_and_runtime_sync(runtime, table, |rt, tbl| match filters.is_null() {
        true => match tbl.table.get_file_uris() {
            Ok(file_uris) => GenericOrError {
                bytes: Box::into_raw(Box::new(DynamicArray::from_vec_string(file_uris.collect())))
                    as *const c_void,
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
    })
}

#[no_mangle]
pub extern "C" fn table_files(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    filters: *mut PartitionFilterList,
) -> GenericOrError {
    do_with_table_and_runtime_sync(runtime, table, |rt, tbl| match filters.is_null() {
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
    })
}

#[no_mangle]
pub extern "C" fn history(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    limit: usize,
    cancellation_token: *const CancellationToken,
    callback: GenericErrorCallback,
) {
    do_with_table_and_runtime_and_cancel(
        runtime,
        table,
        cancellation_token,
        move |rt, tbl| async move {
            let limit = if limit > 0 { Some(limit) } else { None };
            match tbl.table.history(limit).await {
                Ok(history) => todo!(),
                Err(err) => unsafe {
                    callback(
                        std::ptr::null(),
                        DeltaTableError::from_error(rt, err).into_raw(),
                    )
                },
            }
        },
        move || unsafe { callback(std::ptr::null(), std::ptr::null()) },
    )
}

#[no_mangle]
pub extern "C" fn table_update_incremental(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    cancellation_token: *const CancellationToken,
    callback: TableEmptyCallback,
) {
    do_with_table_and_runtime_and_cancel(
        runtime,
        table,
        cancellation_token,
        move |rt, tbl| async move {
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
        move || unsafe { callback(std::ptr::null()) },
    );
}

#[no_mangle]
pub extern "C" fn table_load_version(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    version: i64,
    cancellation_token: *const CancellationToken,
    callback: TableEmptyCallback,
) {
    do_with_table_and_runtime_and_cancel(
        runtime,
        table,
        cancellation_token,
        move |rt, tbl| async move {
            match tbl.table.load_version(version).await {
                Ok(_) => unsafe { callback(std::ptr::null()) },
                Err(err) => {
                    let error = DeltaTableError::from_error(rt, err);
                    unsafe { callback(Box::into_raw(Box::new(error))) }
                }
            };
        },
        move || unsafe { callback(std::ptr::null()) },
    )
}

#[no_mangle]
pub extern "C" fn table_load_with_datetime(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    ts_milliseconds: i64,
    cancellation_token: *const CancellationToken,
    callback: TableEmptyCallback,
) -> bool {
    let naive_dt = match NaiveDateTime::from_timestamp_millis(ts_milliseconds) {
        Some(dt) => dt,
        None => return false,
    };

    let dt = DateTime::<Utc>::from_naive_utc_and_offset(naive_dt, Utc);
    do_with_table_and_runtime_and_cancel(
        runtime,
        table,
        cancellation_token,
        move |rt, tbl| async move {
            match tbl.table.load_with_datetime(dt).await {
                Ok(_) => unsafe { callback(std::ptr::null()) },
                Err(err) => {
                    let error = DeltaTableError::from_error(rt, err);
                    unsafe { callback(Box::into_raw(Box::new(error))) }
                }
            };
        },
        move || unsafe { callback(std::ptr::null()) },
    );
    true
}

#[no_mangle]
pub extern "C" fn table_merge(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    version: i64,
    callback: TableEmptyCallback,
) {
    todo!()
}

#[no_mangle]
pub extern "C" fn table_protocol_versions(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
) -> ProtocolResponse {
    do_with_table_and_runtime_sync(runtime, table, |rt, tbl| match tbl.table.protocol() {
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
    })
}

#[no_mangle]
pub extern "C" fn table_restore(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    version: i64,
    callback: TableEmptyCallback,
) {
    todo!()
}

#[no_mangle]
pub extern "C" fn table_update(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    version: i64,
    callback: TableEmptyCallback,
) {
    todo!()
}

/// Must free the error, but there is no need to free the SerializedBuffer
#[no_mangle]
pub extern "C" fn table_schema(runtime: *mut Runtime, table: *mut RawDeltaTable) -> GenericOrError {
    do_with_table_and_runtime_sync(
        runtime,
        table,
        move |rt, tbl| match crate::schema::get_schema(rt, &tbl.table) {
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
            Err(err) => unsafe {
                GenericOrError {
                    bytes: std::ptr::null(),
                    error: err.into_raw(),
                }
            },
        },
    )
}

#[no_mangle]
pub extern "C" fn table_checkpoint(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
    callback: TableEmptyCallback,
) {
    do_with_table_and_runtime(runtime, table, move |rt, tbl| async move {
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
    })
}

#[no_mangle]
pub extern "C" fn table_vacuum(
    runtime: *mut Runtime,
    table: *mut RawDeltaTable,
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
    do_with_table_and_runtime(runtime, table, move |rt, tbl| async move {
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
        };
    });
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
pub extern "C" fn table_version(table_handle: *mut RawDeltaTable) -> i64 {
    do_with_table(table_handle, |table| table.table.version())
}

#[no_mangle]
pub extern "C" fn table_metadata(
    runtime: *mut Runtime,
    table_handle: *mut RawDeltaTable,
) -> GenericOrError {
    do_with_table_and_runtime_sync(runtime, table_handle, |rt, table| {
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
                    format_options: Box::into_raw(Box::new(Map {
                        data: metadata.format.options.clone(),
                        disable_free: true,
                    })),
                    schema_string: CString::new(metadata.schema_string.clone())
                        .unwrap()
                        .into_raw(),
                    partition_columns: std::mem::ManuallyDrop::new(partition_columns).as_mut_ptr(),
                    partition_columns_count: metadata.partition_columns.len(),
                    created_time: metadata.created_time.unwrap_or(-1),
                    configuration: Box::into_raw(Box::new(Map {
                        data: metadata.configuration.clone(),
                        disable_free: true,
                    })),
                    release: Some(release_metadata),
                };
                GenericOrError {
                    bytes: Box::into_raw(Box::new(table_meta)) as *const c_void,
                    error: std::ptr::null(),
                }
            }
            Err(err) => GenericOrError {
                bytes: std::ptr::null(),
                error: DeltaTableError::from_error(rt, err).into_raw(),
            },
        }
    })
}

fn do_with_table_and_runtime<'a, F, Fut>(rt: *mut Runtime, table: *mut RawDeltaTable, work: F)
where
    F: FnOnce(&'a mut Runtime, &'a mut RawDeltaTable) -> Fut + Send + 'static,
    Fut: std::future::Future<Output = ()> + Send,
{
    let runtime = unsafe { &mut *rt };
    let table = unsafe { &mut *table };
    let runtime_handle = runtime.handle();
    runtime_handle.spawn(async move {
        work(runtime, table).await;
    });
}

fn do_with_table_and_runtime_and_cancel<'a, F, Fut>(
    rt: *mut Runtime,
    table: *mut RawDeltaTable,
    cancellation_token: *const CancellationToken,
    work: F,
    on_cancel: impl FnOnce() + Send + 'static,
) where
    F: FnOnce(&'a mut Runtime, &'a mut RawDeltaTable) -> Fut + Send + 'static,
    Fut: std::future::Future<Output = ()> + Send + 'static,
{
    let runtime = unsafe { &mut *rt };
    let table = unsafe { &mut *table };
    let cancel_token = unsafe { cancellation_token.as_ref() }.map(|v| v.token.clone());
    let runtime_handle = runtime.handle();
    let call_future = work(runtime, table);
    runtime_handle.spawn(async move {
        if let Some(cancel_token) = cancel_token {
            tokio::select! {
                _ = cancel_token.cancelled() => on_cancel(),
                _ = call_future => {},
            }
        } else {
            call_future.await
        }
    });
}

fn do_with_table_and_runtime_sync<'a, F, T>(
    rt: *mut Runtime,
    table: *mut RawDeltaTable,
    work: F,
) -> T
where
    F: FnOnce(&'a mut Runtime, &'a mut RawDeltaTable) -> T,
{
    let runtime = unsafe { &mut *rt };
    let table = unsafe { &mut *table };
    work(runtime, table)
}

fn do_with_table<'a, F, T>(table: *mut RawDeltaTable, work: F) -> T
where
    F: FnOnce(&'a mut RawDeltaTable) -> T,
{
    let table = unsafe { &mut *table };
    work(table)
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
    let table = DeltaTableBuilder::from_uri(table_uri)
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
