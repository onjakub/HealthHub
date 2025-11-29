#!/bin/bash

# Build script for React frontend integration with ASP.NET Core
# Usage: ./build-frontend.sh [Environment]

set -e

ENVIRONMENT=${1:-"Production"}

echo "Building HealthHub React frontend for $ENVIRONMENT environment..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "Error: Node.js is not installed or not in PATH"
    exit 1
fi

echo "Using Node.js version: $(node --version)"

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "Error: npm is not installed or not in PATH"
    exit 1
fi

echo "Using npm version: $(npm --version)"

# Navigate to frontend directory
cd wwwroot

# Install dependencies
echo "Installing npm dependencies..."
npm install
if [ $? -ne 0 ]; then
    echo "Error: npm install failed"
    exit 1
fi

# Build the React application
echo "Building React application..."
if [ "$ENVIRONMENT" = "Development" ]; then
    npm run build
else
    npm run build:static
fi
if [ $? -ne 0 ]; then
    echo "Error: React build failed"
    exit 1
fi

# Copy build output to ASP.NET Core wwwroot
echo "Copying build output to ASP.NET Core wwwroot..."
if [ -d "out" ]; then
    # Remove existing wwwroot content (except index.html which we'll preserve)
    if [ -d "../wwwroot" ]; then
        find ../wwwroot -mindepth 1 -maxdepth 1 ! -name 'index.html' -exec rm -rf {} +
    fi
    
    # Copy new build files
    cp -r out/* ../wwwroot/
    echo "Build files copied successfully"
else
    echo "Error: Build output directory 'out' not found"
    exit 1
fi

# Return to original directory
cd ..

echo "Frontend build completed successfully!"
echo "Next steps:"
echo "1. Build ASP.NET Core application: dotnet build"
echo "2. Run the application: dotnet run"
echo "3. Access the application at: http://localhost:5000"