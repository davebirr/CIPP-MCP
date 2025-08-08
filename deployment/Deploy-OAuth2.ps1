# CIPP MCP Server OAuth2 Deployment Script
# This script deploys the ARM template and creates the required Azure AD app registration

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$BaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$CippApiUrl,
    
    [Parameter(Mandatory=$true)]
    [string]$ExistingKeyVaultName,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "East US",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServicePlanSku = "S1",
    
    [Parameter(Mandatory=$false)]
    [string]$AzureAdAppDisplayName = "$BaseName CIPP MCP OAuth2 App",
    
    [Parameter(Mandatory=$false)]
    [string]$RepositoryUrl = "https://github.com/KelvinTegelaar/CIPP-MCP",
    
    [Parameter(Mandatory=$false)]
    [string]$RepositoryBranch = "main",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipArmDeployment
)

# Ensure required modules are installed
$requiredModules = @('Az.Accounts', 'Az.Resources', 'Az.KeyVault', 'Microsoft.Graph.Authentication', 'Microsoft.Graph.Applications')

foreach ($module in $requiredModules) {
    if (!(Get-Module -ListAvailable -Name $module)) {
        Write-Host "Installing required module: $module" -ForegroundColor Yellow
        Install-Module -Name $module -Force -AllowClobber
    }
}

# Import modules
Import-Module Az.Accounts
Import-Module Az.Resources
Import-Module Az.KeyVault
Import-Module Microsoft.Graph.Authentication
Import-Module Microsoft.Graph.Applications

Write-Host "Starting CIPP MCP Server OAuth2 deployment..." -ForegroundColor Green

# Step 1: Deploy ARM template (if not skipped)
if (-not $SkipArmDeployment) {
    Write-Host "1. Deploying ARM template..." -ForegroundColor Cyan

    $templatePath = Join-Path $PSScriptRoot "AzureDeploymentTemplate-OAuth2.json"

    $deploymentParams = @{
        ResourceGroupName = $ResourceGroupName
        TemplateFile = $templatePath
        baseName = $BaseName
        location = $Location
        cippApiUrl = $CippApiUrl
        existingKeyVaultName = $ExistingKeyVaultName
        repositoryUrl = $RepositoryUrl
        repositoryBranch = $RepositoryBranch
        appServicePlanSku = $AppServicePlanSku
        azureAdAppDisplayName = $AzureAdAppDisplayName
    }

    try {
        $deployment = New-AzResourceGroupDeployment @deploymentParams -Verbose
        $webAppUrl = $deployment.Outputs.webAppUrl.Value
        $managedIdentityName = $deployment.Outputs.managedIdentityName.Value
        
        Write-Host "✅ ARM template deployed successfully" -ForegroundColor Green
        Write-Host "   Web App URL: $webAppUrl" -ForegroundColor White
        Write-Host "   Managed Identity: $managedIdentityName" -ForegroundColor White
    } catch {
        Write-Error "Failed to deploy ARM template: $_"
        exit 1
    }
} else {
    Write-Host "1. Skipping ARM template deployment (already completed)" -ForegroundColor Yellow
    
    # Get existing resources
    $webAppName = "$BaseName-app"
    $managedIdentityName = "$BaseName-mi"
    
    try {
        $webApp = Get-AzWebApp -ResourceGroupName $ResourceGroupName -Name $webAppName
        $webAppUrl = "https://$($webApp.DefaultHostName)"
        
        Write-Host "✅ Found existing resources" -ForegroundColor Green
        Write-Host "   Web App URL: $webAppUrl" -ForegroundColor White
        Write-Host "   Managed Identity: $managedIdentityName" -ForegroundColor White
    } catch {
        Write-Error "Failed to find existing resources. Ensure the ARM template was deployed first with baseName '$BaseName'"
        exit 1
    }
}

# Step 2: Connect to Microsoft Graph
Write-Host "2. Connecting to Microsoft Graph..." -ForegroundColor Cyan

