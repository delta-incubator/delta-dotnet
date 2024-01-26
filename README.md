```bash
dotnet tool install --global ClangSharpPInvokeGenerator
```

```bash
ClangSharpPInvokeGenerator @src/DeltaLake/Bridge/GenerateInterop.rsp
```

OSX

```bash
ClangSharpPInvokeGenerator -I /opt/homebrew/Cellar/llvm/17.0.6/lib/clang/17/include -I /Library/Developer/CommandLineTools/SDKs/MacOSX.sdk/usr/include @src/DeltaLake/Bridge/GenerateInterop.rsp
```

flatc version `23.5.26` [doesn't properly generate verifiers](https://github.com/google/flatbuffers/issues/7899). Until this is fixed, use an earlier version or build directly from master because the change was merged.
