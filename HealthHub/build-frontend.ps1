<#
.SYNOPSIS
    Build script for React frontend integration with ASP.NET Core
.DESCRIPTION
    This script builds the React frontend and copies the static files to the ASP.NET Core wwwroot directory
.PARAMETER Environment
    The build environment (Development, Production)
#>

param(
    [string]$Environment = "Production"
)

Write-Host "Building HealthHub React frontend for $Environment environment..." -ForegroundColor Green

# Check if Node.js is installed
$nodeVersion = node --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Node.js is not installed or not in PATH" -ForegroundColor Red
    exit 1
}
Write-Host "Using Node.js version: $nodeVersion" -ForegroundColor Yellow

# Check if npm is installed
$npmVersion = npm --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: npm is not installed or not in PATH" -ForegroundColor Red
    exit 1
}
Write-Host "Using npm version: $npmVersion" -ForegroundColor Yellow

# Navigate to frontend directory
Set-Location "wwwroot"

# Install dependencies
Write-Host "Installing npm dependencies..." -ForegroundColor Cyan
npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: npm install failed" -ForegroundColor Red
    exit 1
}

# Build the React application
Write-Host "Building React application..." -ForegroundColor Cyan
if ($Environment -eq "Development") {
    npm run build
} else {
    npm run build:static
}
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: React build failed" -ForegroundColor Red
    exit 1
}

# Copy build output to ASP.NET Core wwwroot
Write-Host "Copying build output to ASP.NET Core wwwroot..." -ForegroundColor Cyan
if (Test-Path "out") {
    # Remove existing wwwroot content (except index.html which we'll preserve)
    if (Test-Path "..\wwwroot") {
        Get-ChildItem "..\wwwroot" -Exclude "index.html" | Remove-Item -Recurse -Force
    }
    
    # Copy new build files
    Copy-Item -Path "out\*" -Destination "..\wwwroot\" -Recurse -Force
    Write-Host "Build files copied successfully" -ForegroundColor Green
} else {
    Write-Host "Error: Build output directory 'out' not found" -ForegroundColor Red
    exit 1
}

# Return to original directory
Set-Location ".."

Write-Host "Frontend build completed successfully!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Build ASP.NET Core application: dotnet build" -ForegroundColor White
Write-Host "2. Run the application: dotnet run" -ForegroundColor White
Write-Host "3. Access the application at: http://localhost:5000" -ForegroundColor White