try {
    Connect-MgGraph -Scopes "Application.ReadWrite.All" -NoWelcome
    Write-Host "✅ Connected to Microsoft Graph" -ForegroundColor Green
} catch {
    Write-Error "Failed to connect to Microsoft Graph: $_"
    exit 1
}

# Step 3: Create Azure AD App Registration
Write-Host "3. Creating Azure AD app registration..." -ForegroundColor Cyan

$redirectUris = @(
    "$webAppUrl/oauth/callback",
    "https://default.directline.botframework.com/oauth/redirect",
    "https://webchat.botframework.com/oauth/redirect"
)

$appParams = @{
    DisplayName = $AzureAdAppDisplayName
    Web = @{
        RedirectUris = $redirectUris
        ImplicitGrantSettings = @{
            EnableAccessTokenIssuance = $true
            EnableIdTokenIssuance = $true
        }
    }
    RequiredResourceAccess = @(
        @{
            ResourceAppId = "00000003-0000-0000-c000-000000000000" # Microsoft Graph
            ResourceAccess = @(
                @{
                    Id = "openid"
                    Type = "Scope"
                },
                @{
                    Id = "profile" 
                    Type = "Scope"
                },
                @{
                    Id = "email"
                    Type = "Scope"
                },
                @{
                    Id = "offline_access"
                    Type = "Scope"
                }
            )
        }
    )
}

try {
    $app = New-MgApplication @appParams
    $clientId = $app.AppId
    
    Write-Host "✅ Azure AD app registration created" -ForegroundColor Green
    Write-Host "   Application ID: $clientId" -ForegroundColor White
    Write-Host "   Display Name: $($app.DisplayName)" -ForegroundColor White
} catch {
    Write-Error "Failed to create Azure AD app registration: $_"
    exit 1
}

# Step 4: Create client secret
Write-Host "4. Creating client secret..." -ForegroundColor Cyan

$secretParams = @{
    ApplicationId = $app.Id
    PasswordCredential = @{
        DisplayName = "CIPP MCP OAuth2 Secret"
        EndDateTime = (Get-Date).AddYears(2)
    }
}

try {
    $secret = Add-MgApplicationPassword @secretParams
    $clientSecret = $secret.SecretText
    
    Write-Host "✅ Client secret created" -ForegroundColor Green
    Write-Host "   Secret expires: $($secret.EndDateTime)" -ForegroundColor White
} catch {
    Write-Error "Failed to create client secret: $_"
    exit 1
}

# Step 5: Store secrets in Key Vault
Write-Host "5. Storing secrets in Key Vault..." -ForegroundColor Cyan

try {
    # Store OAuth2 Client ID
    Set-AzKeyVaultSecret -VaultName $ExistingKeyVaultName -Name "OAUTH2-CLIENT-ID" -SecretValue (ConvertTo-SecureString -String $clientId -AsPlainText -Force)
    
    # Store OAuth2 Client Secret
    Set-AzKeyVaultSecret -VaultName $ExistingKeyVaultName -Name "OAUTH2-CLIENT-SECRET" -SecretValue (ConvertTo-SecureString -String $clientSecret -AsPlainText -Force)
    
    Write-Host "✅ Secrets stored in Key Vault" -ForegroundColor Green
    Write-Host "   OAUTH2-CLIENT-ID: Stored" -ForegroundColor White
    Write-Host "   OAUTH2-CLIENT-SECRET: Stored" -ForegroundColor White
} catch {
    Write-Error "Failed to store secrets in Key Vault: $_"
    Write-Host "Manual steps required:" -ForegroundColor Yellow
    Write-Host "  1. Go to Key Vault '$ExistingKeyVaultName'" -ForegroundColor White
    Write-Host "  2. Add secret 'OAUTH2-CLIENT-ID' with value: $clientId" -ForegroundColor White
    Write-Host "  3. Add secret 'OAUTH2-CLIENT-SECRET' with value: $clientSecret" -ForegroundColor White
}

