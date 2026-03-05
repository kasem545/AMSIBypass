#!/bin/bash

echo "AMSI.fail CLI Build Script"
echo "=========================="
echo ""
echo "Select build target:"
echo "1) Windows x64 (default)"
echo "2) Linux x64"
echo "3) macOS x64 (Intel)"
echo "4) macOS ARM64 (Apple Silicon)"
echo ""

read -p "Enter choice [1-4] (default: 1): " choice
choice=${choice:-1}

case $choice in
    1)
        TARGET="win-x64"
        EXE_NAME="AMSIBypass.exe"
        ;;
    2)
        TARGET="linux-x64"
        EXE_NAME="AMSIBypass"
        ;;
    3)
        TARGET="osx-x64"
        EXE_NAME="AMSIBypass"
        ;;
    4)
        TARGET="osx-arm64"
        EXE_NAME="AMSIBypass"
        ;;
    *)
        echo "Invalid choice. Using default: win-x64"
        TARGET="win-x64"
        EXE_NAME="AMSIBypass.exe"
        ;;
esac

echo ""
echo "Building for: $TARGET"
echo ""

dotnet publish -c Release -r $TARGET /p:PublishSingleFile=true /p:SelfContained=true /p:PublishReadyToRun=true

if [ $? -eq 0 ]; then
    echo ""
    echo "========================================"
    echo "BUILD SUCCESSFUL"
    echo "========================================"
    echo ""
    echo "Executable location:"
    echo "bin/Release/net6.0/$TARGET/publish/$EXE_NAME"
    echo ""
    
    # Make executable on Unix-like systems
    if [[ "$TARGET" == "linux-x64" ]] || [[ "$TARGET" == "osx-x64" ]] || [[ "$TARGET" == "osx-arm64" ]]; then
        chmod +x "bin/Release/net6.0/$TARGET/publish/$EXE_NAME"
        echo "Executable permissions set."
    fi
else
    echo ""
    echo "========================================"
    echo "BUILD FAILED"
    echo "========================================"
    echo ""
    echo "Please ensure .NET 6.0 SDK or later is installed."
    echo "Download from: https://dotnet.microsoft.com/download"
fi
