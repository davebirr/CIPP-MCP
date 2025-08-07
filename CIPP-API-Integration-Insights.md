# CIPP-MCP API Integration Insights

## Overview
This document captures key insights from analyzing the CIPP frontend to improve our MCP server implementation for better compatibility with the CIPP-API.

## Authentication Pattern
- **Type**: Azure Static Web Apps authentication
- **Flow**: Users authenticate via `/.auth/login/aad`
- **Headers**: Authentication passed via `x-ms-client-principal` headers
- **Roles**: `admin`, `editor`, `readonly`, `authenticated`, `anonymous`
- **Profile Endpoint**: `/api/me` returns user information

## Core API Endpoints & Parameters

### Tenant Management
| Endpoint | Parameters | Description |
|----------|------------|-------------|
| `/api/ListTenants` | `AllTenantSelector=true/false`, `Mode=TenantList`, `tenantFilter` | List all customer tenants |
| `/api/ListTenants` | `tenantFilter={tenantId}` | Get specific tenant details |
| `/api/ListDomainHealth` | `TenantDomain={domain}` | Check domain health |

### User Management  
| Endpoint | Parameters | Description |
|----------|------------|-------------|
| `/api/ListUsers` | `tenantFilter={tenantId}` | List users in tenant |
| `/api/ListUsers` | `UserId={userId}`, `tenantFilter={tenantId}` | Get specific user details |
| `/api/ListMFAUsers` | `tenantFilter={tenantId}` | MFA report for users |
| `/api/ListUserSigninLogs` | `UserId={userId}`, `tenantFilter={tenantId}`, `top={count}` | User sign-in activity |

### Device Management
| Endpoint | Parameters | Description |
|----------|------------|-------------|
| `/api/ListDevices` | `tenantFilter={tenantId}` | List managed devices |
| `/api/ListGraphRequest` | Custom Graph API queries | Risk detections, compliance |

### Security & Compliance
| Endpoint | Parameters | Description |
|----------|------------|-------------|
| `/api/ListGraphRequest` | Graph API for risk detections | Security insights |
| `/api/ListSignIns` | `tenantFilter={tenantId}` | Sign-in reports |
| `/api/GetCippAlerts` | Tenant-specific alerts | CIPP alerts and notifications |

## Tenant Dashboard Requirements

### Key Metrics to Display
1. **Tenant Overview**
   - Tenant name and domain
   - Total users count
   - Total devices count
   - License summary

2. **Security Insights**
   - Users with risk detections
   - Devices with compliance issues
   - Recent sign-in activity
   - MFA adoption rate

3. **CIPP Management**
   - Applied standards
   - Recent alerts
   - Backup status
   - Policy compliance

### Recommended MCP Tools

#### 1. Tenant Summary Tool
- **Purpose**: Get high-level tenant overview
- **Data**: User count, device count, primary domain, license info
- **API Calls**: `/api/ListTenants`, `/api/ListUsers`, `/api/ListDevices`

#### 2. Security Dashboard Tool  
- **Purpose**: Security and risk overview
- **Data**: Risk detections, MFA status, compliance issues
- **API Calls**: `/api/ListMFAUsers`, `/api/ListGraphRequest`, `/api/ListSignIns`

#### 3. User Analytics Tool
- **Purpose**: User activity and status insights
- **Data**: Active users, last sign-in, license assignments
- **API Calls**: `/api/ListUsers`, `/api/ListUserSigninLogs`

#### 4. Device Compliance Tool
- **Purpose**: Device management overview
- **Data**: Compliant/non-compliant devices, enrollment status
- **API Calls**: `/api/ListDevices`, `/api/ListGraphRequest`

#### 5. CIPP Standards Tool
- **Purpose**: Applied standards and compliance
- **Data**: Standards applied, compliance status, recommendations
- **API Calls**: CIPP-specific endpoints for standards

## Implementation Notes

### Query Parameter Patterns
- Use `tenantFilter` for tenant-specific queries
- Support `AllTenantSelector=true` for multi-tenant operations
- Include `top` parameter for pagination
- Use `UserId` for user-specific details

### Response Handling
- Expect JSON responses with consistent structures
- Handle error responses gracefully
- Support bulk operations where available
- Cache responses appropriately for performance

### Authentication Headers
Our `AuthenticationService` should generate headers matching SWA pattern:
```json
{
  "x-ms-client-principal": "{base64-encoded-user-info}"
}
```

## Next Steps
1. Update existing MCP tools to match these patterns
2. Implement tenant dashboard aggregation tools
3. Add comprehensive error handling
4. Test with Copilot Studio integration
5. Enhance authentication service for production use
