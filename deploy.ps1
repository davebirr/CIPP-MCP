#!/usr/bin/env pwsh
# CIPP-MCP Deployment Setup Script

param(
    [Parameter(Mandatory = $true)]
    [string]$GitHubUsername,
    
    [Parameter(Mandatory = $true)]
    [string]$CippApiUrl,
    
    [Parameter(Mandatory = $true)]
    [string]$CippUserEmail,
    
    [Parameter(Mandatory = $false)]
    [string]$ResourceBaseName = "",
    
    [Parameter(Mandatory = $false)]
    [string]$Location = "East US 2",
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipAzureLogin
)

Write-Host "🚀 CIPP-MCP Deployment Setup" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

# Validate parameters
if ($CippApiUrl -notmatch "^https://.*\.azurestaticapps\.net$") {
    Write-Host "❌ Error: CIPP API URL must be a Static Web App URL (https://xxx.azurestaticapps.net)" -ForegroundColor Red
    exit 1
}

if ($CippUserEmail -notmatch "^[^@]+@[^@]+\.[^@]+$") {
    Write-Host "❌ Error: Please provide a valid email address" -ForegroundColor Red
    exit 1
}

# Generate resource base name if not provided
if ([string]::IsNullOrEmpty($ResourceBaseName)) {
    $ResourceBaseName = "cipp-mcp-$($GitHubUsername.ToLower())"
    Write-Host "📝 Generated resource base name: $ResourceBaseName" -ForegroundColor Yellow
}

# Check if Azure CLI is installed
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "✅ Azure CLI version: $($azVersion.'azure-cli')" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: Azure CLI is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Azure login
if (-not $SkipAzureLogin) {
    Write-Host "🔐 Logging into Azure..." -ForegroundColor Cyan
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Error: Azure login failed" -ForegroundColor Red
        exit 1
    }
}

# Get subscription info
$subscription = az account show --output json | ConvertFrom-Json
Write-Host "✅ Using subscription: $($subscription.name) ($($subscription.id))" -ForegroundColor Green

# Create resource group
$resourceGroupName = "rg-$ResourceBaseName"
Write-Host "📦 Creating resource group: $resourceGroupName" -ForegroundColor Cyan

az group create --name $resourceGroupName --location "$Location" --output table
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error: Failed to create resource group" -ForegroundColor Red
    exit 1
}

# Generate deployment parameters
$deploymentParams = @{
    baseName = $ResourceBaseName
    location = $Location
    cippApiUrl = $CippApiUrl
    cippUserEmail = $CippUserEmail
    repositoryUrl = "https://github.com/$GitHubUsername/CIPP-MCP"
    repositoryBranch = "main"
}

$paramsFile = "deployment-params.json"
$deploymentParams | ConvertTo-Json -Depth 10 | Out-File -FilePath $paramsFile -Encoding UTF8

Write-Host "📋 Deployment parameters:" -ForegroundColor Yellow
$deploymentParams | Format-Table -AutoSize

# Deploy ARM template
Write-Host "🚀 Deploying CIPP-MCP to Azure..." -ForegroundColor Cyan
Write-Host "   This may take 5-10 minutes..." -ForegroundColor Gray

$deploymentName = "cipp-mcp-deployment-$(Get-Date -Format 'yyyyMMddHHmmss')"

az deployment group create `
    --resource-group $resourceGroupName `
    --template-file "deployment/AzureDeploymentTemplate.json" `
    --parameters "@$paramsFile" `
    --name $deploymentName `
    --output table

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error: Deployment failed" -ForegroundColor Red
    Write-Host "   Check the deployment logs in Azure Portal for details" -ForegroundColor Yellow
    exit 1
}

# Get deployment outputs
Write-Host "📊 Getting deployment outputs..." -ForegroundColor Cyan
$outputs = az deployment group show `
    --resource-group $resourceGroupName `
    --name $deploymentName `
    --query "properties.outputs" `
    --output json | ConvertFrom-Json

$functionAppName = $outputs.functionAppName.value
$functionAppUrl = $outputs.functionAppUrl.value
$keyVaultName = $outputs.keyVaultName.value

Write-Host ""
Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Deployment Summary:" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host "Resource Group:     $resourceGroupName" -ForegroundColor White
Write-Host "Function App:       $functionAppName" -ForegroundColor White
Write-Host "Function App URL:   $functionAppUrl" -ForegroundColor White
Write-Host "Key Vault:          $keyVaultName" -ForegroundColor White
Write-Host ""

Write-Host "🔑 Next Steps - Configure CIPP Authentication:" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Yellow
Write-Host "1. Navigate to Azure Portal → Key Vaults → $keyVaultName" -ForegroundColor Gray
Write-Host "2. Go to 'Secrets' and add the following secrets:" -ForegroundColor Gray
Write-Host "   • CIPP-APPLICATION-ID: your CIPP application ID" -ForegroundColor Gray
Write-Host "   • CIPP-APPLICATION-SECRET: your CIPP application secret" -ForegroundColor Gray
Write-Host "   • CIPP-REFRESH-TOKEN: your CIPP refresh token" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Test the deployment:" -ForegroundColor Gray
Write-Host "   Health check: $functionAppUrl/api/health" -ForegroundColor Gray
Write-Host ""

Write-Host "🎯 Configure Copilot Studio:" -ForegroundColor Yellow
Write-Host "=============================" -ForegroundColor Yellow
Write-Host "MCP Server URL: $functionAppUrl" -ForegroundColor White
Write-Host ""

Write-Host "📚 Documentation:" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow
Write-Host "• Authentication: See AUTHENTICATION.md" -ForegroundColor Gray
Write-Host "• Integration: See INTEGRATION-GUIDE.md" -ForegroundColor Gray
Write-Host "• API Docs: See Docs/ directory" -ForegroundColor Gray
Write-Host ""

# Clean up temp files
Remove-Item -Path $paramsFile -Force -ErrorAction SilentlyContinue

Write-Host "🎉 CIPP-MCP deployment complete!" -ForegroundColor Green
Write-Host "   Don't forget to configure the Key Vault secrets before testing." -ForegroundColor Yellow
