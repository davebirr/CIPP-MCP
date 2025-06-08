using CippMcp.Tools;
using System.Text.Json.Serialization;
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
// HandleStatelessMcp(app);

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
        // Use the MonkeyAction enum to generate the list of valid actions
        var actionEnumValues = System.Enum.GetNames(typeof(CippMcp.Tools.MonkeyAction)).Select(a => a.ToLower()).ToArray();
        var inputSchemaObj = new
        {
            type = "object",
            properties = new
            {
                action = new
                {
                    type = "string",
                    description = $"Action to perform. One of: {string.Join(", ", actionEnumValues)}",
                    @enum = actionEnumValues
                },
                name = new
                {
                    type = "string",
                    description = "(Optional) Name of the monkey for actions that require it"
                }
            },
            required = new[] { "action" }
        };
        var inputSchemaJson = System.Text.Json.JsonSerializer.Serialize(inputSchemaObj);
        return new ListToolsResult()
        {
            Tools =
            [
                new Tool()
                {
                    Name = "monkey",
                    Description = "Returns a random monkey fact, all monkeys, or a monkey by name.",
                    InputSchema = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(inputSchemaJson, McpJsonUtilities.DefaultOptions),
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
            var action = actionElement.ToString();
            if (string.IsNullOrWhiteSpace(action))
                throw new McpException("Action cannot be empty", McpErrorCode.InvalidParams);

            var tool = new CippMcp.Tools.MonkeyTools();
            var toolType = tool.GetType();
            var method = toolType.GetMethods()
                .Where(m => m.IsPublic && !m.IsStatic)
                .FirstOrDefault(m =>
                    string.Equals(m.Name, action, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(m.Name, "Get" + action, StringComparison.OrdinalIgnoreCase)
                );
            if (method == null)
                throw new McpException($"Unknown monkey action: '{action}'", McpErrorCode.InvalidParams);

            var parameters = method.GetParameters();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (request.Params.Arguments.TryGetValue(param.Name, out var val))
                {
                    if (val is System.Text.Json.JsonElement je)
                    {
                        if (je.ValueKind == System.Text.Json.JsonValueKind.String)
                            args[i] = je.GetString();
                        else if (je.ValueKind == System.Text.Json.JsonValueKind.Number && param.ParameterType == typeof(int) && je.TryGetInt32(out var intVal))
                            args[i] = intVal;
                        else
                            args[i] = je.ToString();
                    }
                    else
                    {
                        args[i] = val;
                    }
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else
                {
                    throw new McpException($"Missing required argument '{param.Name}' for '{action}' action", McpErrorCode.InvalidParams);
                }
            }

            object? resultObj = method.Invoke(tool, args);
            string? result;
            if (resultObj is Task<string> taskStr)
                result = await taskStr;
            else if (resultObj is string str)
                result = str;
            else
                result = resultObj?.ToString();

            var contentType = (string.Equals(action, "getrandomfact", StringComparison.OrdinalIgnoreCase)) ? "text" : "application/json";
            return new CallToolResponse()
            {
                Content = [new Content() { Text = result, Type = contentType }]
            };
        }
        else
        {
            throw new McpException($"Unknown tool: '{request.Params.Name}'", McpErrorCode.InvalidParams);
        }
    };
}
