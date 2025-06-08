using CippMcp.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using ModelContextProtocol.Server;
using System.Diagnostics;

Console.WriteLine("Starting server...");

var builder = WebApplication.CreateEmptyBuilder(new()
{
    Args = args,
});

// Kestrel transport setup
IConnectionListenerFactory? kestrelTransport = null; // Set this if you want to inject a custom transport
ILoggerProvider? loggerProvider = null; // Set this if you want to inject a custom logger provider

if (kestrelTransport is null)
{
    int port = args.Length > 0 && uint.TryParse(args[0], out var parsedPort) ? (int)parsedPort : 3001;
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(port);
    });
}
else
{
    builder.Services.AddSingleton(kestrelTransport);
}

builder.WebHost.UseKestrelCore();
builder.Services.AddLogging();
builder.Services.AddRoutingCore();
builder.Logging.AddConsole();
ConfigureSerilog(builder.Logging);
if (loggerProvider is not null)
{
    builder.Logging.AddProvider(loggerProvider);
}

builder.Services.AddMcpServer(ConfigureOptions)
    .WithHttpTransport();

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(_ => { });



// Add a test endpoint to verify server is running
app.MapGet("/test", () =>
{
    Console.WriteLine("/test endpoint hit");
    return "Hello, world!";
});

Console.WriteLine("[TRACE] Registering MCP endpoint...");

app.MapMcp("/mcp");

// Handle the /stateless endpoint if no other endpoints have been matched by the call to UseRouting above.
HandleStatelessMcp(app);

await app.RunAsync();

// Serilog configuration helper for top-level statements
static void ConfigureSerilog(ILoggingBuilder loggingBuilder)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "TestServer_.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    loggingBuilder.AddSerilog();
}

// MCP options configuration helper for top-level statements
static void ConfigureOptions(ModelContextProtocol.Server.McpServerOptions options)
{
    options.Capabilities = new ServerCapabilities()
    {
        Tools = new(),
        Resources = new(),
        Prompts = new(),
    };
    options.ServerInstructions = "This is a test server with only stub functionality";

    Console.WriteLine("Registering handlers.");

    // Helper method for request sampling params
    static CreateMessageRequestParams CreateRequestSamplingParams(string context, string uri, int maxTokens = 100)
    {
        return new CreateMessageRequestParams()
        {
            Messages = [new SamplingMessage()
            {
                Role = Role.User,
                Content = new Content()
                {
                    Type = "text",
                    Text = $"Resource {uri} context: {context}"
                }
            }],
            SystemPrompt = "You are a helpful test server.",
            MaxTokens = maxTokens,
            Temperature = 0.7f,
            IncludeContext = ContextInclusion.ThisServer
        };
    }

    // Register dynamic tool handlers for echo, sampleLLM, and monkey
    options.Capabilities.Tools.ListToolsHandler = async (request, cancellationToken) =>
    {
        return new ListToolsResult()
        {
            Tools =
            [
                new Tool()
                {
                    Name = "monkey",
                    Description = "Returns a random monkey fact, all monkeys, or a monkey by name.",
                    InputSchema = JsonSerializer.Deserialize<JsonElement>("""
                        {
                          "type": "object",
                          "properties": {
                            "action": {
                              "type": "string",
                              "description": "Action to perform: 'fact', 'list', or 'get'"
                            },
                            "name": {
                              "type": "string",
                              "description": "(Optional) Name of the monkey for 'get' action"
                            }
                          },
                          "required": ["action"]
                        }
                    """, McpJsonUtilities.DefaultOptions),
                }
            ]
        };
    };

    options.Capabilities.Tools.CallToolHandler = async (request, cancellationToken) =>
    {
        if (request.Params is null)
            throw new McpException("Missing required parameter 'name'", McpErrorCode.InvalidParams);

        if (request.Params.Name == "monkey")
        {
            if (request.Params.Arguments is null || !request.Params.Arguments.TryGetValue("action", out var actionElement))
                throw new McpException("Missing required argument 'action'", McpErrorCode.InvalidParams);
            var action = actionElement.ToString()?.ToLowerInvariant();
            var monkeys = typeof(CippMcp.Tools.MonkeyTools)
                .GetField("monkeys", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.GetValue(null) as List<CippMcp.Tools.Monkey>;
            if (monkeys == null)
                throw new McpException("Monkey data unavailable", McpErrorCode.InternalError);
            if (action == "fact")
            {
                var facts = new[]
                {
                    "Monkeys use tools in the wild!",
                    "Some monkeys can count.",
                    "Capuchin monkeys are known for their intelligence.",
                    "Marmosets are among the smallest monkeys."
                };
                var random = new Random();
                var fact = facts[random.Next(facts.Length)];
                return new CallToolResponse()
                {
                    Content = [new Content() { Text = fact, Type = "text" }]
                };
            }
            else if (action == "list")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(monkeys, CippMcp.Tools.MonkeyContext.Default.ListMonkey);
                return new CallToolResponse()
                {
                    Content = [new Content() { Text = json, Type = "application/json" }]
                };
            }
            else if (action == "get")
            {
                if (!request.Params.Arguments.TryGetValue("name", out var nameElement))
                    throw new McpException("Missing required argument 'name' for 'get' action", McpErrorCode.InvalidParams);
                var name = nameElement.ToString();
                var monkey = monkeys.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (monkey == null)
                    throw new McpException($"Monkey '{name}' not found", McpErrorCode.InvalidParams);
                var json = System.Text.Json.JsonSerializer.Serialize(monkey, CippMcp.Tools.MonkeyContext.Default.Monkey);
                return new CallToolResponse()
                {
                    Content = [new Content() { Text = json, Type = "application/json" }]
                };
            }
            else
            {
                throw new McpException($"Unknown monkey action: '{action}'", McpErrorCode.InvalidParams);
            }
        }
        else
        {
            throw new McpException($"Unknown tool: '{request.Params.Name}'", McpErrorCode.InvalidParams);
        }
    };
}

static void HandleStatelessMcp(WebApplication app)
{
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging();
    serviceCollection.AddSingleton(app.Services.GetRequiredService<ILoggerFactory>());
    serviceCollection.AddSingleton(app.Services.GetRequiredService<DiagnosticListener>());
    serviceCollection.AddRoutingCore();

    serviceCollection.AddMcpServer(options =>
    {
        // You can reuse your ConfigureOptions helper if needed
        // ConfigureOptions(options);
        // Or inline your options setup here
        options.Capabilities = app.Services.GetRequiredService<McpServerOptions>().Capabilities;
        options.ServerInstructions = "This is a stateless MCP endpoint.";
    })
    .WithHttpTransport(options => options.Stateless = true);

    var appBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());
    appBuilder.UseRouting();
    appBuilder.UseEndpoints(innerEndpoints =>
    {
        innerEndpoints.MapMcp("/stateless");
    });

    // This will run the stateless pipeline when /stateless is hit
    app.Map("/stateless/{**catchall}", subApp =>
    {
        subApp.Run(appBuilder.Build());
    });
}
