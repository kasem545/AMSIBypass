#!/bin/bash
echo "AMSI.fail CLI Build Script"
echo "=========================="
echo ""
echo "Building for Windows x64..."
echo ""

TARGET="win-x64"
EXE_NAME="AMSIBypass.exe"

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
else
    echo ""
    echo "========================================"
    echo "BUILD FAILED"
    echo "========================================"
    echo ""
    echo "Please ensure .NET 6.0 SDK or later is installed."
    echo "Download from: https://dotnet.microsoft.com/download"
fi