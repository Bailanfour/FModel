#!/bin/bash
set -e

echo "=========================================="
echo " FModel Mobile - Android Build Script"
echo "=========================================="

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: dotnet SDK is not installed!"
    echo "Please install .NET 8 SDK first from:"
    echo "https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

echo ""
echo "1. Checking .NET SDK version..."
dotnet --version

echo ""
echo "2. Installing required workloads..."
dotnet workload install android --skip-manifest-update

echo ""
echo "3. Restoring dependencies..."
dotnet restore FModel.Maui.csproj

echo ""
echo "4. Building Android APK (Release)..."
dotnet publish FModel.Maui.csproj -f net8.0-android -c Release

echo ""
echo "5. Build completed!"
echo ""
echo "APK location:"
find bin/Release/net8.0-android -name "*.apk"

echo ""
echo "=========================================="
echo " Build successful! Ready to deploy."
echo "=========================================="