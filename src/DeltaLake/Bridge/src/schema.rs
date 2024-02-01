use deltalake::arrow::{
    datatypes::Schema as ArrowSchema,
    ipc::convert::{schema_to_fb_offset, try_schema_from_flatbuffer_bytes},
};
use flatbuffers::FlatBufferBuilder;

use crate::{error::DeltaTableError, runtime::Runtime, ByteArray, ByteArrayRef};

#[repr(C)]
pub enum PartitionFilterBinaryOp {
    Equal = 0,
    /// The partition value with the not equal operator
    NotEqual = 1,
    /// The partition value with the greater than operator
    GreaterThan = 2,
    /// The partition value with the greater than or equal operator
    GreaterThanOrEqual = 3,
    /// The partition value with the less than operator
    LessThan = 4,
    /// The partition value with the less than or equal operator
    LessThanOrEqual = 5,
}

#[repr(C)]
pub enum PartitionFilterSetOp {
    /// The partition values with the in operator
    In = 0,
    /// The partition values with the not in operator
    NotIn = 1,
}
pub struct PartitionFilterList {
    pub(crate) filters: Vec<deltalake::PartitionFilter>,
    disable_free: bool,
}

#[no_mangle]
pub extern "C" fn partition_filter_list_new(capacity: usize) -> *mut PartitionFilterList {
    Box::into_raw(Box::new(PartitionFilterList {
        filters: Vec::with_capacity(capacity),
        disable_free: true,
    }))
}

#[no_mangle]
pub extern "C" fn partition_filter_list_add_binary(
    list: *mut PartitionFilterList,
    key: *const ByteArrayRef,
    op: PartitionFilterBinaryOp,
    value: *const ByteArrayRef,
) -> bool {
    unimplemented!()
}

#[no_mangle]
pub extern "C" fn partition_filter_list_add_set(
    list: *mut PartitionFilterList,
    key: *const ByteArrayRef,
    op: PartitionFilterBinaryOp,
    value: *const ByteArrayRef,
    value_count: usize,
) -> bool {
    unimplemented!()
}

#[no_mangle]
pub extern "C" fn partition_filter_list_free(list: *mut PartitionFilterList) {
    unsafe {
        let _ = Box::from_raw(list);
    }
}

pub(crate) fn get_schema(
    runtime: &mut crate::Runtime,
    table: &deltalake::DeltaTable,
) -> Result<ArrowSchema, DeltaTableError> {
    let delta_schema = table
        .get_schema()
        .map_err(|e| DeltaTableError::from_error(runtime, e))?;
    let arrow_schema = ArrowSchema::try_from(delta_schema).map_err(|e| {
        DeltaTableError::new(
            runtime,
            crate::error::DeltaTableErrorCode::Arrow,
            &e.to_string(),
        )
    })?;
    Ok(arrow_schema)
}

pub(crate) fn serialize_schema(runtime: &mut Runtime, schema: &ArrowSchema) -> (Vec<u8>, usize) {
    let buffer = runtime.borrow_buf();
    let mut fbb = FlatBufferBuilder::from_vec(buffer);
    let root = schema_to_fb_offset(&mut fbb, schema);
    fbb.finish(root, None);

    let (buffer, offset) = fbb.collapse();
    (buffer, offset)
}

pub(crate) fn deserialize_schema(
    runtime: &mut Runtime,
    bytes: &[u8],
) -> Result<ArrowSchema, DeltaTableError> {
    try_schema_from_flatbuffer_bytes(bytes).map_err(|err| {
        DeltaTableError::new(
            runtime,
            crate::error::DeltaTableErrorCode::Arrow,
            &err.to_string(),
        )
    })
}
