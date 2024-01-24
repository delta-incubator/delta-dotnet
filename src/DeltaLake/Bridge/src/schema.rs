use deltalake::{
    arrow::{
        datatypes::Schema as ArrowSchema,
        ipc::convert::{schema_to_fb, schema_to_fb_offset, try_schema_from_flatbuffer_bytes},
    },
    parquet::arrow::ArrowWriter,
    writer::RecordBatchWriter,
};
use flatbuffers::FlatBufferBuilder;

use crate::{error::DeltaTableError, runtime::Runtime, ByteArray};

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
