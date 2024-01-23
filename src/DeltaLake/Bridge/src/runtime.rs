use std::collections::HashMap;
use std::sync::Arc;

use crate::ByteArray;
use crate::DynamicArray;
use crate::Map;

#[repr(C)]
pub struct RuntimeOptions {}

#[derive(Clone)]
pub struct Runtime {
    pub(crate) runtime: Arc<tokio::runtime::Runtime>,
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
    match Runtime::new(unsafe { &*options }) {
        Ok(runtime) => RuntimeOrFail {
            runtime: Box::into_raw(Box::new(runtime)),
            fail: std::ptr::null(),
        },
        Err(err) => {
            // We have to make an empty runtime just for the failure to be
            // freeable
            let mut runtime = Runtime {
                runtime: Arc::new(
                    tokio::runtime::Builder::new_current_thread()
                        .build()
                        .unwrap(),
                ),
            };
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
        let _ = Box::from_raw(runtime);
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

impl Runtime {
    fn new(options: &RuntimeOptions) -> Result<Runtime, std::io::Error> {
        tokio::runtime::Builder::new_multi_thread()
            .build()
            .map(|rt| Runtime {
                runtime: Arc::new(rt),
            })
    }

    fn borrow_buf(&mut self) -> Vec<u8> {
        // We currently do not use a thread-safe byte pool, but if wanted, it
        // can be added here
        Vec::new()
    }

    fn return_buf(&mut self, _vec: Vec<u8>) {
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
            disable_free: false,
        }
    }
}
