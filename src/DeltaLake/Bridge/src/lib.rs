#![allow(
    // We choose to have narrow "unsafe" blocks instead of marking entire
    // functions as unsafe. Even the example in clippy's docs at
    // https://rust-lang.github.io/rust-clippy/master/index.html#not_unsafe_ptr_arg_deref
    // cause a rustc warning for unnecessary inner-unsafe when marked on fn.
    // This check only applies to "pub" functions which are all exposed via C
    // API.
    clippy::not_unsafe_ptr_arg_deref,
)]

pub mod error;
pub mod runtime;
pub mod schema;
#[macro_use]
mod sql;
pub mod table;
use std::{collections::HashMap, mem::ManuallyDrop};

use runtime::Runtime;

#[repr(C)]
pub struct Dictionary {
    values: *mut *mut KeyValuePair,
    length: usize,
    capacity: usize,
}

#[repr(C)]
pub struct KeyValuePair {
    key: *mut u8,
    key_length: usize,
    key_capacity: usize,
    value: *mut u8,
    value_length: usize,
    value_capacity: usize,
}

pub type KeyNullableValuePair = KeyValuePair;

impl KeyValuePair {
    pub(crate) fn from_optional_hash_map(
        input: HashMap<String, Option<String>>,
    ) -> *mut *mut KeyNullableValuePair {
        let mapped = input
            .into_iter()
            .map(|(key, value)| {
                let mut key = ManuallyDrop::new(key);
                let (value, value_length, value_capacity) = match value {
                    Some(value) => {
                        let mut value = ManuallyDrop::new(value);
                        (value.as_mut_ptr(), value.len(), value.capacity())
                    }
                    None => (std::ptr::null_mut(), 0, 0),
                };

                Box::into_raw(Box::new(KeyNullableValuePair {
                    key: key.as_mut_ptr(),
                    key_length: key.len(),
                    key_capacity: key.capacity(),
                    value,
                    value_length,
                    value_capacity,
                }))
            })
            .collect::<Box<_>>();
        ManuallyDrop::new(mapped).as_mut_ptr()
    }

    #[allow(dead_code)]
    pub(crate) fn from_hash_map(input: HashMap<String, String>) -> *mut *mut Self {
        ManuallyDrop::new(
            input
                .into_iter()
                .map(|(key, value)| {
                    let (mut key, mut value) = (ManuallyDrop::new(key), ManuallyDrop::new(value));
                    Box::into_raw(Box::new(KeyNullableValuePair {
                        key: key.as_mut_ptr(),
                        key_length: key.len(),
                        key_capacity: key.capacity(),
                        value: value.as_mut_ptr(),
                        value_length: value.len(),
                        value_capacity: value.capacity(),
                    }))
                })
                .collect::<Box<_>>(),
        )
        .as_mut_ptr()
    }
}

impl Drop for KeyValuePair {
    fn drop(&mut self) {
        unsafe {
            let _ = String::from_raw_parts(self.key, self.key_length, self.key_length);
            if !self.value.is_null() {
                let _ = String::from_raw_parts(self.value, self.value_length, self.value_length);
            }
        }
    }
}
pub struct Map {
    data: HashMap<String, Option<String>>,
    disable_free: bool,
}

impl Map {
    pub(crate) unsafe fn into_hash_map(source: *mut Map) -> Option<HashMap<String, String>> {
        match source.is_null() {
            true => None,
            false => {
                let map = Box::from_raw(source);
                Some(
                    map.data
                        .into_iter()
                        .map(|(k, v)| (k, v.unwrap_or_default()))
                        .collect(),
                )
            }
        }
    }

    pub(crate) unsafe fn into_map(source: *mut Map) -> Option<HashMap<String, Option<String>>> {
        match source.is_null() {
            true => None,
            false => {
                let map = Box::from_raw(source);
                Some(map.data)
            }
        }
    }
}

#[no_mangle]
pub extern "C" fn map_new(runtime: *const Runtime, capacity: usize) -> *const Map {
    let rt = unsafe { &*runtime };
    Box::into_raw(Box::new(rt.allocate_map(capacity)))
}

#[no_mangle]
pub extern "C" fn map_add(
    map: *mut Map,
    key: *const ByteArrayRef,
    value: *const ByteArrayRef,
) -> bool {
    if map.is_null() {
        return false;
    }
    let key = unsafe { &*key };
    let map = unsafe { &mut *map };
    if value.is_null() {
        map.data.insert(key.to_owned_string(), None);
    } else {
        let value = unsafe { &*value };
        map.data
            .insert(key.to_owned_string(), Some(value.to_owned_string()));
    }

    true
}