# Step 6: Grant Key Vault access to managed identity
Write-Host "6. Granting Key Vault access to managed identity..." -ForegroundColor Cyan

try {
    $managedIdentity = Get-AzUserAssignedIdentity -ResourceGroupName $ResourceGroupName -Name $managedIdentityName
    $principalId = $managedIdentity.PrincipalId
    
    Set-AzKeyVaultAccessPolicy -VaultName $ExistingKeyVaultName -ObjectId $principalId -PermissionsToSecrets Get,List
    
    Write-Host "✅ Managed identity granted Key Vault access" -ForegroundColor Green
} catch {
    Write-Warning "Failed to grant Key Vault access automatically. Please grant manually:"
    Write-Host "  1. Go to Key Vault '$ExistingKeyVaultName' > Access policies" -ForegroundColor White
    Write-Host "  2. Add access policy for managed identity '$managedIdentityName'" -ForegroundColor White
    Write-Host "  3. Grant 'Get' and 'List' permissions for secrets" -ForegroundColor White
}

# Step 7: Display completion summary
Write-Host "`n🎉 CIPP MCP Server OAuth2 deployment completed!" -ForegroundColor Green

Write-Host "`n📋 DEPLOYMENT SUMMARY:" -ForegroundColor Yellow
Write-Host "   Web App URL: $webAppUrl" -ForegroundColor White
Write-Host "   Azure AD App ID: $clientId" -ForegroundColor White
Write-Host "   Azure AD App Name: $AzureAdAppDisplayName" -ForegroundColor White
Write-Host "   OAuth Redirect URI: $webAppUrl/oauth/callback" -ForegroundColor White
Write-Host "   Key Vault: $ExistingKeyVaultName" -ForegroundColor White
Write-Host "   Managed Identity: $managedIdentityName" -ForegroundColor White

Write-Host "`n🔧 COPILOT STUDIO CONFIGURATION:" -ForegroundColor Yellow
Write-Host "   1. Edit your Custom Connector in Copilot Studio" -ForegroundColor White
Write-Host "   2. Go to Security tab" -ForegroundColor White
Write-Host "   3. Change Authentication to 'OAuth 2.0'" -ForegroundColor White
Write-Host "   4. Identity Provider: 'Azure Active Directory'" -ForegroundColor White
Write-Host "   5. Client ID: $clientId" -ForegroundColor White
Write-Host "   6. Client Secret: [Use the generated secret]" -ForegroundColor White
Write-Host "   7. Authorization URL: https://login.microsoftonline.com/$((Get-AzContext).Tenant.Id)/oauth2/v2.0/authorize" -ForegroundColor White
Write-Host "   8. Token URL: https://login.microsoftonline.com/$((Get-AzContext).Tenant.Id)/oauth2/v2.0/token" -ForegroundColor White
Write-Host "   9. Refresh URL: https://login.microsoftonline.com/$((Get-AzContext).Tenant.Id)/oauth2/v2.0/token" -ForegroundColor White
Write-Host "   10. Scope: openid profile email offline_access" -ForegroundColor White

Write-Host "`n🧪 TESTING:" -ForegroundColor Yellow
Write-Host "   1. Test MCP server: $webAppUrl/health" -ForegroundColor White
Write-Host "   2. Test OAuth flow in Copilot Studio by using any MCP tool" -ForegroundColor White
Write-Host "   3. Verify users authenticate with their own CIPP permissions" -ForegroundColor White

Write-Host "`n⚠️  IMPORTANT NOTES:" -ForegroundColor Red
Write-Host "   • Each Copilot Studio user will authenticate individually" -ForegroundColor White
Write-Host "   • Users inherit their existing CIPP roles and permissions" -ForegroundColor White
Write-Host "   • This replaces the previous keyvault authentication mode" -ForegroundColor White
Write-Host "   • The client secret expires in 2 years - set a calendar reminder" -ForegroundColor White

Write-Host "`nDeployment completed successfully! 🚀" -ForegroundColor Green
