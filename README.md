<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**  *generated with [DocToc](https://github.com/thlorenz/doctoc)*

- [Quick Start](#quick-start)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

This package is a C# wrapper around [delta-rs](https://github.com/delta-io/delta-rs/tree/rust-v0.17.0) and implements the FFI ([Foreign Function Interface](https://en.wikipedia.org/wiki/Foreign_function_interface)) for [delta-kernel-rs](https://github.com/delta-incubator/delta-kernel-rs).

It uses the [tokio-rs](https://tokio.rs/) runtime to provide asynchronous behavior. This allows the usage of .NET Tasks and async/await to take advantage of the same behavior provided by the underlying rust library.
This library also takes advantage of the [Apache Arrow](https://github.com/apache/arrow/blob/main/csharp/README.md) [C Data Interface](https://arrow.apache.org/docs/format/CDataInterface.html) to minimize the amount of copying required to move data between runtimes.

![alt text](/media/images/delta-dot-net-pkg.png "Using a Rust bridge and Kernel library with .NET p/invoke")

The bridge library incorporates delta-rs, delta-kernel-rs and [tokio-rs](https://tokio.rs/) as shown in the image below.

> The [Delta Kernel FFI](https://delta.io/blog/delta-kernel/) integration is pinned to releases of the Kernel, as new support is added to Kernel, support in C# will be easily extended as a fast follower.
> The library also benefits from [Apache DataFusion](https://datafusion.apache.org/) SQL support, which is [part of delta-rs](https://delta-io.github.io/delta-rs/integrations/delta-lake-datafusion/)

![alt text](/media/images/bridge-library.png "Rust bridge library with tokio and Kernel FFI")

NOTE: On unix systems, there is the possibility of a stack overflow due to small stack sizes for the .NET framework. The default size should correspond to `ulimit -s`, but we can override this by setting the environment variable `DOTNET_DefaultStackSize` to a hexadecimal number of bytes. The unit tests use `180000`.

## Quick Start

This section explains how build and run Delta Dotnet locally from scratch on a fresh Linux Machine. 
> If you're on windows, [WSL](https://learn.microsoft.com/en-us/windows/wsl/install) provides a great way to spinup Linux VMs rapidly for testing.

Step 1: Clone this repo, and initiate the `delta-kernel-rs` submodule:

```bash
cd ~/
git clone https://github.com/delta-incubator/delta-dotnet.git
cd delta-dotnet

GIT_ROOT=$(git rev-parse --show-toplevel)

chmod +x ${GIT_ROOT}/.scripts/checkout-delta-kernel-rs.sh && ${GIT_ROOT}/.scripts/checkout-delta-kernel-rs.sh
```

Step 2: Install dev dependencies:

```bash
chmod +x ${GIT_ROOT}/.scripts/bootstrap-dev-env.sh && ${GIT_ROOT}/.scripts/bootstrap-dev-env.sh
source ~/.bashrc
```

Step 3: Run the unit tests, which also builds the Rust binaries:

```bash
dotnet test -c Debug --logger "console;verbosity=detailed"
```

> All tests should run green ✅

Step 4: Run the example project, which writes delta tables to Azure Storage and reads it back as a [DataFrame](https://learn.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/getting-started-dataframe):

```bash
az login --use-device-code
dotnet run --project ${GIT_ROOT}/examples/local/local.csproj -- "abfss://container@storageaccount.dfs.core.windows.net/a/b/demo-table" "20"

# Table root path: abfss://container@storageaccount.dfs.core.windows.net/a/b/demo-table
# Table partition columns: colHostTest
# Table version before transaction: 0
# Table version after transaction: 1
# Table: 3 columns by 20 rows
#
# colStringTest | colIntegerTest | colHostTest
# --------------|----------------|-------------
# yUnmZGYdeY    | 568935462      | Desktop    
# PfAc0LT7ZQ    | 683443233      | Desktop    
# Tg5xBKy3N3    | 897232561      | Desktop    
# pGMIGfmtRI    | 415054756      | Desktop    
# ormadktrAp    | 1114613767     | Desktop    
# OUYjEQpMxs    | 1159354204     | Desktop    
# J3mECDWmoc    | 1059212885     | Desktop    
# KSlaMMYiaT    | 525569187      | Desktop    
# KA5ZiD1ZTq    | 274902831      | Desktop    
# GRcqxFnF87    | 727541254      | Desktop    
# ZjFFAx6LZt    | 704318687      | Desktop    
# GUBZOqmRU9    | 62468794       | Desktop    
# 6sZdWl5xeV    | 439777191      | Desktop    
# 3oDrKCMZ9c    | 330135342      | Desktop    
# Goiltladv2    | 350043751      | Desktop    
# ts0v56YIDN    | 1381983219     | Desktop    
# 1dyWhaM7SU    | 1935291772     | Desktop    
# 7lBUQeMdeQ    | 1339188314     | Desktop    
# QFfm4Y7Q3w    | 498941470      | Desktop    
# 9yRU65qBy1    | 1105953095     | Desktop  
```