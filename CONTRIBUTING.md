- [Rebuilding Rust extension and interop layer](#rebuilding-rust-extension-and-interop-layer)
- [Regenerating API docs](#regenerating-api-docs)

To regen core interop from header, install
[ClangSharpPInvokeGenerator](https://github.com/dotnet/ClangSharp#generating-bindings) like:

    dotnet tool install --global ClangSharpPInvokeGenerator

Then, run:

    ClangSharpPInvokeGenerator @src/DeltaLake/Bridge/GenerateInterop.rsp

The Rust DLL is built automatically when the project is built.

```bash
dotnet tool install --global ClangSharpPInvokeGenerator
```

### Regenerating API docs

Install [docfx](https://dotnet.github.io/docfx/), then run:

    docfx src/DeltaLake.ApiDoc/docfx.json
