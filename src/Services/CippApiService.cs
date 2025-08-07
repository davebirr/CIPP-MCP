using System.Text.Json;

namespace CippMcp.Services;

public class CippApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CippApiService> _logger;

    public CippApiService(HttpClient httpClient, AuthenticationService authService, IConfiguration configuration, ILogger<CippApiService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
        
        // Configure base address for CIPP-API
        var baseUrl = configuration["CIPP_API_BASE_URL"] ?? "https://cippmboqc.azurewebsites.net";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        try
        {
            var uri = BuildUri(endpoint, queryParams);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            
            // Add authentication headers
            await AddAuthenticationHeadersAsync(request);

            _logger.LogDebug("Making CIPP-API request to: {Uri}", uri);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                _logger.LogWarning("CIPP-API request failed: {StatusCode} {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return default;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CIPP-API endpoint: {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<string> GetStringAsync(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        try
        {
            var uri = BuildUri(endpoint, queryParams);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            
            // Add authentication headers
            await AddAuthenticationHeadersAsync(request);

            _logger.LogDebug("Making CIPP-API request to: {Uri}", uri);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogWarning("CIPP-API request failed: {StatusCode} {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CIPP-API endpoint: {Endpoint}", endpoint);
            return $"Error: {ex.Message}";
        }
    }

    private string BuildUri(string endpoint, Dictionary<string, string>? queryParams)
    {
        var uri = $"/api/{endpoint.TrimStart('/')}";
        
        if (queryParams != null && queryParams.Any())
        {
            var queryString = string.Join("&", 
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            uri += $"?{queryString}";
        }
        
        return uri;
    }

    private async Task AddAuthenticationHeadersAsync(HttpRequestMessage request)
    {
        // Get authentication mode and configure accordingly
        var authMode = _configuration["AUTH_MODE"] ?? "development";
        
        switch (authMode.ToLower())
        {
            case "browser":
                await AddBrowserAuthenticationAsync(request);
                break;
                
            case "keyvault":
            case "development":
            default:
                await AddSwaAuthenticationAsync(request);
                break;
        }
        
        // Add standard headers
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CIPP-MCP/1.0");
    }

    private async Task AddBrowserAuthenticationAsync(HttpRequestMessage request)
    {
        var authCookie = _authService.GetBrowserAuthCookie();
        if (!string.IsNullOrEmpty(authCookie))
        {
            request.Headers.Add("Cookie", $"StaticWebAppsAuthCookie={authCookie}");
            var swaUrl = _configuration["CIPP_SWA_URL"] ?? "https://lemon-hill-0df49860f.3.azurestaticapps.net";
            request.Headers.Add("Referer", swaUrl);
            request.Headers.Add("Origin", swaUrl);
            _logger.LogDebug("Added browser cookie authentication headers");
        }
    }

    private async Task AddSwaAuthenticationAsync(HttpRequestMessage request)
    {
        // For development and Key Vault modes, we'll simulate SWA authentication
        var principal = await _authService.GetClientPrincipalAsync();
        
        if (principal != null)
        {
            request.Headers.Add("x-ms-client-principal-idp", "aad");
            request.Headers.Add("x-ms-client-principal", principal);
            _logger.LogDebug("Added SWA authentication headers");
        }
    }
}
