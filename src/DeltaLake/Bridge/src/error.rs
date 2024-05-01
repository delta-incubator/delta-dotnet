use deltalake::datafusion::sql::sqlparser::parser::ParserError;

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
    OperationCanceled = 33,
    DataFusion = 34,
    SqlParser = 35,
    InvalidTimestamp = 36,
}

impl DeltaTableError {
    pub(crate) fn new(_runtime: &mut Runtime, code: DeltaTableErrorCode, error: &str) -> Self {
        Self {
            code,
            error: _runtime.alloc_utf8(error),
        }
    }

    pub(crate) fn from_parser_error(runtime: &mut Runtime, error: ParserError) -> Self {
        Self::new(runtime, DeltaTableErrorCode::SqlParser, &error.to_string())
    }

    pub(crate) fn from_error(_runtime: &mut Runtime, _error: deltalake::DeltaTableError) -> Self {
        let error_string = _error.to_string();
        let code = match _error {
            deltalake::DeltaTableError::Protocol { .. } => DeltaTableErrorCode::Protocol,
            deltalake::DeltaTableError::ObjectStore { .. } => DeltaTableErrorCode::ObjectStore,
            deltalake::DeltaTableError::Parquet { .. } => DeltaTableErrorCode::Parquet,
            deltalake::DeltaTableError::Arrow { .. } => DeltaTableErrorCode::Arrow,
            deltalake::DeltaTableError::InvalidJsonLog { .. } => {
                DeltaTableErrorCode::InvalidJsonLog
            }
            deltalake::DeltaTableError::InvalidStatsJson { .. } => {
                DeltaTableErrorCode::InvalidStatsJson
            }
            deltalake::DeltaTableError::InvalidInvariantJson { .. } => {
                DeltaTableErrorCode::InvalidInvariantJson
            }
            deltalake::DeltaTableError::InvalidVersion(_) => DeltaTableErrorCode::InvalidVersion,
            deltalake::DeltaTableError::MissingDataFile { .. } => {
                DeltaTableErrorCode::MissingDataFile
            }
            deltalake::DeltaTableError::InvalidDateTimeString { .. } => {
                DeltaTableErrorCode::InvalidDateTimeString
            }
            deltalake::DeltaTableError::InvalidData { .. } => DeltaTableErrorCode::InvalidData,
            deltalake::DeltaTableError::NotATable(_) => DeltaTableErrorCode::NotATable,
            deltalake::DeltaTableError::NoMetadata => DeltaTableErrorCode::NoMetadata,
            deltalake::DeltaTableError::NoSchema => DeltaTableErrorCode::NoSchema,
            deltalake::DeltaTableError::LoadPartitions => DeltaTableErrorCode::LoadPartitions,
            deltalake::DeltaTableError::SchemaMismatch { .. } => {
                DeltaTableErrorCode::SchemaMismatch
            }
            deltalake::DeltaTableError::PartitionError { .. } => {
                DeltaTableErrorCode::PartitionError
            }
            deltalake::DeltaTableError::InvalidPartitionFilter { .. } => {
                DeltaTableErrorCode::InvalidPartitionFilter
            }
            deltalake::DeltaTableError::ColumnsNotPartitioned { .. } => {
                DeltaTableErrorCode::ColumnsNotPartitioned
            }
            deltalake::DeltaTableError::Io { .. } => DeltaTableErrorCode::Io,
            deltalake::DeltaTableError::Transaction { .. } => DeltaTableErrorCode::Transaction,
            deltalake::DeltaTableError::VersionAlreadyExists(_) => {
                DeltaTableErrorCode::VersionAlreadyExists
            }
            deltalake::DeltaTableError::VersionMismatch(_, _) => {
                DeltaTableErrorCode::VersionMismatch
            }
            deltalake::DeltaTableError::MissingFeature { .. } => {
                DeltaTableErrorCode::MissingFeature
            }
            deltalake::DeltaTableError::InvalidTableLocation(_) => {
                DeltaTableErrorCode::InvalidTableLocation
            }
            deltalake::DeltaTableError::SerializeLogJson { .. } => {
                DeltaTableErrorCode::SerializeLogJson
            }
            deltalake::DeltaTableError::SerializeSchemaJson { .. } => {
                DeltaTableErrorCode::SerializeSchemaJson
            }
            deltalake::DeltaTableError::Generic(_) => DeltaTableErrorCode::Generic,
            deltalake::DeltaTableError::GenericError { .. } => DeltaTableErrorCode::GenericError,
            deltalake::DeltaTableError::Kernel { .. } => DeltaTableErrorCode::Kernel,
            deltalake::DeltaTableError::MetadataError(_) => DeltaTableErrorCode::MetaDataError,
            deltalake::DeltaTableError::NotInitialized => DeltaTableErrorCode::NotInitialized,
            deltalake::DeltaTableError::CommitValidation { source: _ } => {
                DeltaTableErrorCode::InvalidData
            }
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
