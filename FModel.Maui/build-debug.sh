#!/bin/bash
set -e

echo "=========================================="
echo " FModel Mobile - Android Debug Build"
echo "=========================================="

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: dotnet SDK is not installed!"
    echo "Please install .NET 8 SDK first."
    exit 1
fi

echo "1. Restoring dependencies..."
dotnet restore FModel.Maui.csproj

echo ""
echo "2. Building Android APK (Debug)..."
dotnet build FModel.Maui.csproj -f net8.0-android -c Debug

echo ""
echo "3. Build completed!"
echo "APK location: bin/Debug/net8.0-android/*.apk"

echo ""
echo "=========================================="
echo " Debug build successful!"
echo "=========================================="