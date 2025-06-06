using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Mcp;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CIPP_MCP.Functions
{
    public class McpBaseFunction
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string CippApiBaseUrl = "https://<your-cipp-api-url>"; // TODO: Set your CIPP-API base URL

        public McpBaseFunction(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<McpBaseFunction>();
            _httpClientFactory = httpClientFactory;
        }

        [Function("McpBase")]
        public async Task RunAsync(
            [McpTrigger] McpRequest mcpRequest,
            [McpResponse] McpResponse mcpResponse,
            FunctionContext context)
        {
            _logger.LogInformation("Received MCP request: {ToolName}", mcpRequest.ToolName);

            // Log which tool map is being used
            _logger.LogInformation("Tool map mode: {Mode}", McpToolMap.UseDynamicToolMap ? "dynamic (OpenAPI-driven)" : "static (in-code)");
            // Use dynamic or static tool map
            var toolMap = McpToolMap.GetToolMappings();
            if (!toolMap.TryGetValue(mcpRequest.ToolName, out var mapping) || !mapping.Enabled)
            {
                await mcpResponse.WriteStringAsync($"Tool '{mcpRequest.ToolName}' is not implemented or not enabled.");
                return;
            }

            // Validate and sanitize parameters
            var sanitizedParams = new Dictionary<string, string>();
            if (mcpRequest.Parameters is not null)
            {
                foreach (var kvp in mcpRequest.Parameters)
                {
                    var key = kvp.Key?.Trim();
                    if (string.IsNullOrWhiteSpace(key)) continue;
                    var value = kvp.Value?.ToString()?.Trim();
                    // Basic sanitization: remove dangerous chars, limit length
                    if (value != null)
                    {
                        value = value.Replace("\r", "").Replace("\n", "");
                        if (value.Length > 2048) value = value.Substring(0, 2048);
                    }
                    sanitizedParams[key] = value ?? string.Empty;
                }
            }

            // Validate required parameters
            var missingParams = mapping.Parameters
                .Where(p => p.Required && (!sanitizedParams.ContainsKey(p.Name) || string.IsNullOrWhiteSpace(sanitizedParams[p.Name])))
                .Select(p => p.Name)
                .ToList();
            if (missingParams.Count > 0)
            {
                mcpResponse.StatusCode = 400;
                await mcpResponse.WriteStringAsync($"Missing required parameter(s): {string.Join(", ", missingParams)}");
                return;
            }

            // Type validation for parameters
            var typeErrors = new List<string>();
            foreach (var param in mapping.Parameters)
            {
                if (!sanitizedParams.ContainsKey(param.Name)) continue; // already checked required above
                var val = sanitizedParams[param.Name];
                if (string.IsNullOrWhiteSpace(val)) continue;
                switch (param.Type.ToLowerInvariant())
                {
                    case "int":
                    case "integer":
                        if (!int.TryParse(val, out _))
                            typeErrors.Add($"Parameter '{param.Name}' must be an integer.");
                        break;
                    case "bool":
                    case "boolean":
                        if (!bool.TryParse(val, out _))
                            typeErrors.Add($"Parameter '{param.Name}' must be a boolean (true/false).");
                        break;
                    // Add more types as needed
                }
            }
            if (typeErrors.Count > 0)
            {
                mcpResponse.StatusCode = 400;
                await mcpResponse.WriteStringAsync(string.Join(" ", typeErrors));
                return;
            }

            // Log tool invocation with tool name, parameters, and user context
            _logger.LogInformation("Invoking tool: {ToolName}, Parameters: {Parameters}, UserContext: {UserContext}",
                mcpRequest.ToolName,
                JsonSerializer.Serialize(sanitizedParams),
                context.InvocationId);

            // Build the CIPP-API URL
            var cippApiUrl = $"{CippApiBaseUrl}{mapping.Endpoint}";

            HttpResponseMessage apiResponse;
            try
            {
                var httpClient = _httpClientFactory.CreateClient("CippApiClient");
                if (mapping.Method == HttpMethod.Get)
                {
                    // Add sanitized parameters as query string
                    if (sanitizedParams.Count > 0)
                    {
                        var query = string.Join("&", sanitizedParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
                        cippApiUrl += cippApiUrl.Contains("?") ? "&" : "?";
                        cippApiUrl += query;
                    }
                    apiResponse = await httpClient.GetAsync(cippApiUrl);
                }
                else if (mapping.Method == HttpMethod.Post)
                {
                    // Forward sanitized parameters as JSON body
                    var jsonBody = JsonSerializer.Serialize(sanitizedParams);
                    var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                    apiResponse = await httpClient.PostAsync(cippApiUrl, content);
                }
                else
                {
                    await mcpResponse.WriteStringAsync($"Tool '{mcpRequest.ToolName}' uses unsupported HTTP method.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CIPP-API endpoint {Endpoint}", mapping.Endpoint);
                mcpResponse.StatusCode = 502;
                await mcpResponse.WriteStringAsync($"Failed to call CIPP-API endpoint: {ex.Message}");
                return;
            }

            var apiContent = await apiResponse.Content.ReadAsStringAsync();
            mcpResponse.StatusCode = (int)apiResponse.StatusCode;
            // Stream the response content to the MCP client
            using (var apiStream = await apiResponse.Content.ReadAsStreamAsync())
            {
                await mcpResponse.WriteStreamAsync(apiStream);
            }
        }
    }
}
