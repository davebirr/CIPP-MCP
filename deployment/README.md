# CIPP-MCP Azure Deployment

This1. Click the "Deploy to Azure" button above
2. **Deploy to the same resource group** as your existing CIPP installation
3. Fill in the required parameters:
   - **Base Name**: Unique name for MCP resources (default auto-generates)
   - **Location**: Azure region (same as your CIPP deployment)
   - **CIPP API URL**: Your existing CIPP Static Web App URL
   - **CIPP User Email**: Your administrator email
   - **Existing Key Vault Name**: Name of your CIPP Key Vault (e.g., "cipp-kv-abc123")
   - **Repository URL**: Repository to deploy from (defaults to this repo)
   - **Repository Branch**: Branch to deploy (usually `main`)

### 3. Grant Key Vault Access

After deployment, you need to grant the new managed identity access to your existing CIPP Key Vault:

1. Navigate to your existing CIPP Key Vault in Azure Portal
2. Go to "Access policies" 
3. Click "+ Add Access Policy"
4. Configure permissions:
   - **Secret permissions**: Get, List
   - **Select principal**: Search for the managed identity name from deployment outputs
5. Click "Add" then "Save"

### 4. Add MCP Authentication Secrets

The MCP server will use the same CIPP authentication secrets. Ensure these exist in your Key Vault:
   - `CIPP-APPLICATION-ID`: Your CIPP application ID
   - `CIPP-APPLICATION-SECRET`: Your CIPP application secret  
   - `CIPP-REFRESH-TOKEN`: Your CIPP refresh token

If these don't exist yet, add them to your CIPP Key Vault. the Azure Resource Manager (ARM) template for adding CIPP-MCP to your existing CIPP deployment.

## Prerequisites

1. **Existing CIPP Installation**: You must have CIPP already deployed and running
2. **CIPP Key Vault Name**: Know the name of your existing CIPP Key Vault
3. **Azure Subscription**: Access to deploy resources in the same resource group as CIPP
4. **Repository Fork**: Fork this repository to your GitHub account (optional for users)

## Quick Deploy

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FCIPP-MCP%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.json)

## What Gets Deployed

This template creates **only** the resources needed for CIPP-MCP:
- **Azure Function App**: Hosts the MCP server (lightweight consumption plan)
- **App Service Plan**: Consumption plan for the Function App
- **Storage Account**: Required for Function App operation
- **Application Insights**: Monitoring and logging for the MCP server
- **Managed Identity**: Secure access to your existing CIPP Key Vault

**No duplicate resources** - it uses your existing CIPP Key Vault and infrastructure.

## Manual Deployment

### 1. Fork the Repository

1. Fork this repository to your GitHub account
2. Note your repository URL (e.g., `https://github.com/davebirr/CIPP-MCP`)

### 2. Deploy Resources

1. Click the "Deploy to Azure" button above (with your correct GitHub URL)
2. Fill in the required parameters:
   - **Base Name**: Unique name for your resources (default auto-generates)
   - **Location**: Azure region for deployment
   - **CIPP API URL**: Your existing CIPP Static Web App URL
   - **CIPP User Email**: Your administrator email
   - **Repository URL**: Your forked repository URL (update if you forked this repo)
   - **Repository Branch**: Branch to deploy (usually `main`)

### 3. Configure CIPP Authentication

After deployment, you need to store CIPP authentication credentials in Key Vault:

1. Navigate to the created Key Vault in Azure Portal
2. Go to "Secrets" 
3. Add the following secrets:
   - `CIPP-APPLICATION-ID`: Your CIPP application ID
   - `CIPP-APPLICATION-SECRET`: Your CIPP application secret  
   - `CIPP-REFRESH-TOKEN`: Your CIPP refresh token

### 4. Test the Deployment

1. Navigate to your Function App in Azure Portal
2. Go to "Functions" and find the MCP endpoints
3. Test the health endpoint: `https://your-function-app.azurewebsites.net/api/health`

## Deployed Resources

The ARM template creates only the additional resources needed for CIPP-MCP:

- **Azure Function App**: Hosts the CIPP-MCP server
- **App Service Plan**: Consumption plan for the Function App
- **Storage Account**: Required for Function App operation
- **Application Insights**: Monitoring and logging
- **Managed Identity**: Secure access to your existing CIPP Key Vault

**Reuses existing CIPP infrastructure**: Key Vault, resource group, and authentication.

## Configuration

The deployment automatically configures:

- Managed identity for secure Key Vault access
- Application settings for CIPP integration
- Continuous deployment from your GitHub repository
- Application Insights for monitoring

## Security

- Leverages your existing CIPP Key Vault for credential storage
- Managed identity provides secure access without storing credentials
- HTTPS-only traffic is enforced
- Storage account blocks public blob access

## Updating

To update your deployment:

1. Push changes to your GitHub repository
2. The Function App will automatically redeploy (if continuous deployment is enabled)
3. Or manually sync in Azure Portal under "Deployment Center"

## Troubleshooting

### Function App not starting
- Check Application Insights logs
- Verify Key Vault access permissions for the managed identity
- Ensure CIPP credentials are properly stored in Key Vault

### Authentication issues
- Verify CIPP API URL is correct and accessible
- Check that CIPP authentication secrets are valid in Key Vault
- Ensure managed identity has "Get" and "List" permissions on Key Vault secrets

### Deployment fails
- Check that resource names are unique
- Verify you have sufficient permissions in the subscription
- Ensure you're deploying to the same resource group as CIPP
- Check that the repository URL is accessible

## Support

For issues and questions:
1. Check the main CIPP-MCP repository issues
2. Review Application Insights logs for runtime errors
3. Verify CIPP integration following the main documentation
