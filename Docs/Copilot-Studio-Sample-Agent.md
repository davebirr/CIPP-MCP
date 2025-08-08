# Copilot Studio Sample Agent: CIPP MSP Assistant

This document provides a complete sample configuration for creating a Copilot Studio agent that leverages the CIPP-MCP server for Microsoft 365 tenant management.

## 🎯 Agent Overview

**Agent Name**: CIPP MSP Assistant  
**Purpose**: Assist Microsoft CSP partners in managing customer tenants  
**Target Users**: Managed Services Providers (MSPs) serving SMBs (25-2000 users)  
**MCP Server**: `https://cipp-mcp.roanoketechhub.com`

## 📝 Agent System Prompt

```markdown
You are a Copilot Studio agent designed to assist Microsoft CSP partners in managing customer tenants using the CyberDrain Improved Partner Portal (CIPP-MCP) hosted at https://cipp-mcp.roanoketechhub.com.

Your primary users are Managed Services Providers (MSPs) serving small to medium businesses (25–75 users) and occasionally larger organizations (up to 2000 users).

## Available Tools (15 total)

### Tenant Management (5 tools)
- **test_cipp_connection**: Test CIPP-API connectivity and authentication
- **get_tenant_dashboard**: Comprehensive tenant metrics and insights
- **list_tenants**: List all managed customer tenants with health status
- **get_tenant_details**: Detailed tenant information and configuration
- **get_tenant_domain_health**: Domain health checks and DNS validation

### User Management (5 tools)
- **list_users**: List users in a tenant with filtering options
- **get_user_details**: Comprehensive user profile information
- **list_user_licenses**: License assignments and usage analysis
- **get_user_sign_in_activity**: Sign-in logs and authentication events
- **get_user_analytics**: User insights and analytics for a tenant

### Device Management (5 tools)
- **list_devices**: List managed devices in a specific tenant
- **get_device_details**: Device specifications and status information
- **get_device_compliance**: Compliance policies and status checks
- **list_device_applications**: Installed applications inventory
- **get_device_analytics**: Device analytics and compliance overview

## Tool Parameters
- **All tools require**: `tenantFilter` (tenant ID or domain like "contoso.onmicrosoft.com")
- **User tools require**: `userId` for specific user operations
- **Device tools require**: `deviceId` for specific device operations
- **Optional filters**: Available for search and data limiting

## Agent Behavior Guidelines

### Authentication & Security
- Users authenticate via their partner Entra ID credentials with MFA
- Always validate tenant access before performing operations
- Use `test_cipp_connection` first if connectivity issues are suspected

### Communication Style
- Provide **concise summaries by default** with option for detailed reports
- Use **formatted tables or adaptive cards** when presenting data
- Support **follow-up questions** and maintain context across interactions
- Be **complementary to the CIPP web interface**, not a replacement

### Response Formatting
- Use tables for multi-item data (users, devices, tenants)
- Highlight critical issues (compliance failures, login anomalies)
- Provide actionable recommendations when appropriate
- Include relevant context (last updated, tenant name, etc.)

### Common Use Cases
1. **Tenant Health Checks**: "Show me the dashboard for Contoso"
2. **User Investigations**: "Find all users who haven't logged in for 30 days"
3. **Compliance Monitoring**: "Check device compliance for all tenants"
4. **License Optimization**: "Show me unused licenses in the Seattle office tenant"
5. **Security Analysis**: "Which users have admin roles across all tenants?"

### Error Handling
- If a tenant is not found, suggest using `list_tenants` to see available options
- For authentication errors, recommend checking CIPP credentials
- Provide specific next steps for common issues

### Best Practices
- Always specify the tenant when multiple tenants exist
- Use analytics tools for trend analysis rather than individual queries
- Batch similar requests when possible for efficiency
- Maintain professional MSP terminology and context
```

## 🔧 MCP Configuration

### Connection Settings
- **Server URL**: `https://cipp-mcp.roanoketechhub.com`
- **Protocol**: HTTPS/JSON-RPC 2.0
- **Authentication**: Managed through Azure Key Vault
- **Timeout**: 30 seconds (recommended)

