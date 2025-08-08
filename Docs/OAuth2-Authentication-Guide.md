# OAuth2 User-Delegated Authentication Guide

## Overview

This guide explains how to implement OAuth2 user-delegated authentication for the CIPP MCP Server, allowing each Copilot Studio user to authenticate individually with their own CIPP roles and permissions.

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐    ┌──────────────┐
│  Copilot Studio │    │   MCP Server     │    │   Azure AD      │    │     CIPP     │
│     User        │    │  (OAuth2 Mode)   │    │  App Registration│    │   Application│
└─────────────────┘    └──────────────────┘    └─────────────────┘    └──────────────┘
         │                       │                       │                     │
         │ 1. Invoke MCP Tool     │                       │                     │
         ├──────────────────────►│                       │                     │
         │                       │                       │                     │
         │ 2. OAuth2 Redirect     │                       │                     │
         ◄──────────────────────┤                       │                     │
         │                       │                       │                     │
         │ 3. User Authentication │                       │                     │
         ├──────────────────────────────────────────────►│                     │
         │                       │                       │                     │
         │ 4. Authorization Code  │                       │                     │
         ◄──────────────────────────────────────────────┤                     │
         │                       │                       │                     │
         │ 5. Code to MCP Server  │                       │                     │
         ├──────────────────────►│                       │                     │
         │                       │                       │                     │
         │                       │ 6. Exchange for Token │                     │
         │                       ├──────────────────────►│                     │
         │                       │                       │                     │
         │                       │ 7. Access Token       │                     │
         │                       ◄──────────────────────┤                     │
         │                       │                       │                     │
         │                       │ 8. Call CIPP API with User Token            │
         │                       ├─────────────────────────────────────────────►│
         │                       │                       │                     │
         │                       │ 9. Response (User's CIPP Data)              │
         │                       ◄─────────────────────────────────────────────┤
         │                       │                       │                     │
         │ 10. MCP Tool Response  │                       │                     │
         ◄──────────────────────┤                       │                     │
```

## Deployment Steps

### Step 1: Deploy Infrastructure

Run the OAuth2 deployment script:

```powershell
# Navigate to deployment directory
cd "C:\Users\davidb\1Repositories\CIPP-Project\CIPP-MCP-v2\deployment"

# Run deployment script
.\Deploy-OAuth2.ps1 -ResourceGroupName "your-rg" -BaseName "cipp-mcp" -CippApiUrl "https://your-cipp-url.com" -ExistingKeyVaultName "your-keyvault"
```

### Step 2: Verify Azure AD App Registration

The script automatically creates an Azure AD app with:
- **Name**: `{BaseName} CIPP MCP OAuth2 App`
- **Redirect URIs**: 
  - `https://your-mcp-server.azurewebsites.net/oauth/callback`
  - `https://default.directline.botframework.com/oauth/redirect`
  - `https://webchat.botframework.com/oauth/redirect`
- **Scopes**: `openid profile email offline_access`
- **Token Settings**: Access tokens and ID tokens enabled

### Step 3: Configure Copilot Studio Custom Connector

1. **Open Copilot Studio** and navigate to your custom connector
2. **Go to Security tab**
3. **Change Authentication Type** to `OAuth 2.0`
4. **Configure OAuth Settings**:
   - **Identity Provider**: `Azure Active Directory`
   - **Client ID**: `[From deployment output]`
   - **Client Secret**: `[From deployment output]`
   - **Authorization URL**: `https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize`
   - **Token URL**: `https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token`
   - **Refresh URL**: `https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token`
   - **Scope**: `openid profile email offline_access`

## Authentication Flow Details

### 1. Initial Tool Invocation
When a user invokes an MCP tool in Copilot Studio:
- Copilot Studio checks if the user has a valid OAuth2 token
- If no token exists, it initiates the OAuth2 flow

### 2. OAuth2 Authorization
- User is redirected to Azure AD login
- User authenticates with their organizational credentials
- Azure AD issues an authorization code

### 3. Token Exchange
- Copilot Studio exchanges the authorization code for tokens
- MCP Server receives the user's access token
- Token contains user identity and claims

### 4. CIPP API Calls
- MCP Server uses the user's token to call CIPP API
- CIPP recognizes the user and applies their specific permissions
- User only sees data they're authorized to access

## Key Benefits

### ✅ Individual User Authentication
- Each user authenticates with their own credentials
- No shared service account or keys

### ✅ Role-Based Access Control
- Users inherit their existing CIPP roles
- Permissions are enforced at the CIPP level
- No additional permission management needed

### ✅ Audit Trail
- All actions are logged under the actual user
- Complete audit trail in CIPP logs
- Compliance with security requirements

### ✅ Token Security
- Short-lived access tokens (1 hour)
- Automatic token refresh
- No long-term credential storage

## Configuration Variables

The MCP Server uses these environment variables for OAuth2:

```
AUTH_MODE=oauth2
OAUTH2_TENANT_ID={Your-Tenant-ID}
OAUTH2_CLIENT_ID={Azure-AD-App-Client-ID}
OAUTH2_CLIENT_SECRET={Azure-AD-App-Client-Secret}
OAUTH2_REDIRECT_URI=https://your-mcp-server.azurewebsites.net/oauth/callback
OAUTH2_SCOPE=openid profile email offline_access
ENABLE_USER_DELEGATION=true
TOKEN_VALIDATION_ENDPOINT=https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token
```

## Implementation Details

### Token Handling
```csharp
// Pseudo-code for token handling in MCP Server
public async Task<string> HandleMcpRequest(string toolName, object parameters, string userToken)
{
    // Validate the user token
    var tokenClaims = await ValidateTokenAsync(userToken);
    
    // Extract user identity
    var userEmail = tokenClaims["email"];
    var userObjectId = tokenClaims["oid"];
    
    // Call CIPP API with user token
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
    
    var response = await httpClient.GetAsync($"{cippApiUrl}/api/{toolName}");
    
    // CIPP validates token and applies user permissions
    return await response.Content.ReadAsStringAsync();
}
```

### Token Refresh
- Access tokens expire after 1 hour
- Refresh tokens are valid for 90 days
- Automatic refresh when tokens expire
- Re-authentication required after refresh token expiry

## Testing the Implementation

### 1. Health Check
```
GET https://your-mcp-server.azurewebsites.net/health
```

### 2. OAuth Flow Test
- Use any MCP tool in Copilot Studio
- Verify OAuth2 redirect occurs
- Complete authentication flow
- Verify tool returns user-specific data

### 3. Permission Verification
- Test with users who have different CIPP roles
- Verify each user sees only their authorized data
- Test admin vs. read-only user access

## Troubleshooting

### Common Issues

#### 1. "Authentication required" errors
- Check Azure AD app registration redirect URIs
- Verify client ID and secret in Key Vault
- Ensure user has CIPP access

#### 2. "Access denied" errors
- Verify user has appropriate CIPP roles
- Check CIPP user permissions
- Ensure user is in correct tenant

#### 3. Token refresh failures
- Check refresh token expiry
- Verify token endpoint configuration
- Re-authenticate if refresh token expired

### Debug Endpoints

```
GET /oauth/debug - Shows current auth configuration
GET /oauth/token-info - Shows token validation status
GET /api/user-info - Shows current user context
```

## Security Considerations

### ✅ Secure Token Storage
- Tokens stored in memory only
- No persistent token storage
- Automatic cleanup on session end

### ✅ Token Validation
- All tokens validated against Azure AD
- Signature verification
- Expiry checking
- Audience validation

### ✅ HTTPS Only
- All communication over HTTPS
- Secure token transmission
- Protection against token interception

### ✅ Minimal Scope
- Only request necessary OAuth2 scopes
- No excessive permissions
- User consent for scope access

## Maintenance

### Token Rotation
- Client secrets expire every 2 years
- Set calendar reminder for renewal
- Update Key Vault secret when rotating

### Monitoring
- Monitor authentication success rates
- Track token refresh failures
- Alert on authentication errors

### Updates
- Keep Azure AD app registration current
- Monitor OAuth2 specification changes
- Update redirect URIs if domains change

## Migration from Key Vault Authentication

### Pre-Migration Checklist
- [ ] Backup current Key Vault secrets
- [ ] Note current users and permissions
- [ ] Plan maintenance window
- [ ] Prepare rollback plan

### Migration Steps
1. Deploy OAuth2 infrastructure
2. Update Copilot Studio connector
3. Test with pilot users
4. Switch all users to OAuth2
5. Remove old Key Vault secrets (optional)

### Rollback Plan
- Keep original ARM template
- Maintain Key Vault secrets
- Can switch back by changing AUTH_MODE

This OAuth2 implementation provides secure, user-delegated authentication that resolves the agent hallucination issue by ensuring proper authentication with CIPP APIs, allowing each user to inherit their existing CIPP roles and permissions.
