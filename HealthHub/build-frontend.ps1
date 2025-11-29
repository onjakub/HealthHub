# PowerShell script to build React frontend and integrate with ASP.NET Core
param(
    [string]$Configuration = "Release"
)

Write-Host "Building HealthHub React frontend integration..." -ForegroundColor Green

# Change to React frontend directory
Set-Location "wwwroot"

# Install dependencies if node_modules doesn't exist
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing React dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install dependencies" -ForegroundColor Red
        exit 1
    }
}

# Build React frontend for production
Write-Host "Building React frontend..." -ForegroundColor Yellow
npm run build:static
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build React frontend" -ForegroundColor Red
    exit 1
}

# Return to project root
Set-Location ".."

# Build ASP.NET Core project
Write-Host "Building ASP.NET Core project..." -ForegroundColor Yellow
dotnet build --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build ASP.NET Core project" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Frontend files are available in the wwwroot directory" -ForegroundColor Cyan
Write-Host "Run the application with: dotnet run" -ForegroundColor Cyan