# CIPP-MCP Azure Deployment

This directory contains the Azure Resource Manager (ARM) template for deploying CIPP-MCP to Azure.

## Prerequisites

1. **Existing CIPP Installation**: You must have CIPP already deployed and running
2. **Azure Subscription**: Access to deploy resources in Azure
3. **Repository Fork**: Fork this repository to your GitHub account

## Quick Deploy

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FCIPP-MCP%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.json)

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
   - **Repository URL**: Your forked repository URL
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

The ARM template creates:

- **Azure Function App**: Hosts the CIPP-MCP server
- **App Service Plan**: Consumption plan for the Function App
- **Storage Account**: Required for Function App operation
- **Key Vault**: Stores CIPP authentication credentials
- **Application Insights**: Monitoring and logging
- **Managed Identity**: Secure access to Key Vault

## Configuration

The deployment automatically configures:

- Managed identity for Key Vault access
- Application settings for CIPP integration
- Continuous deployment from your GitHub repository
- Application Insights for monitoring

## Security

- All sensitive credentials are stored in Key Vault
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
- Verify Key Vault access permissions
- Ensure CIPP credentials are properly stored in Key Vault

### Authentication issues
- Verify CIPP API URL is correct and accessible
- Check that CIPP authentication secrets are valid
- Ensure managed identity has access to Key Vault

### Deployment fails
- Check that resource names are unique
- Verify you have sufficient permissions in the subscription
- Check that the repository URL is accessible

## Support

For issues and questions:
1. Check the main CIPP-MCP repository issues
2. Review Application Insights logs for runtime errors
3. Verify CIPP integration following the main documentation
