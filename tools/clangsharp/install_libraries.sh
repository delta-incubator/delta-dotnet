#!/bin/bash
CLANGSHARP_VERSION=${1:-"20.1.2"}
dotnet tool install --global ClangSharpPInvokeGenerator --version 20.1.2
TARGETPLATFORM=$(uname -m)
TARGETFOLDER=${HOME}/.dotnet/tools/.store/clangsharppinvokegenerator/${CLANGSHARP_VERSION}/clangsharppinvokegenerator/${CLANGSHARP_VERSION}/tools/net8.0/any/

if [ "$TARGETPLATFORM" = "aarch64" ]; then 
FOLDER=arm64
    curl -L https://www.nuget.org/api/v2/package/libclang.runtime.linux-arm64/${CLANGSHARP_VERSION} -o /tmp/libclang.zip
    curl -L https://www.nuget.org/api/v2/package/libClangSharp.runtime.linux-arm64/${CLANGSHARP_VERSION} -o /tmp/libclangsharp.zip ;
elif ["$TARGETPLATFORM" = "x86_64"]; then
FOLDER=x64
    curl -L  https://www.nuget.org/api/v2/package/libclang.runtime.linux-x64/${CLANGSHARP_VERSION} -o /tmp/libclang.zip
    curl -L https://www.nuget.org/api/v2/package/libClangSharp.runtime.linux-x64/${CLANGSHARP_VERSION} -o /tmp/libclangsharp.zip ;
fi

unzip -od /tmp/libclang /tmp/libclang.zip
unzip -od /tmp/libclangsharp /tmp/libclangsharp.zip
cp /tmp/libclang/runtimes/linux-${FOLDER}/native/libclang.so ${TARGETFOLDER}
cp /tmp/libclangsharp/runtimes/linux-${FOLDER}/native/libClangSharp.so ${TARGETFOLDER}