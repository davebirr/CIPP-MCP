# CIPP-MCP Copilot Studio Quick Setup

## 🚀 Quick Start Guide

### 1. Create Copilot Studio Agent
1. Go to [Copilot Studio](https://copilotstudio.microsoft.com)
2. Create new agent → Custom template
3. Set system prompt (see template below)

### 2. Add MCP Custom Connector
**Detailed Steps**:
1. **Tools** (left nav) → **+ New Tool** → **Custom Connector**
2. **+ New Custom Connector** → **Import from GitHub**
3. **Configure Import**:
   - Connector Type: **Custom**
   - Branch: **Dev**
   - Connector: **MCP-Streamable-HTTP**
   - Click **Continue**
4. **Configure Settings**:
   - Logo: Upload CIPP logo (optional)
   - Description: "CIPP-MCP Server for M365 tenant management"
   - **Host**: `cipp-mcp.roanoketechhub.com` *(your deployment)*
   - **Base URL**: `/mcp`
5. **Security**: Select **OAuth 2.0** (recommended)

### 3. Connection Details
- **Server URL**: `https://cipp-mcp.roanoketechhub.com`
- **Protocol**: JSON-RPC 2.0 over HTTPS
- **Authentication**: OAuth 2.0 or No Authentication (testing)
- **Base Path**: `/mcp`

### 2. Test Connection
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "test_cipp_connection",
    "arguments": {}
  },
  "id": 1
}
```

### 3. System Prompt Template
```markdown
You are a CIPP MSP Assistant for Microsoft CSP partners managing customer tenants via https://cipp-mcp.roanoketechhub.com.

## Available Tools (15 total)
**Tenant Management**: test_cipp_connection, get_tenant_dashboard, list_tenants, get_tenant_details, get_tenant_domain_health
**User Management**: list_users, get_user_details, list_user_licenses, get_user_sign_in_activity, get_user_analytics  
**Device Management**: list_devices, get_device_details, get_device_compliance, list_device_applications, get_device_analytics

## Parameters
- All tools need `tenantFilter` (domain like "contoso.onmicrosoft.com")
- User tools need `userId`, device tools need `deviceId`
- Use optional filters for search/limiting results

## Behavior
- Concise summaries by default, detailed on request
- Format data in tables/adaptive cards
- Maintain context across conversations
- Complement the CIPP web interface
```

### 4. Sample Tool Calls

#### List Tenants
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call", 
  "params": {
    "name": "list_tenants",
    "arguments": {}
  },
  "id": 2
}
```

#### Get Tenant Dashboard  
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "get_tenant_dashboard", 
    "arguments": {
      "tenantId": "contoso.onmicrosoft.com"
    }
  },
  "id": 3
}
```

#### List Users
```json
{
  "jsonrpc": "2.0", 
  "method": "tools/call",
  "params": {
    "name": "list_users",
    "arguments": {
      "tenantFilter": "contoso.onmicrosoft.com"
    }
  },
  "id": 4
}
```

### 5. Common Conversation Flows

**Health Check**: 
1. `test_cipp_connection` → `list_tenants` → `get_tenant_dashboard`

**User Investigation**: 
1. `list_tenants` (find tenant) → `list_users` → `get_user_details`

**Compliance Review**: 
1. `list_tenants` → `get_device_analytics` (per tenant) → `get_device_compliance`

### 6. Response Formatting Tips
- Use tables for multi-item results
- Highlight issues with ⚠️ or ❌
- Include actionable next steps
- Show timestamps and context
- Offer follow-up options

### 7. Error Handling
- **Invalid tenant**: Use `list_tenants` to show options
- **Connection issues**: Try `test_cipp_connection` first  
- **Missing parameters**: Prompt for required tenantFilter/userId/deviceId

---
📝 **Full Documentation**: [Copilot Studio Sample Agent](Copilot-Studio-Sample-Agent.md)
