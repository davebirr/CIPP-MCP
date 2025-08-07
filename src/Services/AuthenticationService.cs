using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace CippMcp.Services;

public class AuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private SecretClient? _keyVaultClient;
    private Dictionary<string, string>? _cachedSecrets;

    public AuthenticationService(IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        InitializeKeyVaultClient();
    }

    private void InitializeKeyVaultClient()
    {
        try
        {
            var keyVaultUrl = _configuration["KEY_VAULT_URL"] ?? "https://cippmboqc.vault.azure.net/";
            var clientId = _configuration["AZURE_CLIENT_ID"];
            var clientSecret = _configuration["AZURE_CLIENT_SECRET"];
            var tenantId = _configuration["AZURE_TENANT_ID"];

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenantId))
            {
                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                _keyVaultClient = new SecretClient(new Uri(keyVaultUrl), credential);
                _logger.LogDebug("Key Vault client initialized with service principal");
            }
            else
            {
                // Fallback to default Azure credential (Azure CLI, Managed Identity, etc.)
                var credential = new DefaultAzureCredential();
                _keyVaultClient = new SecretClient(new Uri(keyVaultUrl), credential);
                _logger.LogDebug("Key Vault client initialized with default credential");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize Key Vault client. Falling back to development mode.");
            _keyVaultClient = null;
        }
    }

    public async Task<Dictionary<string, string>> GetCippSecretsAsync()
    {
        if (_cachedSecrets != null)
        {
            return _cachedSecrets;
        }

        var secrets = new Dictionary<string, string>();

        if (_keyVaultClient != null)
        {
            try
            {
                _logger.LogDebug("Retrieving CIPP secrets from Key Vault");
                
                // Get the secrets needed for CIPP authentication
                var secretNames = new[] { "ApplicationId", "ApplicationSecret", "RefreshToken", "tenantid" };
                
                foreach (var secretName in secretNames)
                {
                    try
                    {
                        var secret = await _keyVaultClient.GetSecretAsync(secretName);
                        secrets[secretName] = secret.Value.Value;
                        _logger.LogDebug("Retrieved secret: {SecretName}", secretName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retrieve secret: {SecretName}", secretName);
                    }
                }

                _cachedSecrets = secrets;
                return secrets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secrets from Key Vault");
            }
        }

        // Fallback to configuration values
        _logger.LogDebug("Using fallback configuration values");
        secrets["ApplicationId"] = _configuration["CIPP_AUTH:ApplicationID"] ?? "";
        secrets["ApplicationSecret"] = _configuration["CIPP_AUTH:ApplicationSecret"] ?? "";
        secrets["RefreshToken"] = _configuration["CIPP_AUTH:RefreshToken"] ?? "";
        secrets["tenantid"] = _configuration["Azure:TenantId"] ?? "";

        return secrets;
    }

    public async Task<string?> GetClientPrincipalAsync()
    {
        var authMode = _configuration["AUTH_MODE"] ?? "development";
        
        switch (authMode.ToLower())
        {
            case "keyvault":
                return await GetKeyVaultAuthenticatedPrincipalAsync();
            
            case "browser":
                return GetBrowserAuthenticatedPrincipal();
            
            case "development":
            default:
                return GetDevelopmentPrincipal();
        }
    }

    private async Task<string?> GetKeyVaultAuthenticatedPrincipalAsync()
    {
        try
        {
            var secrets = await GetCippSecretsAsync();
            
            if (!secrets.ContainsKey("ApplicationId") || string.IsNullOrEmpty(secrets["ApplicationId"]))
            {
                _logger.LogWarning("ApplicationId not found in secrets, falling back to development mode");
                return GetDevelopmentPrincipal();
            }

            var userEmail = _configuration["CIPP_USER_EMAIL"] ?? "davidb@roanoketechhub.com";
            var tenantId = secrets["tenantid"] ?? _configuration["Azure:TenantId"] ?? "";

            var authenticatedPrincipal = new
            {
                userId = userEmail,
                userDetails = userEmail,
                userRoles = new[] { "authenticated", "admin" },
                identityProvider = "aad",
                claims = new[]
                {
                    new { typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", val = userEmail },
                    new { typ = "http://schemas.microsoft.com/identity/claims/tenantid", val = tenantId },
                    new { typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", val = userEmail },
                    new { typ = "appid", val = secrets["ApplicationId"] }
                },
                // Include the actual CIPP secrets for API calls
                cippSecrets = new
                {
                    applicationId = secrets["ApplicationId"],
                    refreshToken = secrets["RefreshToken"],
                    tenantId = secrets["tenantid"]
                }
            };

            var json = JsonSerializer.Serialize(authenticatedPrincipal);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            
            _logger.LogDebug("Generated authenticated client principal from Key Vault");
            return base64;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Key Vault authenticated principal");
            return GetDevelopmentPrincipal();
        }
    }

    private string? GetBrowserAuthenticatedPrincipal()
    {
        var authCookie = _configuration["BROWSER_AUTH_COOKIE"];
        
        if (string.IsNullOrEmpty(authCookie))
        {
            _logger.LogWarning("Browser auth cookie not configured, falling back to development mode");
            return GetDevelopmentPrincipal();
        }

        // For browser authentication, we'll pass through the cookie
        // The actual authentication will be handled by the HTTP client
        return GetDevelopmentPrincipal(); // Return dev principal, HTTP client handles cookie
    }

    private string? GetDevelopmentPrincipal()
    {
        var userEmail = _configuration["Azure:UserEmail"] ?? "davidb@roanoketechhub.com";
        var userId = _configuration["Azure:UserId"] ?? userEmail;
        var tenantId = _configuration["Azure:TenantId"] ?? "3278c17c-6680-41c6-86bf-296981c37b94";
        
        var mockPrincipal = new
        {
            userId = userId,
            userDetails = userEmail,
            userRoles = new[] { "authenticated", "admin" },
            identityProvider = "aad",
            claims = new[]
            {
                new { typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", val = userEmail },
                new { typ = "http://schemas.microsoft.com/identity/claims/tenantid", val = tenantId },
                new { typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", val = userId }
            }
        };

        var json = JsonSerializer.Serialize(mockPrincipal);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        
        _logger.LogDebug("Generated development client principal");
        return base64;
    }

    public string? GetBrowserAuthCookie()
    {
        return _configuration["BROWSER_AUTH_COOKIE"];
    }

    public bool IsAuthenticated()
    {
        var authMode = _configuration["AUTH_MODE"] ?? "development";
        return authMode.ToLower() switch
        {
            "keyvault" => _keyVaultClient != null,
            "browser" => !string.IsNullOrEmpty(_configuration["BROWSER_AUTH_COOKIE"]),
            "development" => true,
            _ => true
        };
    }

    public string[] GetUserRoles()
    {
        return new[] { "authenticated", "admin" };
    }
}
