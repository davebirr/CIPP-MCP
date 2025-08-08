# Quick OAuth2 Deployment Guide

## Overview

This guide provides a streamlined path to deploy CIPP MCP Server with OAuth2 user-delegated authentication for Copilot Studio integration.

## Prerequisites

- Existing CIPP deployment in Azure
- Azure subscription with resource creation permissions
- Azure PowerShell or Azure CLI installed
- GitHub account (if forking repository)

## 🚀 One-Script Deployment

### Step 1: Download and Run Deployment Script

```powershell
# Clone or download the repository
git clone https://github.com/KelvinTegelaar/CIPP-MCP.git
cd CIPP-MCP/deployment

# Run the OAuth2 deployment script
.\Deploy-OAuth2.ps1 -ResourceGroupName "your-cipp-rg" `
                    -BaseName "cipp-mcp" `
                    -CippApiUrl "https://your-cipp-domain.com" `
                    -ExistingKeyVaultName "your-cipp-keyvault"
```

### Step 2: Note the Output

The script will output important configuration details:

```
📋 DEPLOYMENT SUMMARY:
   Web App URL: https://cipp-mcp-app.azurewebsites.net
   Azure AD App ID: 12345678-1234-1234-1234-123456789012
   OAuth Redirect URI: https://cipp-mcp-app.azurewebsites.net/oauth/callback

🔧 COPILOT STUDIO CONFIGURATION:
   Client ID: 12345678-1234-1234-1234-123456789012
   Authorization URL: https://login.microsoftonline.com/your-tenant/oauth2/v2.0/authorize
   Token URL: https://login.microsoftonline.com/your-tenant/oauth2/v2.0/token
   Scope: openid profile email offline_access
```

## 🤖 Copilot Studio Configuration

### Step 1: Create Custom Connector

1. **Open Copilot Studio** → **Tools** → **+ New Tool** → **Custom Connector**
2. **Import MCP Connector**:
   - **Connector Type**: Custom
   - **Branch**: Dev
   - **Connector**: MCP-Streamable-HTTP
3. **Configure Basic Settings**:
   - **Host**: `your-mcp-server.azurewebsites.net` (from deployment output)
   - **Base URL**: `/mcp`

### Step 2: Configure OAuth2 Authentication

1. **Click Security Tab**
2. **Set Authentication Type**: `OAuth 2.0`
3. **Configure OAuth Settings** (use values from deployment output):
   - **Identity Provider**: `Azure Active Directory`
   - **Client ID**: `[From deployment summary]`
   - **Client Secret**: `[From deployment summary]`
   - **Authorization URL**: `[From deployment summary]`
   - **Token URL**: `[From deployment summary]`
   - **Refresh URL**: `[Same as Token URL]`
   - **Scope**: `openid profile email offline_access`

### Step 3: Test Connection

1. **Save** the custom connector
2. **Test Connection** in Copilot Studio
3. **You'll be prompted to authenticate** - use your organizational credentials
4. **Verify** you can access CIPP data with your existing permissions

## ✅ Verification Steps

### 1. Health Check
Visit: `https://your-mcp-server.azurewebsites.net/health`

Expected response:
```json
{
  "status": "healthy",
  "authMode": "oauth2",
  "cippApiUrl": "https://your-cipp-domain.com"
}
```

### 2. Test OAuth Flow
1. **Use any MCP tool** in Copilot Studio (e.g., "List my tenants")
2. **You should be redirected** to Azure AD login
3. **After authentication**, the tool should return your CIPP data
4. **Verify permissions** - you should only see data you're authorized to access

### 3. Multi-User Testing
- **Have different users** test the Copilot Studio agent
- **Each user should authenticate individually**
- **Each user should see only their authorized CIPP data**

## 🔧 Troubleshooting

### Common Issues

#### "Authentication required" error
- **Check**: Azure AD app registration redirect URIs
- **Verify**: Client ID and secret in Key Vault
- **Ensure**: User has CIPP access in your organization

#### "Access denied" error
- **Verify**: User has appropriate CIPP roles
- **Check**: User is in the correct Azure AD tenant
- **Confirm**: CIPP permissions are properly configured

#### OAuth redirect fails
- **Check**: Redirect URIs match exactly (including https://)
- **Verify**: Azure AD app registration is in correct tenant
- **Ensure**: No typos in URL configuration

### Debug Endpoints

```
GET /oauth/debug - Shows current auth configuration
GET /health - Shows server status and auth mode
```

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/KelvinTegelaar/CIPP-MCP/issues)
- **Documentation**: [Full OAuth2 Guide](./OAuth2-Authentication-Guide.md)
- **CIPP Support**: [CIPP Discord](https://discord.gg/cipp)

## 🎯 Success Indicators

✅ **Deployment Script Completes Successfully**
✅ **Health Endpoint Returns 200 OK**
✅ **Copilot Studio Authentication Flow Works**
✅ **Users See Only Their Authorized CIPP Data**
✅ **Multiple Users Can Authenticate Independently**

Your CIPP MCP Server with OAuth2 authentication is now ready for production use! 🚀
