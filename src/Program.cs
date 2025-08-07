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
    version = "1.0.0"
}));

app.Run();
