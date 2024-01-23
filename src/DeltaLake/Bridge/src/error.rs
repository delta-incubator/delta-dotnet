use crate::{runtime::Runtime, ByteArray};

#[repr(C)]
pub struct DeltaTableError {
    code: DeltaTableErrorCode,
    error: ByteArray,
}

#[repr(C)]
pub enum DeltaTableErrorCode {
    Utf8 = 0,
    Protocol = 1,
    ObjectStore = 2,
    Parquet = 3,
    Arrow = 4,
    InvalidJsonLog = 5,
    InvalidStatsJson = 6,
    InvalidInvariantJson = 7,
    InvalidVersion = 8,
    MissingDataFile = 9,
    InvalidDateTimeString = 10,
    InvalidData = 11,
    NotATable = 12,
    NoMetadata = 13,
    NoSchema = 14,
    LoadPartitions = 15,
    SchemaMismatch = 16,
    PartitionError = 17,
    InvalidPartitionFilter = 18,
    ColumnsNotPartitioned = 19,
    Io = 20,
    Transaction = 21,
    VersionAlreadyExists = 22,
    VersionMismatch = 23,
    MissingFeature = 24,
    InvalidTableLocation = 25,
    SerializeLogJson = 26,
    SerializeSchemaJson = 27,
    Generic = 28,
    GenericError = 29,
}

impl DeltaTableError {
    pub(crate) fn new(_runtime: &Runtime, code: DeltaTableErrorCode, error: ByteArray) -> Self {
        Self { code, error }
    }

    pub(crate) fn from_error(_runtime: &Runtime, _error: deltalake::DeltaTableError) -> Self {
        todo!("need to implement this")
    }

    pub(crate) fn into_raw(self) -> *mut DeltaTableError {
        Box::into_raw(Box::new(self))
    }
}

#[no_mangle]
pub extern "C" fn error_free(_runtime: *mut Runtime, error: *const DeltaTableError) {
    if error.is_null() {
        return;
    }

    let error = error as *mut DeltaTableError;
    unsafe {
        let _ = Box::from_raw(error);
    }
}
