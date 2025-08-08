# CIPP-MCP Azure Deployment

⚠️ **CRITICAL**: This deployment creates an **ASP.NET Core Web App**, not an Azure Function App. Ensure the runtime stack is configured as `DOTNETCORE|9.0`.

This Azure Resource Manager (ARM) template adds CIPP-MCP to your existing CIPP deployment as a production-ready Web App with OAuth2 user-delegated authentication for Copilot Studio integration.

## Prerequisites

1. **Existing CIPP Installation**: You must have CIPP already deployed and running
2. **CIPP Key Vault Name**: Know the name of your existing CIPP Key Vault  
3. **Azure Subscription**: Access to deploy resources in the same resource group as CIPP
4. **PowerShell**: For automated OAuth2 setup (Azure PowerShell and Microsoft Graph modules)
5. **Repository Fork**: Fork this repository to your GitHub account (optional for CI/CD)

## 🚀 Recommended Deployment Flow (OAuth2 + Copilot Studio)

### Step 1: Deploy Infrastructure
[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FCIPP-MCP%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.json)

**Parameters to set:**
- **Auth Mode**: `oauth2` (for Copilot Studio user authentication)
- **Base Name**: Unique name for MCP resources
- **CIPP API URL**: Your existing CIPP Static Web App URL
- **CIPP User Email**: Your administrator email (used for Key Vault mode fallback)
- **Existing Key Vault Name**: Name of your CIPP Key Vault

### Step 2: Complete OAuth2 Setup
After the ARM template deployment completes, run the PowerShell script to create the Azure AD app registration:

```powershell
# Download and run the OAuth2 setup script
git clone https://github.com/yourusername/CIPP-MCP.git
cd CIPP-MCP/deployment

.\Deploy-OAuth2.ps1 -ResourceGroupName "your-cipp-rg" `
                    -BaseName "the-basename-from-step1" `
                    -CippApiUrl "https://your-cipp.azurestaticapps.net" `
                    -ExistingKeyVaultName "your-cipp-keyvault" `
                    -SkipArmDeployment
```

**Note**: Use the `-SkipArmDeployment` parameter since you already deployed via the Azure button.

### Step 3: Configure Copilot Studio
The script will output the exact configuration for your Copilot Studio custom connector:
- OAuth2 Client ID
- Authorization and Token URLs
- Redirect URIs
- Required scopes

## Alternative: Legacy Key Vault Mode

If you prefer service-level authentication instead of user-delegated OAuth2:

1. Deploy with **Auth Mode**: `keyvault`
2. Skip the PowerShell script
3. Manually add CIPP secrets to Key Vault:
   - `CIPP-APPLICATION-ID`
   - `CIPP-APPLICATION-SECRET` 
   - `CIPP-REFRESH-TOKEN`
   - **Repository URL**: Repository to deploy from (for CI/CD setup)
   - **Repository Branch**: Branch to deploy (usually `main`)

### 3. Grant Key Vault Access

After deployment, grant the new managed identity access to your existing CIPP Key Vault:

1. Navigate to your existing CIPP Key Vault in Azure Portal
2. Go to "Access policies" 
3. Click "+ Add Access Policy"
4. Configure permissions:
   - **Secret permissions**: Get, List
   - **Select principal**: Search for the managed identity name from deployment outputs
5. Click "Add" then "Save"

### 4. Add MCP Authentication Secrets

The MCP server uses the same CIPP authentication secrets. Ensure these exist in your Key Vault:
   - `CIPP-APPLICATION-ID`: Your CIPP application ID
   - `CIPP-APPLICATION-SECRET`: Your CIPP application secret  
   - `CIPP-REFRESH-TOKEN`: Your CIPP refresh token

If these don't exist yet, add them to your CIPP Key Vault.

### 5. Configure Continuous Deployment (Optional)

For automatic deployments from your fork:

1. Navigate to your new **Web App** in Azure Portal
2. Go to **Deployment Center**
3. Select **GitHub** as the source
4. **Authenticate with GitHub** and authorize Azure
5. Select your **repository** (your fork)
6. Choose the **main branch**
7. Set **Build provider** to GitHub Actions
8. Click **Save**

