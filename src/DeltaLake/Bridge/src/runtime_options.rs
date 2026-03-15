use crate::ByteArrayRef;

#[repr(C)]
pub struct RuntimeOptions {
    pub data_fusion_execution_batch_size: libc::size_t,
    pub data_fusion_runtime_max_spill_size: libc::size_t,
    pub data_fusion_runtime_temp_directory: *const ByteArrayRef,
    pub data_fusion_runtime_max_temp_directory_size: libc::size_t,
}

impl RuntimeOptions {
    pub fn new() -> Self {
        RuntimeOptions{
            data_fusion_execution_batch_size: 0,
            data_fusion_runtime_max_spill_size: 0,
            data_fusion_runtime_temp_directory: std::ptr::null(),
            data_fusion_runtime_max_temp_directory_size: 0,
        }
    }
}
