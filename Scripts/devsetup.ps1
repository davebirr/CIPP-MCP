# Dev setup script for CIPP-MCP Azure Functions MCP server
# Checks for required tools and installs missing dependencies

# Ensure script runs from project root
Set-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Definition)
Set-Location ..

Write-Host "[CIPP-MCP] Checking development environment..." -ForegroundColor Cyan

# 0. Check if project targets .NET 8 and initialize if needed
$csprojPath = Join-Path (Get-Location) 'CIPP-MCP.csproj'
if (Test-Path $csprojPath) {
    $csprojContent = Get-Content $csprojPath -Raw
    if ($csprojContent -notmatch '<TargetFramework>net8.0</TargetFramework>') {
        Write-Host "Project is not targeting .NET 8.0. Attempting to update target framework..." -ForegroundColor Yellow
        $csprojContent = $csprojContent -replace '<TargetFramework>.*?</TargetFramework>', '<TargetFramework>net8.0</TargetFramework>'
        Set-Content $csprojPath $csprojContent
        Write-Host "Target framework updated to .NET 8.0. Running dotnet restore..." -ForegroundColor Cyan
        dotnet restore
    } else {
        Write-Host "Project is already targeting .NET 8.0." -ForegroundColor Green
    }
} else {
    Write-Host "CIPP-MCP.csproj not found. Please ensure you are in the correct directory." -ForegroundColor Red
    exit 1
}

# 1. Check for .NET SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ".NET SDK not found. Please install .NET 8.0 SDK or later from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
} else {
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK version $dotnetVersion found." -ForegroundColor Green
}
dotnet dev-certs https --trust

# 2. Check for Azure Functions Core Tools
if (-not (Get-Command func -ErrorAction SilentlyContinue)) {
    Write-Host "Azure Functions Core Tools not found. Installing via npm..." -ForegroundColor Yellow
    if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
        Write-Host "npm (Node.js) is required to install Azure Functions Core Tools. Please install Node.js from https://nodejs.org/" -ForegroundColor Red
        exit 1
    }
    npm install -g azure-functions-core-tools@4 --unsafe-perm true
} else {
    Write-Host "Azure Functions Core Tools found." -ForegroundColor Green
}

# 3. Restore .NET project dependencies
Write-Host "Restoring .NET project dependencies..." -ForegroundColor Cyan
Set-Location ..
dotnet restore

Write-Host "[CIPP-MCP] Development environment setup complete." -ForegroundColor Green
