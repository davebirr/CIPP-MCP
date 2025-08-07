# Contributing to CIPP-MCP

Thank you for your interest in contributing to CIPP-MCP! This project extends CIPP with AI capabilities through the Model Context Protocol.

## 🚀 Quick Start for Contributors

1. **Fork the repository**
2. **Set up local development environment**:
   ```bash
   git clone https://github.com/YourUsername/CIPP-MCP.git
   cd CIPP-MCP/src
   dotnet restore
   ```
3. **Configure environment** (copy `.env.template` to `.env`)
4. **Run tests**: `dotnet test`
5. **Start development server**: `dotnet run`

## 🏗️ Project Structure

```
CIPP-MCP/
├── src/                          # Main application source
│   ├── Program.cs               # Application entry point
│   ├── Services/                # Core services
│   │   ├── AuthenticationService.cs
│   │   └── CIPPApiService.cs
│   ├── Tools/                   # MCP tools implementation
│   │   ├── TenantTools.cs
│   │   ├── UserTools.cs
│   │   └── DeviceTools.cs
│   └── Models/                  # Data models
├── deployment/                   # Azure deployment templates
├── Docs/                        # Documentation
├── Scripts/                     # Testing and utility scripts
└── .github/workflows/           # CI/CD pipelines
```

## 🛠️ Development Guidelines

### Code Style
- Follow C# coding conventions
- Use async/await for all API calls
- Include XML documentation for public methods
- Use meaningful variable and method names

### Testing
- Write unit tests for new functionality
- Test with both mock and real CIPP data
- Include PowerShell integration tests
- Verify all MCP tools work correctly

### Documentation
- Update README.md for new features
- Add XML documentation for public APIs
- Include examples in documentation
- Update AUTHENTICATION.md for auth changes

## 🔧 Adding New MCP Tools

1. **Create tool class** in `src/Tools/`
2. **Add `[McpServerTool]` attribute** to the class
3. **Implement tool methods** with proper error handling
4. **Add authentication** using `AuthenticationService`
5. **Write tests** for the new functionality
6. **Update documentation**

Example tool structure:
```csharp
[McpServerTool]
public class MyNewTool
{
    private readonly AuthenticationService _authService;
    private readonly CIPPApiService _cippService;

    public MyNewTool(AuthenticationService authService, CIPPApiService cippService)
    {
        _authService = authService;
        _cippService = cippService;
    }

    [McpServerMethod("my_new_tool")]
    public async Task<object> ExecuteAsync()
    {
        // Implementation
    }
}
```

## 🔐 Authentication and Security

- **Never commit credentials** to the repository
- **Use Key Vault** for production secrets
- **Test with multiple auth modes** (development, browser, keyvault)
- **Follow principle of least privilege**
- **Validate all inputs** from MCP calls

## 🧪 Testing Requirements

### Before Submitting PR
1. **Run all tests**: `dotnet test`
2. **Test authentication modes**: Run `Scripts/Test-Development-Modular.ps1`
3. **Verify MCP tools**: Test with VS Code MCP integration
4. **Check security**: No secrets in code or logs
5. **Test deployment**: Verify ARM template works

### Test Categories
- **Unit Tests**: Test individual components
- **Integration Tests**: Test CIPP API integration
- **End-to-End Tests**: Test full MCP workflow
- **Security Tests**: Verify authentication and authorization

## 📋 Pull Request Process

1. **Create feature branch** from `main`
2. **Implement changes** following guidelines
3. **Write/update tests** for your changes
4. **Update documentation** as needed
5. **Test thoroughly** in local environment
6. **Submit PR** with clear description

### PR Requirements
- [ ] All tests pass
- [ ] No secrets in code
- [ ] Documentation updated
- [ ] MCP tools tested
- [ ] Authentication verified
- [ ] ARM template validated

## 🐛 Bug Reports

Include in bug reports:
- **Clear description** of the issue
- **Steps to reproduce** the problem
- **Expected vs actual behavior**
- **Environment details** (local/Azure, auth mode)
- **Logs and error messages**
- **CIPP version and configuration**

## 💡 Feature Requests

For new features:
- **Describe the use case** and business value
- **Explain integration** with CIPP workflows
- **Consider security implications**
- **Provide examples** of expected behavior
- **Discuss implementation approach**

## 🏷️ Release Process

1. **Update version** in project files
2. **Update CHANGELOG.md** with changes
3. **Tag release** following semantic versioning
4. **Test deployment** with new ARM template
5. **Update documentation** for new features

## 🌐 Community Guidelines

- **Be respectful** and inclusive
- **Help others** learn and contribute
- **Share knowledge** through documentation
- **Follow CIPP community standards**
- **Report issues** constructively

## 📚 Resources

- **CIPP Documentation**: [CIPP GitHub](https://github.com/KelvinTegelaar/CIPP)
- **Model Context Protocol**: [MCP Specification](https://modelcontextprotocol.io/)
- **Azure Functions**: [Azure Documentation](https://docs.microsoft.com/azure/azure-functions/)
- **C# Guidelines**: [Microsoft Docs](https://docs.microsoft.com/dotnet/csharp/)

## 🙏 Recognition

Contributors will be recognized in:
- Repository contributors list
- Release notes
- Documentation acknowledgments
- Community discussions

Thank you for helping make CIPP-MCP better! 🚀
