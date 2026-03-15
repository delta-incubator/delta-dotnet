use std::collections::HashMap;
use std::sync::{Arc, Once};
use crate::ByteArray;
use crate::DynamicArray;
use crate::Map;
use deltalake::datafusion::execution::runtime_env::RuntimeEnvBuilder;
use deltalake::datafusion::execution::SessionStateBuilder;
use deltalake::datafusion::prelude::{SessionConfig, SessionContext};
use deltalake::datafusion::common::DataFusionError;
use deltalake::datafusion::execution::disk_manager::{DiskManagerBuilder, DiskManagerMode};
use deltalake::datafusion::execution::memory_pool::{FairSpillPool, TrackConsumersPool};
use deltalake::delta_datafusion::DeltaSessionConfig;
use deltalake::delta_datafusion::planner::DeltaPlanner;
use crate::runtime_options::RuntimeOptions;

pub struct Runtime {
    runtime: tokio::runtime::Runtime,

    data_fusion_execution_batch_size: Option<usize>,
    data_fusion_runtime_max_spill_size: Option<usize>,
    data_fusion_runtime_temp_directory: Option<String>,
    data_fusion_runtime_max_temp_directory_size: Option<u64>,
}

/// If fail is not null, it must be manually freed when done. Runtime is always
/// present, but it should never be used if fail is present, only freed after
/// fail is freed using it.
#[repr(C)]
pub struct RuntimeOrFail {
    runtime: *mut Runtime,
    fail: *const ByteArray,
}

#[no_mangle]
pub extern "C" fn runtime_new(options: *const RuntimeOptions) -> RuntimeOrFail {
    let o = match options.is_null() {
        true => &RuntimeOptions::new(),
        false => unsafe { &*options },
    };
    match Runtime::new(o) {
        Ok(runtime) => RuntimeOrFail {
            runtime: Box::into_raw(Box::new(runtime)),
            fail: std::ptr::null(),
        },
        Err(err) => {
            // We have to make an empty runtime just for the failure to be
            // freeable
            let mut runtime = Runtime::default();
            let fail = runtime.alloc_utf8(&format!("Invalid options: {}", err));
            RuntimeOrFail {
                runtime: Box::into_raw(Box::new(runtime)),
                fail: fail.into_raw(),
            }
        }
    }
}

#[no_mangle]
pub extern "C" fn runtime_free(runtime: *mut Runtime) {
    unsafe {
        let rt = Box::from_raw(runtime);
        rt.runtime.shutdown_background();
    }
}

#[no_mangle]
pub extern "C" fn byte_array_free(runtime: *mut Runtime, bytes: *const ByteArray) {
    // Bail if freeing is disabled
    unsafe {
        if bytes.is_null() || (*bytes).disable_free {
            return;
        }
    }
    let bytes = bytes as *mut ByteArray;
    // Return vec back to core before dropping bytes
    let vec = unsafe { Vec::from_raw_parts((*bytes).data as *mut u8, (*bytes).size, (*bytes).cap) };
    // Set to null so the byte dropper doesn't try to free it
    unsafe { (*bytes).data = std::ptr::null_mut() };
    // Return only if runtime is non-null
    if !runtime.is_null() {
        let runtime = unsafe { &mut *runtime };
        runtime.return_buf(vec);
    }
    unsafe {
        let _ = Box::from_raw(bytes);
    }
}

#[no_mangle]
pub extern "C" fn map_free(_runtime: *mut Runtime, map: *const Map) {
    // Bail if freeing is disabled
    unsafe {
        if map.is_null() || (*map).disable_free {
            return;
        }
    }

    let mut_map: *mut Map = map as *mut Map;

    let _ = unsafe { Box::from_raw(mut_map) };
}

#[no_mangle]
pub extern "C" fn dynamic_array_free(runtime: *mut Runtime, array: *const DynamicArray) {
    // Bail if freeing is disabled
    unsafe {
        if array.is_null() || (*array).disable_free {
            return;
        }
    }

    let mut_array: *mut DynamicArray = array as *mut DynamicArray;

    let mut array = unsafe { Box::from_raw(mut_array) };

    // Return vec back to core before dropping bytes
    let vec = unsafe { Vec::from_raw_parts(array.data as *mut ByteArray, array.size, array.cap) };
    // Set to null so the byte dropper doesn't try to free it
    array.data = std::ptr::null_mut();

    for i in vec.iter() {
        byte_array_free(runtime, i);
    }
}

