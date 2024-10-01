# Delta Kernel FFI generation

## Quickstart

### Build Kernel DLL

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

### Build C# Interop

```powershell
# Copy built header
#
$GENERATED_FFI_HEADER = "$GIT_ROOT/src/DeltaLake/Kernel/delta-kernel-rs/target/ffi-headers/delta_kernel_ffi.h"
$CSHARP_FRIENDLY_FFI_HEADER = "$GIT_ROOT/src/DeltaLake/Kernel/include/delta_kernel_ffi.h"

Copy-Item -Path $GENERATED_FFI_HEADER -Destination $CSHARP_FRIENDLY_FFI_HEADER -Force

# Prepare it for ClangSharp conversion
#
Get-ChildItem "$GIT_ROOT/src/DeltaLake/Kernel/include" -Filter delta_kernel_ffi.h -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updatedContent = $content -replace '(?m)^(#if .+)$', '// $1 // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out' `
                               -replace '(?m)^(#endif)$', '// $1'
    $updatedContent = $updatedContent.TrimEnd("`r`n")
    Set-Content -Path $_.FullName -Value $updatedContent
}

# Fire ClangSharp to convert to .cs
#
cd "$GIT_ROOT/src/DeltaLake/Kernel"
ClangSharpPInvokeGenerator `
    --config <# configuration for the generator #> `
    compatible-codegen  <# bindings generated with .NET standard 2.0 compatibility #> `
    exclude-fnptr-codegen <# generated bindings for latest or preview codegen should not use function pointers #> `
    exclude-anonymous-field-helpers <# helper ref properties generated for fields in nested anonymous structs should not be generated. #> `
    exclude-com-proxies  <# com proxies, e.g. functions ending with _UserFree etc, should not have bindings generated. #> `
    --with-access-specifier "*=internal" <# kernel objects should not be exposed to end-users #> `
    --file ".\include\delta_kernel_ffi.h" <# file we want to generate bindings for #>  `
    --include-directory "C:\Program Files\LLVM\lib\clang\18\include" <# include clang headers from LLVM installation #> `
    -n "DeltaLake.Kernel.Interop" <# namespace of the interop bindings #> `
    --headerFile generated-header.txt  <# header content for all generated cs files #>  `
    --methodClassName Methods <# class name where to put methods #> `
    --libraryPath delta_kernel_ffi <# name of the DLL where code will be referenced from via PInvoke #> `
    -o .\Interop\Interop.cs <# output to self-contained file #>

# Test build
#
$xmlContent = @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

</Project>
'@
$xmlContent | Out-File -FilePath "Interop\Interop.csproj" -Encoding UTF8
dotnet build .\Interop\Interop.csproj

rm -Recurse Interop\bin, Interop\obj, .\Interop\Interop.csproj
```