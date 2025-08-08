using CippMcp.Services;
using CippMcp.Tools;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using DotNetEnv;

// Load environment variables from .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"✅ Loaded environment from: {envPath}");
}
else
{
    Console.WriteLine($"⚠️ No .env file found at: {envPath}");
}

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add CIPP API services
builder.Services.AddScoped<CippApiService>();
builder.Services.AddScoped<AuthenticationService>();

// Add MCP server services with HTTP transport and CIPP tools
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<TenantTools>()
    .WithTools<UserTools>() 
    .WithTools<DeviceTools>();

// Add CORS for HTTP transport support in browsers
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable CORS
app.UseCors();

// Map MCP endpoints
app.MapMcp("/mcp");

// Add status endpoint
app.MapGet("/status", () => "CIPP-MCP Server - Ready for use with HTTP transport");

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    authMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "development"
}));

// Add OAuth2 endpoints for user-delegated authentication
app.MapGet("/oauth2/start", (HttpContext context) =>
{
    var authMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "development";
    if (authMode.ToLower() != "oauth2")
    {
        return Results.BadRequest(new { error = "OAuth2 mode not enabled", authMode });
    }

    var tenantId = Environment.GetEnvironmentVariable("OAUTH2_TENANT_ID");
    var clientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID");
    var redirectUri = Environment.GetEnvironmentVariable("OAUTH2_REDIRECT_URI");
    
    if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
    {
        return Results.BadRequest(new { error = "OAuth2 configuration incomplete" });
    }

    var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
                  $"?client_id={clientId}" +
                  $"&response_type=code" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&scope=openid%20profile%20email%20offline_access" +
                  $"&state={Guid.NewGuid()}";

    return Results.Ok(new { authUrl, redirectUri, message = "Redirect user to authUrl for OAuth2 authentication" });
});

app.MapGet("/oauth2/debug", () =>
{
    var authMode = Environment.GetEnvironmentVariable("AUTH_MODE");
    var tenantId = Environment.GetEnvironmentVariable("OAUTH2_TENANT_ID");
    var clientId = Environment.GetEnvironmentVariable("OAUTH2_CLIENT_ID");
    var redirectUri = Environment.GetEnvironmentVariable("OAUTH2_REDIRECT_URI");
    
    return Results.Ok(new
    {
        authMode,
        tenantId,
        clientId = string.IsNullOrEmpty(clientId) ? "Not configured" : "Configured",
        redirectUri,
        timestamp = DateTime.UtcNow
    });
});

app.Run();
