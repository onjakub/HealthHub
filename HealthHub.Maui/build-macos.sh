#!/bin/bash

# HealthHub MAUI Build Script for macOS
# This script builds and runs the MAUI app on macOS

echo "🏥 HealthHub MAUI Build Script for macOS"
echo "======================================"

# Check if .NET 10 SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 10 SDK from:"
    echo "   https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ Found .NET version: $DOTNET_VERSION"

# Check if MAUI workload is installed
if ! dotnet workload list | grep -q "maui"; then
    echo "📦 Installing MAUI workload..."
    dotnet workload install maui
    if [ $? -ne 0 ]; then
        echo "❌ Failed to install MAUI workload"
        exit 1
    fi
    echo "✅ MAUI workload installed successfully"
else
    echo "✅ MAUI workload already installed"
fi

# Navigate to project directory
cd "$(dirname "$0")"

echo "📦 Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore packages"
    exit 1
fi

echo "🔨 Building for macOS..."
dotnet build -f net10.0-maccatalyst --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "🚀 Running HealthHub MAUI app..."
echo "Make sure your HealthHub backend is running on http://localhost:5000/graphql/"
echo ""

# Check if backend is running
if curl -s http://localhost:5000/graphql -X POST -H "Content-Type: application/json" -d '{"query":"{__schema{types{name}}}"}' > /dev/null 2>&1; then
    echo "✅ HealthHub backend detected on localhost:5000"
else
    echo "⚠️  Warning: HealthHub backend not detected on localhost:5000"
    echo "   The app will attempt to connect but may show connection errors."
    echo "   Start your HealthHub backend first with:"
    echo "   cd HealthHub && dotnet run"
fi

echo ""
dotnet run -f net10.0-maccatalyst

echo ""
echo "👋 HealthHub MAUI app closed. Thank you for using HealthHub!"