#!/bin/bash
set -e

echo "=========================================="
echo " FModel Mobile - Android Build Script"
echo "=========================================="

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: dotnet SDK is not installed!"
    echo "Please install .NET 8 SDK first."
    exit 1
fi

echo "1. Restoring dependencies..."
dotnet restore FModel.Maui.csproj

echo ""
echo "2. Building Android APK (Release)..."
dotnet publish FModel.Maui.csproj -f net8.0-android -c Release /p:AndroidSigningKeyStore=./keystore.keystore /p:AndroidSigningKeyAlias=mykey /p:AndroidSigningKeyPass=password /p:AndroidSigningStorePass=password

echo ""
echo "3. Build completed!"
echo "APK location: bin/Release/net8.0-android/publish/*.apk"

echo ""
echo "=========================================="
echo " Build successful! Ready to deploy."
echo "=========================================="