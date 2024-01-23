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
