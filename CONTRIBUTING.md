- [Rebuilding Rust extension and interop layer](#rebuilding-rust-extension-and-interop-layer)
- [Rebuilding Kernel FFI and interop layer](#rebuilding-kernel-ffi-and-interop-layer)
  - [Build Kernel DLL](#build-kernel-dll)
  - [Build C# Interop](#build-c-interop)
- [Regenerating API docs](#regenerating-api-docs)

### Rebuilding Rust extension and interop layer

To regen core interop from header, install
[ClangSharpPInvokeGenerator](https://github.com/dotnet/ClangSharp#generating-bindings) like:

    dotnet tool install --global ClangSharpPInvokeGenerator

Then, run:

    ClangSharpPInvokeGenerator @src/DeltaLake/Bridge/GenerateInterop.rsp

The Rust DLL is built automatically when the project is built.

```bash
dotnet tool install --global ClangSharpPInvokeGenerator
```

### Rebuilding Kernel FFI and interop layer

[delta-kernel-rs](https://github.com/delta-incubator/delta-kernel-rs) is linked as a [git submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) in this repo. 

We first build the Kernel DLL and FFI header, and use the header to generate the C# Interop classes.

All 3 of these things are tightly coupled and must happen sequentially when you git clone this repo:

1. The git submodule must be initiated
2. The `dll` and `.h` must be built
3. The `.h` header is used to generate the interop `.cs` classes

So, since C# code in this repo will not change the Kernal DLL/Header that's generated, there's no point in continuously rebuilding the `.dll` via `csproj`.

#### Build Kernel DLL

```powershell

$GIT_ROOT = git rev-parse --show-toplevel
cd "$GIT_ROOT"

# Initiate submodule at release tag
#
git submodule update --init
$DELTA_KERNEL_RS_TAG = Get-Content $GIT_ROOT\src\DeltaLake\Kernel\delta-kernel-rs.version.txt
git -C $GIT_ROOT\src\DeltaLake\Kernel\delta-kernel-rs checkout $DELTA_KERNEL_RS_TAG

# Build the FFI with cloud storage features
#
$FFI_PROJ_PATH = "$GIT_ROOT/src/DeltaLake/Kernel/delta-kernel-rs/ffi"
cargo build --manifest-path $FFI_PROJ_PATH/Cargo.toml --features "delta_kernel/cloud"

```

#### Build C# Interop

```powershell

# Copy the built header from cargo into C# Interop project
#
$HEADER_FILE = "delta_kernel_ffi.h"
$GENERATED_FFI_HEADER = "${GIT_ROOT}/src/DeltaLake/Kernel/delta-kernel-rs/target/ffi-headers/${HEADER_FILE}"
$CSHARP_FRIENDLY_FFI_HEADER = "$GIT_ROOT/src/DeltaLake/Kernel/include/${HEADER_FILE}"

Copy-Item -Path $GENERATED_FFI_HEADER -Destination $CSHARP_FRIENDLY_FFI_HEADER -Force

# Prepare header for ClangSharp conversion.
#
# TODO: we should work out a solution with delta-kernel folks for a future release,
# on a solution for removing the build flags at the C header level, and instead pushing
# it up to a Rust feature flag.
#
Get-ChildItem "$GIT_ROOT/src/DeltaLake/Kernel/include" -Filter delta_kernel_ffi.h -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updatedContent = $content -replace '(?m)^(#if .+)$', '// $1 // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out' `
                               -replace '(?m)^(#endif)$', '// $1'
    $updatedContent = $updatedContent.TrimEnd("`r`n")
    Set-Content -Path $_.FullName -Value $updatedContent
}

ClangSharpPInvokeGenerator @src/DeltaLake/Kernel/GenerateInterop.rsp

# Post-processing script to remove Mangled entrpoints - ClangSharpPInvokeGenerator
# does not seem to have a built-in arg that does this.
#
Get-ChildItem "$GIT_ROOT/src/DeltaLake/Kernel/Interop" -Filter Interop.cs -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updatedContent = $content -replace ', EntryPoint = "[^"]+"', ''
    Set-Content -Path $_.FullName -Value $updatedContent
}
```

### Regenerating API docs

Install [docfx](https://dotnet.github.io/docfx/), then run:

    docfx src/DeltaLake.ApiDoc/docfx.json