### Required Headers
```json
{
  "Content-Type": "application/json",
  "Accept": "application/json"
}
```

### Sample MCP Test Call
```json
{
  "jsonrpc": "2.0",
  "method": "tools/list",
  "id": 1
}
```

## 💡 Sample Conversations

### Example 1: Tenant Overview
**User**: "Show me the dashboard for contoso.onmicrosoft.com"  
**Agent**: *Calls `get_tenant_dashboard` with tenantId="contoso.onmicrosoft.com"*  
**Response**: Formatted dashboard with key metrics, user counts, device compliance, etc.

### Example 2: User Investigation
**User**: "Find all admin users in the Seattle office tenant"  
**Agent**: 
1. *Calls `list_tenants` to identify "Seattle office" tenant*
2. *Calls `list_users` with tenantFilter and admin filter*
3. *Provides formatted table with admin users*

### Example 3: Compliance Check
**User**: "Check device compliance across all my tenants"  
**Agent**:
1. *Calls `list_tenants` to get all tenants*
2. *Calls `get_device_analytics` for each tenant*
3. *Summarizes compliance status in a table*

## 🎨 Response Templates

### Tenant Dashboard Template
```markdown
## 📊 Tenant Dashboard: {TenantName}
**Last Updated**: {Timestamp}

### Key Metrics
| Metric | Value | Status |
|--------|-------|--------|
| Total Users | {UserCount} | ✅ |
| Licensed Users | {LicensedCount} | ✅ |
| Device Compliance | {CompliancePercent}% | {Status} |
| Last Sync | {LastSync} | ✅ |

### Quick Actions
- View detailed user list
- Check device compliance
- Review domain health
```

### User List Template
```markdown
## 👥 Users in {TenantName}
**Total**: {Count} users | **Filter**: {AppliedFilter}

| Display Name | Email | Last Sign-In | License | Status |
|--------------|-------|--------------|---------|---------|
{UserRows}

💡 *Use 'get user details' for specific user information*
```

### Device Summary Template
```markdown
## 💻 Device Overview: {TenantName}
**Total Devices**: {Count} | **Compliant**: {CompliantCount} ({Percentage}%)

### Compliance Status
| Status | Count | Percentage |
|--------|-------|------------|
| Compliant | {Compliant} | {CompliancePercent}% |
| Non-Compliant | {NonCompliant} | {NonCompliancePercent}% |
| Unknown | {Unknown} | {UnknownPercent}% |

⚠️ **Action Required**: {NonCompliantCount} devices need attention
```

## 🚀 Implementation Steps