#[repr(C)]
pub struct ByteArrayRef {
    data: *const u8,
    size: libc::size_t,
}

impl ByteArrayRef {
    fn to_slice(&self) -> &[u8] {
        unsafe { std::slice::from_raw_parts(self.data, self.size) }
    }

    fn to_str(&self) -> &str {
        // Trust caller to send UTF8. Even if we did do a checked call here with
        // error, the caller can still have a bad pointer or something else
        // wrong. Therefore we trust the caller implicitly.
        unsafe { std::str::from_utf8_unchecked(std::slice::from_raw_parts(self.data, self.size)) }
    }

    fn to_owned_string(&self) -> String {
        self.to_str().to_string()
    }

    #[allow(dead_code)]
    fn to_option_slice(&self) -> Option<&[u8]> {
        if self.size == 0 {
            None
        } else {
            Some(self.to_slice())
        }
    }

    fn to_option_str(&self) -> Option<&str> {
        if self.size == 0 {
            None
        } else {
            Some(self.to_str())
        }
    }

    fn to_option_string(&self) -> Option<String> {
        self.to_option_str().map(str::to_string)
    }
}

#[repr(C)]
pub struct ArrayRef {
    data: *const ByteArrayRef,
    size: libc::size_t,
    /// For internal use only.
    cap: libc::size_t,
    /// For internal use only.
    disable_free: bool,
}

#[repr(C)]
pub struct DynamicArray {
    data: *const ByteArray,
    size: libc::size_t,
    /// For internal use only.
    cap: libc::size_t,
    /// For internal use only.
    disable_free: bool,
}

impl DynamicArray {
    pub(crate) fn from_vec_string(input: Vec<String>) -> Self {
        let data: Vec<ByteArray> = input
            .into_iter()
            .map(|path| ByteArray::from_utf8(path.to_string()))
            .collect();
        DynamicArray {
            data: data.as_ptr(),
            size: data.len(),
            cap: data.capacity(),
            disable_free: false,
        }
    }
}

#[repr(C)]
pub struct ByteArray {
    data: *const u8,
    size: libc::size_t,
    /// For internal use only.
    cap: libc::size_t,
    /// For internal use only.
    disable_free: bool,
}

impl ByteArray {
    fn from_utf8(str: String) -> ByteArray {
        ByteArray::from_vec(str.into_bytes())
    }

    fn from_vec(vec: Vec<u8>) -> ByteArray {
        // Mimics Vec::into_raw_parts that's only available in nightly
        let mut vec = std::mem::ManuallyDrop::new(vec);
        ByteArray {
            data: vec.as_mut_ptr(),
            size: vec.len(),
            cap: vec.capacity(),
            disable_free: false,
        }
    }

    #[allow(dead_code)]
    fn from_vec_disable_free(vec: Vec<u8>) -> ByteArray {
        let mut b = ByteArray::from_vec(vec);
        b.disable_free = true;
        b
    }

    fn into_raw(self) -> *mut ByteArray {
        Box::into_raw(Box::new(self))
    }
}

// Required because these instances are used by lazy_static and raw pointers are
// not usually safe for send/sync.
unsafe impl Send for ByteArray {}
unsafe impl Sync for ByteArray {}

impl Drop for ByteArray {
    fn drop(&mut self) {
        // In cases where freeing is disabled (or technically some other
        // drop-but-not-freed situation though we don't expect any), the bytes
        // remain non-null so we re-own them here. See "byte_array_free" in
        // runtime.rs.
        if !self.data.is_null() {
            unsafe { Vec::from_raw_parts(self.data as *mut u8, self.size, self.cap) };
        }
    }
}

pub struct CancellationToken {
    token: tokio_util::sync::CancellationToken,
}

#[no_mangle]
pub extern "C" fn cancellation_token_new() -> *mut CancellationToken {
    Box::into_raw(Box::new(CancellationToken {
        token: tokio_util::sync::CancellationToken::new(),
    }))
}

#[no_mangle]
pub extern "C" fn cancellation_token_cancel(token: *mut CancellationToken) {
    let token = unsafe { &*token };
    token.token.cancel();
}

#[no_mangle]
pub extern "C" fn cancellation_token_free(token: *mut CancellationToken) {
    unsafe {
        let _ = Box::from_raw(token);
    }
}
