<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**  *generated with [DocToc](https://github.com/thlorenz/doctoc)*

- [Quick Start](#quick-start)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

This package is a C# wrapper around [delta-rs](https://github.com/delta-io/delta-rs/tree/rust-v0.17.0).

It uses the [tokio-rs](https://tokio.rs/) runtime to provide asynchronous behavior. This allows the usage of .NET Tasks and async/await to take advantage of the same behavior provided by the underlying rust library.
This library also takes advantage of the [Apache Arrow](https://github.com/apache/arrow/blob/main/csharp/README.md) [C Data Interface](https://arrow.apache.org/docs/format/CDataInterface.html) to minimize the amount of copying required to move data between runtimes.

![alt text](/media/images/delta-dot-net-pkg.png "Using a Rust bridge library with .NET p/invoke")

The bridge library incorporates delta-rs and [tokio-rs](https://tokio.rs/) as shown in the image below.
![alt text](/media/images/bridge-library.png "Rust bridge library with tokio")

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
dotnet test -c Release --logger "console;verbosity=detailed"
```