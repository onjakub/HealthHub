#!/bin/bash

# Bash script to build React frontend and integrate with ASP.NET Core

CONFIGURATION=${1:-Release}

echo "Building HealthHub React frontend integration..."

# Change to React frontend directory
cd wwwroot

# Install dependencies if node_modules doesn't exist
if [ ! -d "node_modules" ]; then
    echo "Installing React dependencies..."
    npm install
    if [ $? -ne 0 ]; then
        echo "Failed to install dependencies"
        exit 1
    fi
fi

# Build React frontend for production
echo "Building React frontend..."
npm run build:static
if [ $? -ne 0 ]; then
    echo "Failed to build React frontend"
    exit 1
fi

# Return to project root
cd ..

# Build ASP.NET Core project
echo "Building ASP.NET Core project..."
dotnet build --configuration $CONFIGURATION
if [ $? -ne 0 ]; then
    echo "Failed to build ASP.NET Core project"
    exit 1
fi

echo "Build completed successfully!"
echo "Frontend files are available in the wwwroot directory"
echo "Run the application with: dotnet run"