Azure will automatically:
- Create a GitHub Actions workflow in your repository
- Build and deploy your code whenever you push changes
- Show deployment history and logs

### 6. Verify Deployment

1. Navigate to your Web App in Azure Portal
2. Go to the URL (e.g., `https://your-app-name.azurewebsites.net`)
3. Test endpoints:
   - Health: `https://your-app-name.azurewebsites.net/health`
   - MCP: `https://your-app-name.azurewebsites.net/mcp`

## Deployed Resources

The ARM template creates only the additional resources needed for CIPP-MCP:

- **Azure Web App**: Hosts the ASP.NET Core CIPP-MCP server
- **App Service Plan**: Basic B1 tier for production workloads
- **Application Insights**: Monitoring and logging
- **Managed Identity**: Secure access to your existing CIPP Key Vault

**Reuses existing CIPP infrastructure**: Key Vault, resource group, and authentication.

## Configuration

The deployment automatically configures:

- **Runtime Stack**: `DOTNETCORE|9.0` for ASP.NET Core 9.0
- **Managed Identity**: Secure Key Vault access without storing credentials
- **Application Settings**: CIPP integration and authentication mode
- **HTTPS Enforcement**: Secure communication only
- **Application Insights**: Comprehensive monitoring and logging

**Note**: CI/CD setup is optional and done after deployment for flexibility.

## Custom Domain Setup (Optional)

For production deployments with custom domains:

1. **Deploy the Web App** using the ARM template
2. **Configure DNS records** (CNAME and TXT for validation)  
3. **Add custom domain** in Azure Portal
4. **Create SSL certificate** (Azure managed recommended)
5. **Enable HTTPS-only** enforcement
6. **Test all endpoints** on the custom domain

Example: `https://cipp-mcp.yourdomain.com`

## Security

- Leverages your existing CIPP Key Vault for credential storage
- Managed identity provides secure access without storing credentials
- HTTPS-only traffic is enforced
- Storage account blocks public blob access

## Updating

After setting up CI/CD (see step 5 above):

1. Push changes to your GitHub repository
2. GitHub Actions will automatically build and deploy
3. Monitor deployment progress in Azure Portal > Function App > Deployment Center

## Troubleshooting

### ⚠️ Critical: Runtime Stack Issues

**Symptom**: Seeing nginx welcome page, PHP, or 502 errors instead of .NET application  
**Cause**: Incorrect runtime stack configuration  
**Solution**: Verify the ARM template uses `DOTNETCORE|9.0` not `DOTNET|9.0`

```bash
# Check current runtime stack
az webapp config show --name your-app --resource-group your-rg --query "linuxFxVersion"

# Fix if incorrect
az webapp config set --name your-app --resource-group your-rg --linux-fx-version "DOTNETCORE|9.0"
```

### Web App Not Starting
- Check Application Insights logs for startup errors
- Verify Key Vault access permissions for the managed identity
- Ensure CIPP credentials are properly stored in Key Vault
- Confirm runtime stack is `DOTNETCORE|9.0`

### Authentication Issues
- Verify CIPP API URL is correct and accessible
- Check that CIPP authentication secrets are valid in Key Vault
- Ensure managed identity has "Get" and "List" permissions on Key Vault secrets
- Test authentication with `Test-Authentication.ps1` script

### Deployment Failures
- Check that resource names are unique across Azure
- Verify you have sufficient permissions in the subscription
- Ensure you're deploying to the same resource group as CIPP
- Check that the repository URL is accessible
- Verify ARM template parameter values

### Custom Domain Issues
- Ensure DNS records are properly configured (CNAME and TXT)
- Verify domain ownership validation is complete
- Check SSL certificate status in Azure Portal
- Test domain resolution with `nslookup` or `dig`

### Verification Commands

```bash
# Test deployment health
curl https://your-app.azurewebsites.net/health

# Test MCP endpoint  
curl -X POST https://your-app.azurewebsites.net/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "method": "tools/list", "id": 1}'

# Check runtime configuration
az webapp config show --name your-app --resource-group your-rg

# View recent logs
az webapp log tail --name your-app --resource-group your-rg
```

## Support

For issues and questions:
1. Check the main CIPP-MCP repository issues
2. Review Application Insights logs for runtime errors
3. Verify CIPP integration following the main documentation
