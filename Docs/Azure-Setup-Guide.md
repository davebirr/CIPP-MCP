# Azure Resource Group Setup Guide

## Overview

CIPP-MCP is designed to deploy alongside your existing CIPP infrastructure in the same Azure Resource Group. This approach provides several benefits:

- **Security**: Service principal permissions scoped to specific resource group
- **Organization**: All CIPP resources co-located for easier management
- **Cost Management**: Unified billing and resource tracking
- **Simplified Deployment**: Automated discovery of existing CIPP resources

## Prerequisites

Before running the setup script, you'll need:

1. **Azure CLI** installed and authenticated
2. **Existing CIPP deployment** in Azure (Function App, Static Web App, Key Vault)
3. **Contributor permissions** on the resource group containing CIPP
4. **PowerShell 7+** (for cross-platform compatibility)

## Required Information

Collect the following information about your CIPP deployment:

### 1. Azure Resource Group Name
- **Purpose**: Contains your CIPP infrastructure
- **Example**: `rg-cipp-production`, `cipp-resources`, `RG-CIPP-MAIN`
- **How to find**: In Azure Portal → Resource Groups → look for the group containing your CIPP resources

### 2. Azure Subscription ID
- **Purpose**: Identifies which Azure subscription to use
- **Example**: `12345678-1234-1234-1234-123456789012`
- **How to find**: Azure Portal → Subscriptions, or run `az account show`

### 3. CIPP Resource Names
The setup script will auto-discover these, but verify they exist:
- **Function App**: CIPP-API (e.g., `cipp-api-prod`)
- **Static Web App**: CIPP Frontend (e.g., `cipp-frontend`)
- **Key Vault**: CIPP secrets (e.g., `kv-cipp-prod`)
- **Storage Account**: CIPP data (e.g., `stcippprod001`)

## Setup Process

### Step 1: Authenticate to Azure

```powershell
# Login to Azure
az login

# Verify correct subscription
az account show

# Switch subscription if needed
az account set --subscription "your-subscription-id"
```

### Step 2: Run Setup Script

```powershell
# Navigate to CIPP-MCP directory
cd "C:\Path\To\CIPP-MCP-v2"

# Run setup with auto-discovery
.\Scripts\Setup-Azure-Resources.ps1

# Or specify resource group explicitly
.\Scripts\Setup-Azure-Resources.ps1 -ResourceGroupName "rg-cipp-production"

# Full configuration example
.\Scripts\Setup-Azure-Resources.ps1 `
    -ResourceGroupName "rg-cipp-production" `
    -SubscriptionId "12345678-1234-1234-1234-123456789012" `
    -ServicePrincipalName "cipp-mcp-service-principal"
```

### Step 3: Verify Configuration

The script will update your `.env` file with discovered values:

```bash
# Azure Resource Group (where CIPP infrastructure resides)
AZURE_RESOURCE_GROUP=rg-cipp-production
AZURE_SUBSCRIPTION_ID=12345678-1234-1234-1234-123456789012

# Azure Service Principal for Key Vault Access
AZURE_CLIENT_ID=87654321-4321-4321-4321-210987654321
AZURE_CLIENT_SECRET=generated-secret-value
AZURE_TENANT_ID=your-tenant-id

# Key Vault Configuration (auto-discovered)
KEY_VAULT_NAME=kv-cipp-prod
KEY_VAULT_URL=https://kv-cipp-prod.vault.azure.net/

# CIPP API Configuration (auto-discovered)
CIPP_API_BASE_URL=https://cipp-api-prod.azurewebsites.net
CIPP_SWA_URL=https://your-cipp-frontend.azurestaticapps.net
```

## Service Principal Permissions

The setup script creates a service principal with these permissions:

### Resource Group Level
- **Role**: Contributor
- **Scope**: `/subscriptions/{subscription}/resourceGroups/{resource-group}`
- **Purpose**: Deploy and manage CIPP-MCP resources

### Key Vault Level
- **Permissions**: `get`, `list` (secrets)
- **Purpose**: Read CIPP authentication tokens and configuration

## Security Considerations

### Principle of Least Privilege
- Service principal permissions limited to specific resource group
- Key Vault access restricted to read-only secret operations
- No subscription-wide or tenant-wide permissions granted

### Secret Management
- Service principal credentials stored in `.env` file (git-ignored)
- CIPP authentication tokens retrieved from Key Vault at runtime
- No hardcoded secrets in application code

### Environment Separation
- Development: Uses mock data or limited permissions
- Production: Full Key Vault integration with real CIPP data
- Each environment has separate service principals

## Troubleshooting

### Common Issues

#### Resource Group Not Found
```
Resource group 'rg-cipp-prod' not found
```
**Solution**: Verify resource group name and ensure you have access permissions

#### Service Principal Creation Failed
```
Error creating service principal: Insufficient privileges
```
**Solution**: Ensure you have Contributor or Owner role on the resource group

#### Key Vault Access Denied
```
Access denied to Key Vault 'kv-cipp-prod'
```
**Solution**: Verify Key Vault exists and you have permission to modify access policies

### Verification Commands

```powershell
# Verify resource group exists
az group show --name "rg-cipp-production"

# List resources in group
az resource list --resource-group "rg-cipp-production" --output table

# Test service principal
az login --service-principal --username $AZURE_CLIENT_ID --password $AZURE_CLIENT_SECRET --tenant $AZURE_TENANT_ID

# Test Key Vault access
az keyvault secret show --vault-name "kv-cipp-prod" --name "test-secret"
```

## Manual Setup (Alternative)

If the automated script doesn't work, you can set up manually:

### 1. Create Service Principal
```powershell
az ad sp create-for-rbac --name "cipp-mcp-sp" --role "Contributor" --scopes "/subscriptions/{subscription}/resourceGroups/{resource-group}"
```

### 2. Add Key Vault Permissions
```powershell
az keyvault set-policy --name "kv-cipp-prod" --spn {client-id} --secret-permissions get list
```

### 3. Update .env File
Manually populate the `.env` file with the values from steps 1-2.

## Next Steps

After successful setup:

1. **Test Authentication**: Run `dotnet run --project src/CIPP-MCP.csproj`
2. **Verify MCP Tools**: Use the PowerShell test scripts to validate functionality
3. **Deploy to Azure**: Use Azure Container Apps or App Service for production deployment
4. **Configure Copilot Studio**: Connect to your deployed CIPP-MCP endpoint

## Support

For issues with setup:
1. Check Azure Portal for resource group and Key Vault configuration
2. Verify Azure CLI authentication: `az account show`
3. Review service principal permissions in Azure AD
4. Test Key Vault connectivity manually before running CIPP-MCP
