# CIPP-MCP Authentication Setup Guide

## 🔐 Authentication Strategy

### Development Authentication Options
1. **Browser Session Authentication** - For interactive development
2. **Azure CLI Authentication** - For automated testing  
3. **Service Principal + Key Vault** - For CI/CD and production-like testing

## 🚀 Setup Instructions

### Prerequisites
Before starting, gather this information about your CIPP deployment:
- **Resource Group Name**: Where your CIPP resources are deployed (e.g., `rg-cipp-production`)
- **Subscription ID**: Your Azure subscription ID (e.g., `12345678-1234-1234-1234-123456789012`)
- **Key Vault Name**: Your CIPP Key Vault name (e.g., `your-keyvault-name`)

### Automated Setup (Recommended)

Use our automated setup script to configure everything:

```powershell
# Run the automated setup script with your resource group name
.\Scripts\Setup-Azure-Resources.ps1 -ResourceGroupName "your-cipp-resource-group"

# Or let it auto-discover your CIPP resource group
.\Scripts\Setup-Azure-Resources.ps1
```

The script will:
- ✅ Discover existing CIPP resources
- ✅ Create service principal with minimal permissions
- ✅ Configure Key Vault access policies
- ✅ Update your `.env` file automatically

### Manual Setup (Alternative)

If you prefer manual setup, follow these steps:

#### Step 1: Set Your Variables
```bash
# Set these variables for your environment - REPLACE WITH YOUR VALUES
SUBSCRIPTION_ID="12345678-1234-1234-1234-123456789012"
RESOURCE_GROUP="your-cipp-resource-group"
KEY_VAULT_NAME="your-keyvault-name"
SERVICE_PRINCIPAL_NAME="cipp-mcp-sp"
```

#### Step 2: Create Service Principal for CIPP-MCP

```bash
# Create the service principal with resource group scope
az ad sp create-for-rbac \
  --name "$SERVICE_PRINCIPAL_NAME" \
  --role "Contributor" \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"

# Note the output - you'll need these values:
# {
#   "appId": "12345678-1234-1234-1234-123456789012",
#   "displayName": "cipp-mcp-sp",
#   "password": "your-client-secret",
#   "tenant": "your-tenant-id"
# }
```

#### Step 3: Configure Key Vault Access

```bash
# Give the service principal access to Key Vault secrets
az keyvault set-policy \
  --name "$KEY_VAULT_NAME" \
  --spn "{appId-from-step2}" \
  --secret-permissions get list

# Verify access
az keyvault secret list \
  --vault-name "$KEY_VAULT_NAME" \
  --query "[].name" \
  --output table
```

### Step 3: Configure Environment Variables

Create `.env` file (git-ignored) with your secrets:

```env
# Azure Resource Group (where CIPP infrastructure resides)
AZURE_RESOURCE_GROUP=your-cipp-resource-group
AZURE_SUBSCRIPTION_ID=12345678-1234-1234-1234-123456789012

# Service Principal for Key Vault Access
AZURE_CLIENT_ID=your-service-principal-app-id
AZURE_CLIENT_SECRET=your-service-principal-secret
AZURE_TENANT_ID=your-tenant-id-here

# Key Vault Configuration
KEY_VAULT_NAME=your-keyvault-name
KEY_VAULT_URL=https://your-keyvault-name.vault.azure.net/

# CIPP Configuration
CIPP_API_BASE_URL=https://your-cipp-api.azurewebsites.net
CIPP_SWA_URL=https://your-cipp-frontend.azurestaticapps.net
CIPP_USER_EMAIL=your-email@yourdomain.com

# Authentication Mode
AUTH_MODE=development  # Options: development, keyvault, browser
```

## 🧪 Testing Approaches

### Quick Health Check
```powershell
# Test basic server functionality
dotnet run --project src/CIPP-MCP.csproj --urls "http://localhost:5000"

# In another terminal, verify health endpoint
curl http://localhost:5000/health
```

### Interactive Browser Testing
```powershell
# Test browser-based authentication
$env:AUTH_MODE = "browser"
.\scripts\auth\Get-BrowserSession.ps1 -UserEmail "your-email@yourdomain.com"
```

### Automated Key Vault Testing  
```powershell
# Test Key Vault authentication mode
$env:AUTH_MODE = "keyvault"
.\scripts\auth\Test-KeyVaultAuth.ps1
```

### Full Integration Testing
```powershell
# Run comprehensive tool testing
.\Scripts\Test-Development-Modular.ps1 -Authenticated
```

### MCP Client Testing
```powershell
# Test individual MCP tools
.\Scripts\testing\Test-MCP-Tools.ps1 -ToolName "list_tenants"
```

## 🔑 Required Key Vault Secrets

Based on your CIPP Function App, these secrets are needed for full authentication:

