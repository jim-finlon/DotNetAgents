using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNetAgents.Mcp;
using DotNetAgents.Mcp.Abstractions;
using DotNetAgents.Mcp.Configuration;
using DotNetAgents.Mcp.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace DotNetAgents.Mcp.Tests;

public class McpClientTests
{
    private readonly Mock<ILogger<McpClient>> _loggerMock;
    private readonly McpServiceConfig _config;

    public McpClientTests()
    {
        _loggerMock = new Mock<ILogger<McpClient>>();
        _config = new McpServiceConfig
        {
            ServiceName = "test-service",
            BaseUrl = "https://api.example.com",
            AuthType = "none",
            TimeoutSeconds = 30
        };
    }

    [Fact]
    public void ServiceName_ReturnsConfigServiceName()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new McpClient(httpClient, _config, _loggerMock.Object);

        // Act
        var serviceName = client.ServiceName;

        // Assert
        serviceName.Should().Be("test-service");
    }

    [Fact]
    public async Task ListToolsAsync_WithValidResponse_ReturnsTools()
    {
        // Arrange
        var expectedTools = new McpListToolsResponse
        {
            Tools = new List<McpToolDefinition>
            {
                new()
                {
                    Name = "test_tool",
                    Description = "Test tool",
                    InputSchema = new McpToolInputSchema()
                }
            },
            TotalCount = 1
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedTools)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };

        var client = new McpClient(httpClient, _config, _loggerMock.Object);

        // Act
        var result = await client.ListToolsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Tools.Should().HaveCount(1);
        result.Tools[0].Name.Should().Be("test_tool");
        result.Tools[0].ServiceName.Should().Be("test-service");
    }

    [Fact]
    public async Task CallToolAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new McpToolCallRequest
        {
            Tool = "test_tool",
            Arguments = new Dictionary<string, object> { ["param"] = "value" }
        };

        var expectedResponse = new McpToolCallResponse
        {
            Success = true,
            Result = new { output = "result" },
            DurationMs = 100
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };

        var client = new McpClient(httpClient, _config, _loggerMock.Object);

        // Act
        var result = await client.CallToolAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHealthAsync_WithHealthyService_ReturnsHealthy()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };

        var client = new McpClient(httpClient, _config, _loggerMock.Object);

        // Act
        var result = await client.GetHealthAsync();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("healthy");
        result.ServiceName.Should().Be("test-service");
    }
}
