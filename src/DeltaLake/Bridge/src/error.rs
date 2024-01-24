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
    Kernel = 30,
    MetaDataError = 31,
    NotInitialized = 32,
}

impl DeltaTableError {
    pub(crate) fn new(_runtime: &mut Runtime, code: DeltaTableErrorCode, error: &str) -> Self {
        Self {
            code,
            error: _runtime.alloc_utf8(error),
        }
    }

    pub(crate) fn from_error(_runtime: &mut Runtime, _error: deltalake::DeltaTableError) -> Self {
        let error_string = _error.to_string();
        let code = match _error {
            deltalake::DeltaTableError::Protocol { source } => DeltaTableErrorCode::Protocol,
            deltalake::DeltaTableError::ObjectStore { source } => DeltaTableErrorCode::ObjectStore,
            deltalake::DeltaTableError::Parquet { source } => DeltaTableErrorCode::Parquet,
            deltalake::DeltaTableError::Arrow { source } => DeltaTableErrorCode::Arrow,
            deltalake::DeltaTableError::InvalidJsonLog {
                json_err,
                line,
                version,
            } => DeltaTableErrorCode::InvalidJsonLog,
            deltalake::DeltaTableError::InvalidStatsJson { json_err } => {
                DeltaTableErrorCode::InvalidStatsJson
            }
            deltalake::DeltaTableError::InvalidInvariantJson { json_err, line } => {
                DeltaTableErrorCode::InvalidInvariantJson
            }
            deltalake::DeltaTableError::InvalidVersion(_) => DeltaTableErrorCode::InvalidVersion,
            deltalake::DeltaTableError::MissingDataFile { source, path } => {
                DeltaTableErrorCode::MissingDataFile
            }
            deltalake::DeltaTableError::InvalidDateTimeString { source } => {
                DeltaTableErrorCode::InvalidDateTimeString
            }
            deltalake::DeltaTableError::InvalidData { violations } => {
                DeltaTableErrorCode::InvalidData
            }
            deltalake::DeltaTableError::NotATable(_) => DeltaTableErrorCode::NotATable,
            deltalake::DeltaTableError::NoMetadata => DeltaTableErrorCode::NoMetadata,
            deltalake::DeltaTableError::NoSchema => DeltaTableErrorCode::NoSchema,
            deltalake::DeltaTableError::LoadPartitions => DeltaTableErrorCode::LoadPartitions,
            deltalake::DeltaTableError::SchemaMismatch { msg } => {
                DeltaTableErrorCode::SchemaMismatch
            }
            deltalake::DeltaTableError::PartitionError { partition } => {
                DeltaTableErrorCode::PartitionError
            }
            deltalake::DeltaTableError::InvalidPartitionFilter { partition_filter } => {
                DeltaTableErrorCode::InvalidPartitionFilter
            }
            deltalake::DeltaTableError::ColumnsNotPartitioned {
                nonpartitioned_columns,
            } => DeltaTableErrorCode::ColumnsNotPartitioned,
            deltalake::DeltaTableError::Io { source } => DeltaTableErrorCode::Io,
            deltalake::DeltaTableError::Transaction { source } => DeltaTableErrorCode::Transaction,
            deltalake::DeltaTableError::VersionAlreadyExists(_) => {
                DeltaTableErrorCode::VersionAlreadyExists
            }
            deltalake::DeltaTableError::VersionMismatch(_, _) => {
                DeltaTableErrorCode::VersionMismatch
            }
            deltalake::DeltaTableError::MissingFeature { feature, url } => {
                DeltaTableErrorCode::MissingFeature
            }
            deltalake::DeltaTableError::InvalidTableLocation(_) => {
                DeltaTableErrorCode::InvalidTableLocation
            }
            deltalake::DeltaTableError::SerializeLogJson { json_err } => {
                DeltaTableErrorCode::SerializeLogJson
            }
            deltalake::DeltaTableError::SerializeSchemaJson { json_err } => {
                DeltaTableErrorCode::SerializeSchemaJson
            }
            deltalake::DeltaTableError::Generic(_) => DeltaTableErrorCode::Generic,
            deltalake::DeltaTableError::GenericError { source } => {
                DeltaTableErrorCode::GenericError
            }
            deltalake::DeltaTableError::Kernel { source: _ } => DeltaTableErrorCode::Kernel,
            deltalake::DeltaTableError::MetadataError(_) => DeltaTableErrorCode::MetaDataError,
            deltalake::DeltaTableError::NotInitialized => DeltaTableErrorCode::NotInitialized,
        };

        Self::new(_runtime, code, &error_string)
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
