using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.Extensions.AI;

namespace CippMcp.Tools
{
    [McpServerToolType]
    public class WeatherServiceTools
    {
        private readonly IWeatherService _weatherService;

        public WeatherServiceTools(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [McpServerTool, Description("Get the weather forecast for a location.")]
        public string GetWeather([Description("The location to get the weather for")] string location)
        {
            return _weatherService.GetForecast(location);
        }

        [McpServerTool, Description("Process large weather data with progress reporting.")]
        public string ProcessLargeData(
            string data,
            IProgress<ProgressNotificationValue> progress)
        {
            // Simulate processing data in chunks
            for (int i = 0; i < 10; i++)
            {
                // Simulate work (replace with real logic as needed)
                System.Threading.Thread.Sleep(100); // Simulate delay

                // Report progress
                progress.Report(new ProgressNotificationValue
                {
                    Progress = i * 10,
                    Total = 100,
                    Message = $"Processing chunk {i + 1}/10"
                });
            }
            return "Processing complete";
        }

        [McpServerTool, Description("A tool that throws an error for testing error handling.")]
        public string ThrowingTool()
        {
            try
            {
                throw new InvalidOperationException("Something went wrong");
            }
            catch (Exception ex)
            {
                // Log error details to terminal during development
                Console.Error.WriteLine($"[WeatherServiceTools.ThrowingTool] Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        [McpServerTool, Description("A long-running operation that supports cancellation.")]
        public async Task<string> LongRunningOperation(
            string input,
            CancellationToken cancellationToken)
        {
            // Simulate long-running work with cancellation support
            await Task.Delay(10000, cancellationToken);
            return "Completed";
        }

        public static McpServerTool CustomSchemaTool = McpServerTool.Create(
            (int num, string str) => $"Result: {num}, {str}",
            new McpServerToolCreateOptions
            {
                SchemaCreateOptions = new AIJsonSchemaCreateOptions
                {
                    TransformSchemaNode = (context, node) =>
                    {
                        // Add custom schema modifications
                        node["additionalProperties"] = false;
                        return node;
                    }
                }
            }
        );
    }
}
