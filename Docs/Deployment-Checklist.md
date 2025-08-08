# CIPP MCP Deployment Checklist

## Pre-Deployment Requirements ✅

- [ ] **Existing CIPP Installation**: CIPP is deployed and working
- [ ] **CIPP Key Vault Name**: Know your CIPP Key Vault name (e.g., "cipp-kv-abc123")
- [ ] **CIPP URL**: Know your CIPP Static Web App URL
- [ ] **Azure Permissions**: Can create resources in CIPP resource group
- [ ] **PowerShell Setup**: Azure PowerShell modules installed
- [ ] **Repository Access**: Can clone/download CIPP-MCP repository

## OAuth2 Deployment Steps (Recommended)

### Step 1: Deploy Infrastructure via Azure Portal ⚡
- [ ] Click "Deploy to Azure" button in README
- [ ] **Resource Group**: Select same resource group as CIPP
- [ ] **Auth Mode**: Set to `oauth2`
- [ ] **Base Name**: Choose unique name (e.g., "cipp-mcp-prod")
- [ ] **CIPP API URL**: Enter your CIPP Static Web App URL
- [ ] **CIPP User Email**: Enter admin email (for fallback)
- [ ] **Existing Key Vault Name**: Enter your CIPP Key Vault name
- [ ] **Deploy**: Wait for completion (5-10 minutes)
- [ ] **Note Output**: Copy the deployment outputs (Web App URL, Managed Identity name)

### Step 2: OAuth2 Setup via PowerShell 🔐
- [ ] **Download Repository**: Clone or download CIPP-MCP repository
- [ ] **Open PowerShell**: Navigate to `deployment` folder
- [ ] **Run Script**: Execute `Deploy-OAuth2.ps1` with `-SkipArmDeployment`
- [ ] **Parameters**: Use same values from Step 1
- [ ] **Azure Login**: Authenticate when prompted
- [ ] **Graph Permission**: Grant "Application.ReadWrite.All" when requested
- [ ] **Complete**: Wait for script completion (2-5 minutes)
- [ ] **Save Output**: Copy OAuth2 configuration details

### Step 3: Copilot Studio Configuration 🤖
- [ ] **Open Copilot Studio**: Navigate to your custom connector
- [ ] **Security Tab**: Change authentication to "OAuth 2.0"
- [ ] **Identity Provider**: Set to "Azure Active Directory"
- [ ] **Client ID**: Use value from PowerShell script output
- [ ] **Client Secret**: Use value from PowerShell script output
- [ ] **Authorization URL**: Use value from PowerShell script output
- [ ] **Token URL**: Use value from PowerShell script output
- [ ] **Scope**: Set to "openid profile email offline_access"
- [ ] **Save Connector**: Test connection

### Step 4: Verification 🧪
- [ ] **Health Check**: Visit `https://your-mcp-app.azurewebsites.net/health`
- [ ] **Expected Response**: `{"status":"healthy","authMode":"oauth2"}`
- [ ] **Test Tool**: Use any MCP tool in Copilot Studio
- [ ] **OAuth Flow**: Verify redirect to Azure AD login
- [ ] **Authentication**: Complete login with your organizational account
- [ ] **Tool Response**: Verify tool returns your CIPP data
- [ ] **Permissions**: Confirm you only see data you're authorized to access

## Alternative: Key Vault Mode Deployment

### If you prefer service-level authentication:
- [ ] **Deploy**: Use "Deploy to Azure" with `authMode=keyvault`
- [ ] **Skip**: PowerShell script (not needed)
- [ ] **Manual Setup**: Add CIPP secrets to Key Vault:
  - [ ] `CIPP-APPLICATION-ID`
  - [ ] `CIPP-APPLICATION-SECRET`
  - [ ] `CIPP-REFRESH-TOKEN`
- [ ] **Copilot Studio**: Use "No Authentication" or basic authentication

## Post-Deployment Maintenance 🔧

### Security
- [ ] **Key Vault Access**: Verify managed identity has Get/List permissions
- [ ] **Client Secret**: Set calendar reminder for 2-year renewal
- [ ] **Monitor**: Set up alerts for authentication failures

### CI/CD (Optional)
- [ ] **Fork Repository**: If you want automatic updates
- [ ] **Deployment Center**: Configure GitHub Actions in Azure Portal
- [ ] **Branch**: Connect to your fork's main branch

### Monitoring
- [ ] **Application Insights**: Check logs and performance
- [ ] **Health Endpoint**: Set up monitoring alerts
- [ ] **User Feedback**: Monitor Copilot Studio usage

## Troubleshooting Quick Reference 🛠️

### Common Issues
- **"Authentication required"**: Check Azure AD app redirect URIs
- **"Access denied"**: Verify user has CIPP permissions
- **Health check fails**: Verify managed identity Key Vault access
- **OAuth redirect fails**: Check redirect URI exact match

### Debug Commands
```powershell
# Test Web App
Invoke-RestMethod -Uri "https://your-mcp-app.azurewebsites.net/health"

# Check Key Vault access
Get-AzKeyVaultSecret -VaultName "your-keyvault" -Name "OAUTH2-CLIENT-ID"

# Verify managed identity
Get-AzUserAssignedIdentity -ResourceGroupName "your-rg" -Name "your-mi-name"
```

## Success Indicators ✅

- [ ] **Health endpoint returns 200 OK**
- [ ] **OAuth2 flow completes successfully**
- [ ] **Copilot Studio tools return real CIPP data**
- [ ] **Multiple users can authenticate independently**
- [ ] **Users see only their authorized data**
- [ ] **No agent hallucination (fake tenant names)**

## Support Resources 📞

- **Documentation**: [OAuth2 Authentication Guide](./OAuth2-Authentication-Guide.md)
- **Issues**: [GitHub Issues](https://github.com/KelvinTegelaar/CIPP-MCP/issues)
- **CIPP Community**: [CIPP Discord](https://discord.gg/cipp)
- **Deployment Help**: [Deployment README](./deployment/README.md)

---

**Deployment Complete!** 🎉 Your CIPP MCP Server with OAuth2 authentication is ready for production use with Copilot Studio.
