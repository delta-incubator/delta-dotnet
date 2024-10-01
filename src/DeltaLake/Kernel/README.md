# Delta Kernel FFI generation

## Quickstart

```powershell
# Initiate git modules to pull delta-kernel-rs "main" branch
#
git submodule update --init
$GIT_ROOT = git rev-parse --show-toplevel

# Switch from main, checkout specific released version of the Kernel
#
$DELTA_KERNEL_RS_TAG = Get-Content $GIT_ROOT\src\DeltaLake\Kernel\delta-kernel-rs.version.txt
git -C $GIT_ROOT\src\DeltaLake\Kernel\delta-kernel-rs checkout $DELTA_KERNEL_RS_TAG

# Build the FFI with cloud storage features
#
$FFI_PROJ_PATH = "$GIT_ROOT/src/DeltaLake/Kernel/delta-kernel-rs/ffi"
cargo build --manifest-path $FFI_PROJ_PATH/Cargo.toml --features "delta_kernel/cloud"
```