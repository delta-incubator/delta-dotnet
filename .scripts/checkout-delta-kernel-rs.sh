#!/bin/bash
#
#
#       Checks out a specific version of the delta-kernel-rs git submodule, this is
#       the pinning mechanism we use during build.       
#
# ---------------------------------------------------------------------------------------
#

set -eou pipefail

GIT_ROOT=$(git rev-parse --show-toplevel)
git submodule update --init
DELTA_KERNEL_RS_TAG=$(cat "$GIT_ROOT/src/DeltaLake/Kernel/delta-kernel-rs.version.txt")
git -C "$GIT_ROOT/src/DeltaLake/Kernel/delta-kernel-rs" checkout "$DELTA_KERNEL_RS_TAG"