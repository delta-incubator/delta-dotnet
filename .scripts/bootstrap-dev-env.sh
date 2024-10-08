#!/bin/bash
#
#
#       Sets up a dev env with all pre-reqs. This script is idempotent, it will
#       only attempt to install dependencies, if not exists.   
#
# ---------------------------------------------------------------------------------------
#

set -e
set -m

echo ""
echo "┌───────────────────────────────────┐"
echo "│ Checking for package dependencies │"
echo "└───────────────────────────────────┘"
echo ""

PACKAGES=""
if ! dpkg -l | grep -q build-essential; then PACKAGES="build-essential"; fi
if ! dpkg -l | grep -q pkg-config; then PACKAGES="$PACKAGES pkg-config"; fi
if ! command -v openssl &> /dev/null; then PACKAGES="$PACKAGES openssl libssl-dev"; fi
if [ ! -z "$PACKAGES" ]; then
    echo "Packages $PACKAGES not found - installing..."
    sudo apt-get update 2>&1 > /dev/null
    sudo DEBIAN_FRONTEND=noninteractive apt-get install -y $PACKAGES 2>&1 > /dev/null
fi

echo ""
echo "┌────────────────────────────────────┐"
echo "│ Checking for language dependencies │"
echo "└────────────────────────────────────┘"
echo ""

DOTNET_VERSION=8.0

if ! command -v dotnet &> /dev/null; then
    echo "dotnet not found - installing..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x ./dotnet-install.sh
    ./dotnet-install.sh --channel 8.0
    rm dotnet-install.sh

    LINES="export DOTNET_ROOT=\$HOME/.dotnet\nexport PATH=\$PATH:\$DOTNET_ROOT:\$DOTNET_ROOT/tools"
    echo -e "$LINES" >> ~/.bashrc
    source ~/.bashrc
fi

if ! command -v cargo &> /dev/null; then
    echo "cargo not found - installing..."
    curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
    source ~/.bashrc
fi

echo ""
echo "┌────────────────────────┐"
echo "│ Checking for CLI tools │"
echo "└────────────────────────┘"
echo ""

if ! command -v az &> /dev/null; then
    echo "az not found - installing..."
    curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
fi

echo ""
echo "┌──────────┐"
echo "│ Versions │"
echo "└──────────┘"
echo ""

echo "Dotnet: $(dotnet --version)"
echo "Cargo (Rust): $(cargo version)"
echo "Azure CLI: $(az version)"