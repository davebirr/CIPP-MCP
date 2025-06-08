using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CippMcp.Tests
{
    public class CippMcpServerTests
    {
        [Fact]
        public async Task Server_Responds_To_MonkeyTools_GetMonkeys()
        {
            // Arrange
            using var client = new HttpClient();
            var url = "http://localhost:5000/api/mcp";
            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "MonkeyTools.GetMonkeys",
                @params = new { },
                id = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            content.Headers.Add("Accept", "application/json, text/event-stream");

            // Act
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Server did not respond successfully: {responseString}");
            Assert.Contains("monkey", responseString, StringComparison.OrdinalIgnoreCase);
        }
    }
}