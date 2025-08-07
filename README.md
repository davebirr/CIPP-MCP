# CIPP-MCP: Model Context Protocol Server for CIPP

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdavebirr%2FCIPP-MCP%2Fmain%2Fdeployment%2FAzureDeploymentTemplate.json)

CIPP-MCP is a Model Context Protocol server that enables AI assistants to securely interact with CIPP (CyberDrain Improved Partner Portal) functionality. This allows Copilot Studio, Claude Desktop, and other AI tools to perform Microsoft 365 tenant management operations through natural language commands.

> **References:**
> - [MCP Documentation](https://modelcontextprotocol.io/introduction)
> - [Microsoft MCP Blog: Introducing Model Context Protocol (MCP) in Copilot Studio](https://www.microsoft.com/en-us/microsoft-copilot/blog/copilot-studio/introducing-model-context-protocol-mcp-in-copilot-studio-simplified-integration-with-ai-apps-and-agents)
> - [CIPP Feature Request #3975](https://github.com/KelvinTegelaar/CIPP/issues/3975)
> - [MCP tool bindings for Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-mcp?pivots=programming-language-csharp)

## 🚀 Quick Deploy to Azure

CIPP-MCP follows the same **fork-and-deploy pattern** as CIPP-API, ensuring consistency with the CIPP ecosystem.

### Why Separate Repository?
- **Different Technology Stacks**: PowerShell vs C# .NET with different build systems
- **Independent Deployment**: Azure Functions vs different hosting options
- **Independent Release Cycles**: Each component evolves separately
- **Fork-and-Deploy Alignment**: Matches CIPP's established user deployment pattern

1. **Fork this repository** to your GitHub account
2. **Deploy to Azure** using the button above (defaults to this repository, update if you forked)
3. **Configure authentication** by adding your CIPP credentials to Key Vault
4. **Connect to Copilot Studio** or VS Code for AI-powered tenant management

## 🏗️ Architecture

CIPP-MCP integrates seamlessly with the existing CIPP ecosystem by acting as an authenticated proxy between AI agents and the CIPP-API.

### Current CIPP Architecture
- **CIPP Frontend**: React SPA hosted as Azure Static Web App
- **CIPP-API**: PowerShell Azure Functions for Microsoft Graph API operations

### CIPP-MCP Integration
- **Deployment**: Azure Function App alongside existing CIPP infrastructure
- **Language**: C# (.NET 9)
- **Protocol**: Model Context Protocol over HTTP
- **Authentication**: Leverages existing CIPP Static Web Apps authentication

![CIPP-MCP Architecture Diagram](Docs/images/CIPP-MCP_diagram.png)

### Integration Flow
1. **AI Agent/Copilot** sends MCP request to CIPP-MCP endpoint
2. **CIPP-MCP Server** receives request and authenticates using SWA cookies
3. **CIPP-MCP** calls appropriate CIPP-API PowerShell functions via HTTP
4. **CIPP-API** executes business logic and Microsoft Graph operations
5. **CIPP-MCP** streams formatted response back to AI agent

### Security Model
- **Controlled Gateway**: CIPP-MCP exposes only explicitly configured tools
- **Authentication Proxy**: Uses existing CIPP Static Web Apps authentication
- **No Direct Access**: AI agents cannot directly access CIPP-API
- **Audit Trail**: All requests logged through Application Insights

- **Azure Function App**: Hosts the MCP server with automatic scaling
- **Key Vault Integration**: Secure storage for CIPP authentication
- **Static Web Apps Proxy**: Leverages existing CIPP authentication flow
- **Application Insights**: Comprehensive monitoring and logging

## 🧪 Local Development

1. **Clone and setup**:
   ```bash
   git clone https://github.com/davebirr/CIPP-MCP.git
   cd CIPP-MCP/src
   dotnet restore
   ```

2. **Configure environment** (copy `.env.template` to `.env`):
   ```bash
   AUTH_MODE=browser
   CIPP_API_BASE_URL=https://your-cipp.azurestaticapps.net
   CIPP_USER_EMAIL=your-email@domain.com
   BROWSER_AUTH_COOKIE=your-swa-cookie
   ```

3. **Run locally**:
   ```bash
   dotnet run
   ```

4. **Test the server**:
   - Health check: http://localhost:5000/health
   - VS Code: Add MCP Server with URL `http://localhost:5000`

## 📋 Prerequisites

- **Existing CIPP deployment** in Azure (required)
- **Azure subscription** with deployment permissions
- **GitHub account** for repository forking
- **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** for local development

### Local Development Requirements
- .NET 9 SDK with explicit reference to `Microsoft.Extensions.Hosting` version `9.0.0-preview.3.24172.9`
- Azure CLI (for deployment)
- PowerShell 7+ (for testing scripts)
- Visual Studio or VS Code (recommended)

## 🛠️ Available MCP Tools

CIPP-MCP provides 15 comprehensive tools for Microsoft 365 tenant management, carefully selected from the 200+ CIPP-API endpoints to provide the most valuable AI-assisted operations.

### Tenant Management
- **ListTenants**: Get all customer tenants with health status
- **GetTenantDetails**: Detailed tenant information and configuration  
- **GetTenantDomainHealth**: Domain health checks and DNS validation

### User Management
- **ListUsers**: List users across tenants with filtering
- **GetUserDetails**: Comprehensive user profile information
- **ListUserLicenses**: License assignments and usage analysis
- **GetUserSignInActivity**: Sign-in logs and authentication events

### Device Management
- **ListDevices**: Managed devices across all tenants
- **GetDeviceDetails**: Device specifications and status
- **GetDeviceCompliance**: Compliance policies and status
- **ListDeviceApplications**: Installed applications inventory

### Security & Monitoring
- **TestCIPPConnection**: Health checks and API validation
- **GetDashboardData**: Overview metrics and alerts
- **ListAlerts**: Security alerts and recommendations
- **GetUserRisks**: Identity protection and risk scores

### AI Use Cases
The tools are designed for complex AI-driven scenarios such as:
- **Complex Reporting**: "Every P1 licensed user that logged in last Friday from an iPad using Outlook"
- **Health Check Automation**: Automated tenant health assessments
- **Security Analysis**: Risk pattern detection across multiple tenants
- **Compliance Monitoring**: Automated compliance reporting and alerting

> **Available CIPP Endpoints**: CIPP-API provides 200+ HTTP endpoints across tenant management, user administration, device management, security, and Exchange operations. The current MCP implementation focuses on read-only operations for safety, with the most valuable endpoints for AI-assisted management. See the [original project documentation](https://github.com/KelvinTegelaar/CIPP) for the complete endpoint catalog.

## 🔐 Authentication Modes

CIPP-MCP supports multiple authentication methods:

- **Browser Mode** (Development): Uses Static Web Apps authentication cookie
- **Key Vault Mode** (Production): Retrieves credentials from Azure Key Vault  
- **Development Mode** (Testing): Mock authentication for local testing

## 📋 Prerequisites

- Existing CIPP deployment in Azure
- Azure subscription with deployment permissions
- GitHub account for repository forking

## 🚀 Production Deployment

### Option 1: One-Click Deploy
1. Fork this repository
2. Update the "Deploy to Azure" button URL with your username
3. Click deploy and fill in parameters

### Option 2: Manual Deployment
See [deployment/README.md](deployment/README.md) for detailed instructions

## 🔧 Configuration

After deployment, configure authentication in Key Vault:

```bash
# Required secrets for production
CIPP-APPLICATION-ID=your-app-id
CIPP-APPLICATION-SECRET=your-app-secret  
CIPP-REFRESH-TOKEN=your-refresh-token
```

## 📊 Monitoring

- **Application Insights**: Automatic logging and performance monitoring
- **Health Endpoints**: Built-in health checks at `/health`
- **Error Tracking**: Comprehensive error logging and alerting

## 🔄 CI/CD Pipeline

The ARM template includes:
- Continuous deployment from GitHub
- Automatic updates on repository changes
- Blue-green deployment for zero downtime

## 🛡️ Security Features

- **Managed Identity**: No stored credentials in application
- **Key Vault Integration**: Secure secrets management
- **HTTPS Enforcement**: TLS 1.2+ required
- **Authentication Required**: All endpoints protected

## 🧪 Testing & Development

### Running Tests
This project uses [xUnit](https://xunit.net/) for unit and integration testing.

```bash
# Run all tests
dotnet test

# Add xUnit to test projects
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
```

### Development Testing Scripts
```bash
# Test authentication
.\Scripts\Test-Authentication.ps1

# Test all MCP tools
.\Scripts\Test-Development-Modular.ps1

# Validate deployment
.\Scripts\Test-Production.ps1
```

### Installing Preview Packages
For advanced AI schema support:
```bash
dotnet add package Microsoft.Extensions.AI --prerelease
```

Run the comprehensive test suite:

```bash
# Test authentication
.\Scripts\Test-Authentication.ps1

# Test all MCP tools
.\Scripts\Test-Development-Modular.ps1

# Validate deployment
.\Scripts\Test-Production.ps1
```

## 📚 Documentation

- [Authentication Guide](AUTHENTICATION.md)
- [Integration Guide](INTEGRATION-GUIDE.md) 
- [Deployment Instructions](deployment/README.md)
- [API Documentation](Docs/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📞 Support

- **Issues**: Use GitHub Issues for bug reports and feature requests
- **Documentation**: Check the Docs/ directory for detailed guides
- **Community**: Join the [CIPP Discord community](https://discord.gg/cipp)
- **CIPP Project**: Visit [CIPP on GitHub](https://github.com/KelvinTegelaar/CIPP)

## 🤝 Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

1. Fork the repository
2. Create a feature branch
3. Make your changes with tests
4. Submit a pull request

## 📄 License

This project follows the same license terms as CIPP. Please see the main [CIPP repository](https://github.com/KelvinTegelaar/CIPP) for license details.

## 🏷️ Version

**Current Version**: 1.0.1

### Release Notes
- ✅ Production-ready deployment with ARM templates
- ✅ Comprehensive authentication system (browser, development, Key Vault)
- ✅ 15 MCP tools covering tenant, user, device, and security management
- ✅ CI/CD pipeline with GitHub Actions
- ✅ Complete documentation and testing framework

---

**CIPP-MCP v1.0.1** - Extending CIPP with AI capabilities through the Model Context Protocol.

> This is a community project that enhances CIPP with AI integration. An existing CIPP deployment is required for operation.
