# Delta Kernel FFI generation

## Quickstart

```powershell
$GIT_ROOT = git rev-parse --show-toplevel
$FFI_PROJ_PATH = "$GIT_ROOT/src/DeltaLake/Kernel/delta-kernel-rs/ffi"

pushd $FFI_PROJ_PATH
cargo build --manifest-path ./Cargo.toml --features "delta_kernel/cloud"
popd
```