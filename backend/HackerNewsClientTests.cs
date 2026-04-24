using Xunit;
using Moq;
using Moq.Protected;
using hackernews_api.Services;

namespace hackernews_api.Tests;

public class HackerNewsClientTests
{
    private readonly Mock<ILogger<HackerNewsClient>> _mockLogger;

    public HackerNewsClientTests()
    {
        _mockLogger = new Mock<ILogger<HackerNewsClient>>();
    }

    [Fact]
    public async Task GetLatestStoryIdsAsync_WithValidResponse_ReturnsStoryIds()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = "[1, 2, 3, 4, 5]";
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var client = new HackerNewsClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.GetLatestStoryIdsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetLatestStoryIdsAsync_WithNetworkError_ReturnsEmptyList()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var client = new HackerNewsClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.GetLatestStoryIdsAsync();

        // Assert
        Assert.Empty(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStoryAsync_WithValidResponse_ReturnsStory()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = @"{
            ""id"": 123,
            ""title"": ""Test Story"",
            ""url"": ""https://example.com"",
            ""score"": 100,
            ""descendants"": 10,
            ""by"": ""testuser"",
            ""time"": 1234567890,
            ""type"": ""story""
        }";
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var client = new HackerNewsClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.GetStoryAsync(123);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("Test Story", result.Title);
        Assert.Equal("https://example.com", result.Url);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public async Task GetStoryAsync_WithNetworkError_ReturnsNull()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var client = new HackerNewsClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.GetStoryAsync(123);

        // Assert
        Assert.Null(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStoryAsync_WithTimeoutError_ReturnsNull()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException("Timeout"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var client = new HackerNewsClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.GetStoryAsync(123);

        // Assert
        Assert.Null(result);
    }
}