### 1. Create Copilot Studio Agent
1. Open [Copilot Studio](https://copilotstudio.microsoft.com)
2. Create new agent with "Custom" template
3. Configure authentication for your organization
4. Set the system prompt using the template above

### 2. Add MCP Custom Connector
Follow these detailed steps to connect your agent to the CIPP-MCP server:

#### Step 2.1: Access Custom Connectors
1. In your Copilot Studio agent, click **Tools** in the left navigation
2. Click **+ New Tool**
3. Select **Custom Connector** (this will open Power Apps in a new tab)

#### Step 2.2: Import MCP Connector
1. In Power Apps, click **+ New Custom Connector** 
2. Select **Import from GitHub**
3. Configure the import:
   - **Connector Type**: Custom
   - **Branch**: Dev
   - **Connector**: MCP-Streamable-HTTP
4. Click **Continue**

#### Step 2.3: Configure Connector Details
1. **General Tab**:
   - **Logo**: Upload CIPP logo (optional, improves visual identification)
   - **Description**: "CIPP-MCP Server for Microsoft 365 tenant management via Model Context Protocol"
   - **Host**: `cipp-mcp.roanoketechhub.com` *(replace with your deployment URL)*
   - **Base URL**: `/mcp` (default, do not change)

#### Step 2.4: Configure Authentication
1. Click **Security** tab
2. **Authentication Type**: Select **OAuth 2.0** (recommended for production)
   - **Alternative**: "No Authentication" for testing environments
   - **Note**: OAuth 2.0 provides better security and audit trails

#### Step 2.5: Test Connection
1. Click **Test** tab
2. Create a test operation to verify connectivity
3. Test with basic MCP call: `tools/list`

### 3. Configure Topics (Optional)
Create specific topics for common scenarios:
- **Tenant Health Check**
- **User Investigation** 
- **Device Compliance**
- **License Analysis**

### 4. Test & Validate
1. Test basic connectivity with `test_cipp_connection`
2. Verify tenant listing with `list_tenants`
3. Test user and device queries
4. Validate response formatting

## 🔧 Copilot Studio Connection Troubleshooting

### Common Issues & Solutions

#### Custom Connector Import Fails
**Problem**: GitHub import doesn't find MCP-Streamable-HTTP connector
**Solution**: 
- Verify branch is set to "Dev"
- Check GitHub repository access
- Try manual connector creation with these settings:
  - Protocol: HTTPS
  - Host: `cipp-mcp.roanoketechhub.com`
  - Base URL: `/mcp`

#### Authentication Errors
**Problem**: 401/403 errors when calling MCP tools
**Solutions**:
- **OAuth 2.0**: Verify OAuth configuration matches your Azure setup
- **No Auth**: Use for testing, but not recommended for production
- **Test**: Use `test_cipp_connection` tool to verify backend authentication

#### Connection Timeouts
**Problem**: Tools take too long or timeout
**Solutions**:
- Increase timeout in connector settings (default 30s)
- Check CIPP-MCP server health: `https://cipp-mcp.roanoketechhub.com/health`
- Verify Azure Web App is running and responsive

#### Tool Discovery Issues
**Problem**: Agent doesn't see available MCP tools
**Solutions**:
- Test connector with manual `tools/list` call
- Verify JSON-RPC 2.0 response format
- Check connector definition matches MCP specification

### Connection Validation Steps
1. **Manual Test**: Use Power Apps connector test feature
2. **Health Check**: `curl https://cipp-mcp.roanoketechhub.com/health`
3. **Tool List**: Call `tools/list` method via connector
4. **Sample Tool**: Try `test_cipp_connection` for end-to-end validation

## 🔒 Security Considerations

### Authentication Flow
1. User authenticates to Copilot Studio via Entra ID
2. Copilot Studio connects to CIPP-MCP server
3. CIPP-MCP authenticates to CIPP-API via managed identity
4. CIPP-API accesses Microsoft Graph with stored credentials

### Data Protection
- All communication over HTTPS/TLS 1.2+
- No sensitive data stored in conversation logs
- Managed identity eliminates credential exposure
- Audit trail through Application Insights

### Access Control
- Users must have appropriate CIPP permissions
- Tenant filtering prevents cross-tenant access
- Rate limiting protects against abuse
- Monitor usage through Application Insights

## 📈 Monitoring & Analytics

### Key Metrics to Track
- Tool usage frequency
- Response times
- Error rates
- User satisfaction
- Common query patterns

### Application Insights Queries
```kusto
// Tool usage by frequency
traces
| where message contains "MCP Tool Called"
| summarize count() by tostring(customDimensions.toolName)
| order by count_ desc

// Error rate analysis
exceptions
| where timestamp > ago(24h)
| summarize count() by tostring(customDimensions.tenantFilter)
```

## 🎯 Next Steps

1. **Deploy this sample agent** to test basic functionality
2. **Gather user feedback** on response formats and use cases
3. **Create specialized agents** for specific MSP workflows:
   - Security-focused agent
   - Compliance monitoring agent
   - License optimization agent
4. **Develop agent library** with industry-specific templates
5. **Integrate with existing MSP tools** and workflows

---

**Need Help?** 
- Check [CIPP-MCP Documentation](../README.md)
- Review [troubleshooting guide](../README.md#troubleshooting)
- Test server connectivity at `https://cipp-mcp.roanoketechhub.com/health`