static HANDLERS: Once = Once::new();
impl Runtime {
    pub(crate) fn new(options: &RuntimeOptions) -> Result<Runtime, std::io::Error> {
        HANDLERS.call_once(|| {
            deltalake::aws::register_handlers(None);
            deltalake::azure::register_handlers(None);
            deltalake::gcp::register_handlers(None);
        });
        tokio::runtime::Builder::new_multi_thread()
            .enable_all()
            .build()
            .map(|rt| Runtime {
                runtime: rt,
                data_fusion_execution_batch_size: none_when_zero(options.data_fusion_execution_batch_size),
                data_fusion_runtime_max_spill_size: none_when_zero(options.data_fusion_runtime_max_spill_size),
                data_fusion_runtime_temp_directory: (|| unsafe { options.data_fusion_runtime_temp_directory.as_ref() }?.to_option_string())(),
                data_fusion_runtime_max_temp_directory_size: none_when_zero(options.data_fusion_runtime_max_temp_directory_size).map(|v| v as u64),
            })
    }

    pub(crate) fn create_session_context(&self, max_spill_size: Option<usize>) -> Result<SessionContext, DataFusionError> {
        let memory_pool = max_spill_size
            .or(self.data_fusion_runtime_max_spill_size)
            .map(|size| TrackConsumersPool::new(FairSpillPool::new(size), 10.try_into().unwrap()));

        let disk_manager_builder = DiskManagerBuilder::default()
            .chain_if_some(self.data_fusion_runtime_max_temp_directory_size, |b, size| b.with_max_temp_directory_size(size))
            .chain_if_some(self.data_fusion_runtime_temp_directory.clone(), |b, path| b.with_mode(DiskManagerMode::Directories(vec![path.into()])));

        let runtime_env = RuntimeEnvBuilder::default()
            .with_disk_manager_builder(disk_manager_builder)
            .chain_if_some(memory_pool, |b, pool| b.with_memory_pool(Arc::new(pool)))
            .build_arc()?;

        let config = SessionConfig::from(DeltaSessionConfig::default())
            .chain_if_some(self.data_fusion_execution_batch_size, |c, batch_size| {
                c.with_batch_size(batch_size)
            })

            // TODO: Do we actually want to keep case insensitivity? Or can we accept the breaking change?
            // Overrides the value from DeltaSessionConfig to preserve existing behaviour
            .set_bool("datafusion.sql_parser.enable_ident_normalization", true);

        let planner = DeltaPlanner::new();

        let state = SessionStateBuilder::new()
            .with_default_features()
            .with_config(config)
            .with_runtime_env(runtime_env)
            .with_query_planner(planner)
            .build();

        Ok(SessionContext::new_with_state(state))
    }

    pub fn borrow_buf(&mut self) -> Vec<u8> {
        // We currently do not use a thread-safe byte pool, but if wanted, it
        // can be added here
        Vec::new()
    }

    pub fn return_buf(&mut self, _vec: Vec<u8>) {
        // We currently do not use a thread-safe byte pool, but if wanted, it
        // can be added here
    }

    pub fn alloc_utf8(&mut self, v: &str) -> ByteArray {
        let mut buf = self.borrow_buf();
        buf.clear();
        buf.extend_from_slice(v.as_bytes());
        ByteArray::from_vec(buf)
    }

    pub fn allocate_map(&self, capacity: usize) -> Map {
        Map {
            data: HashMap::with_capacity(capacity),
            disable_free: true,
        }
    }

    pub fn handle(&self) -> tokio::runtime::Handle {
        self.runtime.handle().clone()
    }
}

impl Default for Runtime {
    fn default() -> Self {
        Runtime {
            runtime: tokio::runtime::Builder::new_multi_thread().build().unwrap(),
            data_fusion_execution_batch_size: None,
            data_fusion_runtime_max_spill_size: None,
            data_fusion_runtime_temp_directory: None,
            data_fusion_runtime_max_temp_directory_size: None,
        }
    }
}

fn none_when_zero(value: libc::size_t) -> Option<usize> {
    match value {
        0 => None,
        v => Some(v as usize),
    }
}

trait ChainingExtensions {
    fn chain_if_some<Value>(
        self,
        value: Option<Value>,
        func: impl FnOnce(Self, Value) -> Self) -> Self where Self: Sized {
        match value {
            Some(value) => func(self, value),
            None => self,
        }
    }
}

impl<T> ChainingExtensions for T {}
