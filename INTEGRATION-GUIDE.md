# CIPP-MCP Integration Guide

## Overview
CIPP-MCP is a Model Context Protocol (MCP) server that provides AI agents with access to CIPP (Cybersecurity Information Sharing Platform & Portal) functionality. This enables Copilot Studio and other AI systems to manage Microsoft 365 tenants through natural language interactions.

## Architecture

### Current Setup
- **CIPP-API**: PowerShell Azure Functions at `https://cippmboqc.azurewebsites.net`
- **CIPP Frontend**: React Static Web App at `https://lemon-hill-0df49860f.3.azurestaticapps.net`
- **CIPP-MCP**: .NET 9 MCP Server providing AI integration

### Authentication Flow
CIPP-API uses Azure Static Web Apps authentication with the following pattern:
- Header: `x-ms-client-principal` (Base64 encoded JSON)
- Required roles: `admin`, `editor`, `readonly`, `authenticated`
- Identity provider: Azure AD (`aad`)

## Key CIPP-API Endpoints Discovered

Based on frontend analysis, these are the most important endpoints for our MCP tools:

### Core Endpoints
- `/api/me` - Get current user information
- `/api/ListTenants` - List managed tenants
- `/api/ListUsers` - List users in tenants
- `/api/ListDevices` - List managed devices
- `/api/GetCippAlerts` - Get system alerts
- `/api/ListUserSettings` - Get user preferences

### Tenant Management
- `/api/ListTenants?AllTenantSelector=true` - Include all tenants
- `/api/ListTenants?Mode=TenantList` - Standard tenant listing
- `/api/ListDomainHealth` - Domain health checks

### User Management
- `/api/ListUsers?UserId={id}&tenantFilter={tenant}` - Specific user
- `/api/ListMFAUsers` - MFA status report
- `/api/ListSignIns` - Sign-in activity
- `/api/ListUserSigninLogs` - Detailed sign-in logs

### Device Management
- `/api/ListDevices?tenantFilter={tenant}` - Devices in tenant
- `/api/ListDeviceCompliance` - Compliance status
- `/api/ListDeviceApplications` - Installed applications

### Security & Compliance
- `/api/ListGraphRequest` - Risk detections
- `/api/ListLicenses` - License information
- `/api/ListBreachesTenant` - Security breaches
- `/api/ListStandards` - Applied standards

## Planned MCP Tools

### 1. Tenant Dashboard Tool
**Purpose**: Provide comprehensive tenant overview for Copilot Studio
**Functionality**:
- Tenant summary (users, devices, licenses)
- Security posture (MFA status, risk detections)
- Compliance status (standards applied)
- Recent alerts and activities

**Implementation**:
```csharp
[Description("Get comprehensive tenant dashboard with key metrics")]
public async Task<string> GetTenantDashboard(
    [Description("Tenant ID or domain")] string tenantId)
```

### 2. Enhanced Tenant Tools
- `ListTenants()` - List all managed tenants with filtering
- `GetTenantDetails()` - Detailed tenant information
- `GetTenantDomainHealth()` - Domain health status
- `GetTenantSecurityScore()` - Security posture summary

### 3. Enhanced User Tools
- `ListUsers()` - Users with role and license info
- `GetUserDetails()` - Complete user profile
- `GetUserSecurityStatus()` - MFA, sign-ins, risk
- `GetUserDevices()` - User's managed devices

### 4. Enhanced Device Tools
- `ListDevices()` - Managed devices with compliance
- `GetDeviceDetails()` - Device specifications and status
- `GetDeviceCompliance()` - Compliance policies and status
- `GetDeviceApplications()` - Installed applications

### 5. Security & Compliance Tools
- `GetSecurityAlerts()` - Recent security events
- `GetRiskDetections()` - User and sign-in risks
- `GetComplianceStatus()` - Standards and policies
- `GetLicenseUsage()` - License allocation and usage

## Quick Start

### 1. Configure Authentication
Update `appsettings.Development.json` with your Azure details:
```json
{
  "Azure": {
    "UserEmail": "your-email@yourdomain.com",
    "UserId": "your-azure-user-id",
    "TenantId": "your-azure-tenant-id"
  }
}
```

### 2. Test Connection
Run the CIPP-MCP server and test with the `TestCippConnection` tool:
```bash
cd CIPP-MCP-v2/src
dotnet run
```

### 3. Verify API Access
Test endpoints in order:
1. `/api/me` - Verify authentication
2. `/api/ListTenants` - Verify tenant access
3. `/api/ListUsers` - Verify user data access

## Integration with Copilot Studio

### MCP Configuration
- **Server URL**: `http://localhost:5000/mcp` (development)
- **Transport**: HTTP with Server-Sent Events
- **Authentication**: None (handled internally by CIPP-MCP)

### Example Prompts for Copilot Studio
- "Show me a dashboard for tenant contoso.com"
- "List all users in the tenant with MFA disabled"
- "What devices are non-compliant in our organization?"
- "Get security alerts for the last 24 hours"

## Development Roadmap

### Phase 1: Core Functionality ✅
- [x] Basic MCP server setup
- [x] Authentication service
- [x] Connection to CIPP-API
- [x] Test tools for validation

### Phase 2: Essential Tools 🔄
- [ ] Tenant dashboard tool
- [ ] Enhanced tenant management
- [ ] User management tools
- [ ] Device management tools

### Phase 3: Advanced Features
- [ ] Security monitoring tools
- [ ] Compliance reporting
- [ ] Automated remediation suggestions
- [ ] Executive reporting

### Phase 4: Production Deployment
- [ ] Azure App Service deployment
- [ ] Production authentication
- [ ] Monitoring and logging
- [ ] Performance optimization

## Troubleshooting

### Common Issues
1. **401 Unauthorized**: Check Azure authentication configuration
2. **403 Forbidden**: Verify user has required CIPP roles
3. **500 Server Error**: Check CIPP-API logs for backend issues
4. **Connection Timeout**: Verify network connectivity to Azure

### Debug Tools
- Use `TestCippConnection` tool to verify basic connectivity
- Check server logs for detailed error messages
- Test endpoints directly with tools like Postman
- Verify Azure Static Web App authentication headers

## Security Considerations

### Authentication
- Never store credentials in source code
- Use Azure Key Vault for production secrets
- Implement proper role-based access control
- Regular rotation of authentication tokens

### API Security
- All calls go through authenticated CIPP-API
- No direct Microsoft Graph API calls from MCP
- Audit logging for all MCP tool usage
- Rate limiting to prevent abuse

## Performance Optimization

### Caching Strategy
- Cache tenant lists for 5 minutes
- Cache user data for 2 minutes
- Cache device data for 10 minutes
- Invalidate cache on data modifications

### Connection Pooling
- Reuse HTTP connections to CIPP-API
- Configure appropriate timeouts
- Implement retry logic for transient failures
- Monitor connection health

---

Last Updated: August 6, 2025
Version: 1.0
