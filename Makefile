generate-kernel-bindings:
	cargo build --manifest-path src/DeltaLake/Kernel/delta-kernel-rs/ffi/Cargo.toml --no-default-features --features "default-engine-rustls"
	cp src/DeltaLake/Kernel/delta-kernel-rs/target/ffi-headers/delta_kernel_ffi.h src/DeltaLake/Kernel/include/
	ClangSharpPInvokeGenerator -I "$$(llvm-config --libdir)/clang/20/include" -D DEFINE_DEFAULT_ENGINE_BASE=1 @src/DeltaLake/Kernel/GenerateInterop.rsp

generate-bridge-bindings:
	cargo build --manifest-path src/DeltaLake/Bridge/Cargo.toml
	ClangSharpPInvokeGenerator -I "$$(llvm-config --libdir)/clang/20/include" @src/DeltaLake/Bridge/GenerateInterop.rsp

generate-bindings: generate-bridge-bindings generate-kernel-bindings