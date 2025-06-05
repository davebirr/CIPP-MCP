# CIPP MCP Integration Project

## Project Overview and Goals
The goal of this project is to enable M365 Copilot to use a specialized agent that communicates with the CIPP project using the Model Context Protocol (MCP). This will allow Copilot to solve tasks or fetch data through the existing CIPP API.

> **References:**
> - [MCP documentation](https://modelcontextprotocol.io/introduction)
> - [Microsoft MCP Blog: Introducing Model Context Protocol (MCP) in Copilot Studio](https://www.microsoft.com/en-us/microsoft-copilot/blog/copilot-studio/introducing-model-context-protocol-mcp-in-copilot-studio-simplified-integration-with-ai-apps-and-agents)
> - [CIPP Feature Request #3975](https://github.com/KelvinTegelaar/CIPP/issues/3975)

## Current Architecture
- **CIPP:** Node.js (JavaScript) user interface hosted as an Azure Static Web App.
- **CIPP-API:** Azure Function App written in PowerShell, primarily making calls to the Microsoft Graph API and other Microsoft APIs to fetch data and perform tasks for the end user using the static web app.

## Decision to Add a New CIPP-MCP Azure Function App
- **Language:** C# (.NET)
- **Purpose:** Implement MCP streamable HTTP endpoints to facilitate communication between M365 Copilot and the CIPP project.

## Integration Approach
1. **M365 Copilot (via MCP agent)** sends a request to the MCP streamable HTTP endpoint.
2. **MCP Endpoint (C#)** receives the request, parses the MCP payload, and determines which PowerShell function(s) to call.
3. **MCP Endpoint** calls the appropriate PowerShell function(s) via HTTP.
4. **PowerShell Function** executes the business logic and returns results.
5. **MCP Endpoint** streams the response back to the agent, following MCP streaming conventions.

## Current HTTP Endpoints in CIPP-API
- AddChocoApp
- AddMSPApp
- AddOfficeApp
- BestPracticeAnalyser_List
- DomainAnalyser_List
- ExecAccessChecks
- ExecAssignApp
- ExecBackendURLs
- ExecBECCheck
- GetDashboard
- GetVersion
- ListAllTenantDeviceCompliance
- ListAPDevices
- ListAppStatus
- ListApplicationQueue
- ListApps
- ListAppsRepository
- ListAutopilotconfig
- ListContacts
- ListDefenderState
- ListDevices
- ListDomainHealth
- ListDomains
- ListLogs
- ListMailboxMobileDevices
- ListOAuthApps
- ListServiceHealth
- ListSharepointSettings
- ListSites
- ListTeams
- ListTeamsActivity
- ListTenants
- ListUserDevices
- ListUserGroups
- ListUsers
- RemoveAPDevice
- RemoveApp
- RemoveQueuedApp

## Key Architectural and Security Notes
- **Language Choice:** C# is recommended for the MCP endpoint due to its robust support for HTTP streaming, strong typing, and native Azure integration.
- **Integration Method:** The MCP endpoint will call PowerShell functions via HTTP, ensuring decoupled, language-agnostic communication.
- **Security:** Use Azure Managed Identity or API keys for secure, internal communication between function apps.
- **Deployment:** Deploy the MCP endpoint as a separate Azure Function App in the same subscription and resource group for independent scaling and optimization.