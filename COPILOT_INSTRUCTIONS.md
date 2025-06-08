# Copilot Integration Instructions for CIPP-MCP
## MCP C# SDK Reference
- Official SDK: https://github.com/modelcontextprotocol/csharp-sdk
- API Docs: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.html
- All code should use the SDK types, attributes, and helpers for Model Context Protocol integration.

## Overview
This Azure Function App implements a Model Context Protocol (MCP) server that enables Microsoft 365 Copilot (and other MCP clients) to securely interact with the CIPP API. The MCP server exposes only explicitly configured tools and data, acting as a secure gateway between Copilot and your CIPP environment.

---

## Key Principles

- **Least Privilege:** Only expose endpoints and data required for Copilot scenarios.
- **Read-Only by Default:** Start with read-only endpoints; add write capabilities only after review.
- **Explicit Tool Registration:** Register each MCP tool (endpoint) with clear descriptions and scopes.
- **No Proxying of Third-Party APIs:** The MCP server must never proxy arbitrary requests.
- **Authentication & Authorization:** Enforce OAuth 2.1 and Azure AD authentication for all MCP endpoints.
- **Auditing:** Log all MCP requests and responses for traceability.
- **Input Validation:** Strictly validate all incoming payloads and parameters.

---

## MCP Tool Registration

- **Describe Each Tool:** For every MCP-exposed function, provide a clear, user-friendly description and expected parameters.
- **Scope Tools:** Limit tool access to only the required tenants, users, or data.
- **Streaming Support:** Where possible, implement MCP streaming conventions for large or long-running responses.

---

## Security Best Practices

- **Dynamic Client Registration:** Use dynamic registration for MCP clients where supported.
- **Token Scoping:** Enforce strict OAuth scopes and validate all tokens.
- **Session Isolation:** Never leak tokens or session data to clients.
- **CORS:** Restrict allowed origins to trusted Copilot hosts.
- **Rate Limiting:** Apply rate limits to prevent abuse.

---

## Example: Tool Registration Template

```json
{
  "toolName": "ListUsers",
  "description": "Retrieves a list of users for the specified tenant.",
  "parameters": [
    { "name": "TenantFilter", "type": "string", "required": true, "description": "Tenant identifier" }
  ],
  "scope": "read:user",
  "streaming": false
}
```

---

## Endpoint Design

- **Follow OpenAPI:** Ensure all endpoints are described in your OpenAPI spec.
- **Consistent Naming:** Use clear, consistent naming for all MCP tools.
- **Error Handling:** Return meaningful error messages and MCP-compliant error codes.

---

## Deployment

- **Azure Container App:** Deploy using Docker & Azure best practices (slot-based deployment, managed identity, etc.).
- **Secrets Management:** Store secrets in Azure Key Vault or Github secrets, never in code or config files.
- **Monitoring:** Enable Application Insights for logging and monitoring.

---

## References

- [Model Context Protocol (MCP) Documentation](https://modelcontextprotocol.io/introduction)
- [Azure Functions MCP Bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-mcp?pivots=programming-language-csharp)
- [CIPP Project](https://github.com/KelvinTegelaar/CIPP)
- [Microsoft Copilot Studio Blog](https://www.microsoft.com/en-us/microsoft-copilot/blog/copilot-studio/introducing-model-context-protocol-mcp-in-copilot-studio-simplified-integration-with-ai-apps-and-agents)

---

## Contact

For questions or to request new MCP tools, open an issue in the repository or contact the project maintainers.