### Core CIPP Secrets
- **`ApplicationId`** - CIPP app registration ID
- **`ApplicationSecret`** - CIPP app secret  
- **`RefreshToken`** - CIPP refresh token
- **`tenantid`** - CIPP tenant ID

### How to Find Your Values

#### Resource Group Name
```bash
# List all resource groups to find your CIPP resource group
az group list --query "[].name" --output table

# Look for groups containing CIPP resources
az group list --query "[?contains(name, 'cipp') || contains(name, 'CIPP')].{Name:name, Location:location}" --output table
```

#### Subscription ID
```bash
# Get your current subscription details
az account show --query "{SubscriptionId:id, SubscriptionName:name}" --output table

# List all available subscriptions
az account list --query "[].{SubscriptionId:id, Name:name, IsDefault:isDefault}" --output table
```

#### Key Vault Name
```bash
# Find Key Vaults in your CIPP resource group
az keyvault list --resource-group "your-cipp-resource-group" --query "[].name" --output table

# List all Key Vaults if you're unsure of the resource group
az keyvault list --query "[?contains(name, 'cipp') || contains(name, 'CIPP')].{Name:name, ResourceGroup:resourceGroup}" --output table
```
### How to Verify Key Vault Secrets
```bash
# List all secrets in your CIPP Key Vault
az keyvault secret list --vault-name "your-keyvault-name" --query "[].name" --output table

# Check specific secret (without revealing value)
az keyvault secret show --vault-name "your-keyvault-name" --name "ApplicationId" --query "id"
```

### Adding Missing Secrets
If secrets are missing from Key Vault, add them:
```bash
# Example: Add application secret
az keyvault secret set --vault-name "your-keyvault-name" --name "ApplicationSecret" --value "your-secret-value"
```

## 🛡️ Security Notes

### Environment Separation
- **Development Mode**: Uses mock data or cached credentials from `.env`
- **Browser Mode**: Captures real browser session for testing
- **Key Vault Mode**: Production-ready authentication from Azure Key Vault

### Security Best Practices
- `.env` is git-ignored to prevent secret exposure
- Use `.env.template` as a reference for required variables
- Service principal has minimal required permissions (scoped to resource group)
- Browser sessions are used only for development
- Production deployments use managed identity (future enhancement)

### Permission Scoping
```bash
# Service principal permissions are limited to:
# 1. Contributor role on specific resource group
# 2. Key Vault secret read access (get, list)
# 3. NO subscription-wide or tenant-wide permissions
```

## 🚀 Quick Start Commands

### Complete Setup in One Go
```powershell
# 1. Clone and setup
git clone <repository-url>
cd CIPP-MCP-v2

# 2. Run automated Azure setup (replace with your resource group name)
.\Scripts\Setup-Azure-Resources.ps1 -ResourceGroupName "your-cipp-resource-group"

# 3. Start development server
dotnet run --project src/CIPP-MCP.csproj --urls "http://localhost:5000"

# 4. Test in another terminal
curl http://localhost:5000/health
```

### Development Workflow
```powershell
# Build and test cycle
dotnet build src/CIPP-MCP.csproj
.\Scripts\Test-Development-Modular.ps1
dotnet run --project src/CIPP-MCP.csproj --urls "http://localhost:5000"
```

## 📋 Troubleshooting

### Common Issues

#### Authentication Failed
```
Error: Unable to authenticate to CIPP API
```
**Solutions:**
1. Verify `.env` file has correct values
2. Check `AUTH_MODE` setting matches your setup
3. Test Key Vault access: `az keyvault secret list --vault-name "your-keyvault-name"`

#### Service Principal Creation Failed
```
Error: Insufficient privileges to complete operation
```
**Solutions:**
1. Ensure you have `Contributor` or `Owner` role on the resource group
2. Verify Azure CLI authentication: `az account show`
3. Check subscription permissions: `az role assignment list --assignee $(az account show --query user.name -o tsv)`

#### Key Vault Access Denied
```
Error: Access denied to Key Vault
```
**Solutions:**
1. Verify Key Vault name and resource group
2. Check access policies: `az keyvault show --name "your-keyvault-name" --query "properties.accessPolicies"`
3. Re-run access policy setup: `az keyvault set-policy --name "your-keyvault-name" --spn "your-sp-id" --secret-permissions get list`

### Validation Commands
```powershell
# Test service principal authentication
az login --service-principal --username $env:AZURE_CLIENT_ID --password $env:AZURE_CLIENT_SECRET --tenant $env:AZURE_TENANT_ID

# Verify resource group access
az group show --name $env:AZURE_RESOURCE_GROUP

# Test Key Vault connectivity
az keyvault secret list --vault-name $env:KEY_VAULT_NAME --query "[].name" --output table
```
