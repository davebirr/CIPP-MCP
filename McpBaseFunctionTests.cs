using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Mcp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CIPP_MCP.Functions.Tests
{
    public class McpBaseFunctionTests
    {
        [Fact]
        public async Task RunAsync_StreamsLargeResponse()
        {
            // Arrange
            var loggerFactory = Mock.Of<ILoggerFactory>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var fakeStream = new MemoryStream(Encoding.UTF8.GetBytes(new string('A', 1024 * 1024)));
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(fakeStream)
            };
            var httpClient = new Mock<HttpClient>();
            httpClientFactory.Setup(f => f.CreateClient("CippApiClient")).Returns(new HttpClient(new FakeHttpMessageHandler(fakeResponse)));

            var function = new McpBaseFunction(loggerFactory, httpClientFactory.Object);
            var mcpRequest = new McpRequest
            {
                ToolName = "ListMailboxes",
                Parameters = new Dictionary<string, object> { { "TenantFilter", "test" } }
            };
            var mcpResponse = new TestMcpResponse();
            var context = new Mock<FunctionContext>().Object;

            // Act
            await function.RunAsync(mcpRequest, mcpResponse, context);

            // Assert
            Assert.Equal(200, mcpResponse.StatusCode);
            Assert.Equal(1024 * 1024, mcpResponse.Stream.Length);
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_response);
        }

        private class TestMcpResponse : McpResponse
        {
            public MemoryStream Stream { get; } = new MemoryStream();
            public override Task WriteStringAsync(string value)
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                Stream.Write(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            }
            public override Task WriteStreamAsync(Stream stream)
            {
                stream.CopyTo(Stream);
                return Task.CompletedTask;
            }
            public override int StatusCode { get; set; }
        }
    }
}
