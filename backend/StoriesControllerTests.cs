using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using hackernews_api.Controllers;
using hackernews_api.Models;
using hackernews_api.Services;

namespace hackernews_api.Tests;

public class StoriesControllerTests
{
    private readonly Mock<IStoryService> _mockStoryService;
    private readonly Mock<ILogger<StoriesController>> _mockLogger;

    public StoriesControllerTests()
    {
        _mockStoryService = new Mock<IStoryService>();
        _mockLogger = new Mock<ILogger<StoriesController>>();
    }

    [Fact]
    public async Task GetStories_WithDefaultParameters_ReturnsOkWithStories()
    {
        // Arrange
        var stories = new List<Story>
        {
            new() { Id = 1, Title = "Story 1", Url = "https://example.com", Score = 100, CommentCount = 10, By = "user1", PublishedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Story 2", Url = "https://example.com", Score = 80, CommentCount = 5, By = "user2", PublishedAt = DateTime.UtcNow }
        };

        var response = new StoryResponse
        {
            Stories = stories,
            TotalCount = 2,
            Page = 1,
            PageSize = 30,
            TotalPages = 1
        };

        _mockStoryService
            .Setup(x => x.GetStoriesAsync(1, 30, null, "date"))
            .ReturnsAsync(response);

        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetStories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<StoryResponse>(okResult.Value);
        Assert.Equal(2, returnedResponse.Stories.Count);
        Assert.Equal(1, returnedResponse.TotalPages);
    }

    [Fact]
    public async Task GetStories_WithSearch_ReturnsFilteredStories()
    {
        // Arrange
        var stories = new List<Story>
        {
            new() { Id = 1, Title = "C# Best Practices", Url = "https://example.com", Score = 100, CommentCount = 10, By = "user1", PublishedAt = DateTime.UtcNow }
        };

        var response = new StoryResponse
        {
            Stories = stories,
            TotalCount = 1,
            Page = 1,
            PageSize = 30,
            TotalPages = 1
        };

        _mockStoryService
            .Setup(x => x.GetStoriesAsync(1, 30, "C#", "date"))
            .ReturnsAsync(response);

        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetStories(page: 1, pageSize: 30, search: "C#");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<StoryResponse>(okResult.Value);
        Assert.Single(returnedResponse.Stories);
    }

    [Fact]
    public async Task GetStories_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var storiesPage2 = new List<Story>
        {
            new() { Id = 31, Title = "Story 31", Url = "https://example.com", Score = 10, CommentCount = 1, By = "user31", PublishedAt = DateTime.UtcNow }
        };

        var response = new StoryResponse
        {
            Stories = storiesPage2,
            TotalCount = 40,
            Page = 2,
            PageSize = 30,
            TotalPages = 2
        };

        _mockStoryService
            .Setup(x => x.GetStoriesAsync(2, 30, null, "date"))
            .ReturnsAsync(response);

        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetStories(page: 2, pageSize: 30);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<StoryResponse>(okResult.Value);
        Assert.Equal(2, returnedResponse.Page);
        Assert.Equal(2, returnedResponse.TotalPages);
    }

    [Fact]
    public async Task GetStories_WithSortByScore_ReturnsStoriesSortedByScore()
    {
        // Arrange
        var stories = new List<Story>
        {
            new() { Id = 2, Title = "Story 2", Url = "https://example.com", Score = 200, CommentCount = 20, By = "user2", PublishedAt = DateTime.UtcNow },
            new() { Id = 1, Title = "Story 1", Url = "https://example.com", Score = 100, CommentCount = 10, By = "user1", PublishedAt = DateTime.UtcNow }
        };

        var response = new StoryResponse
        {
            Stories = stories,
            TotalCount = 2,
            Page = 1,
            PageSize = 30,
            TotalPages = 1
        };

        _mockStoryService
            .Setup(x => x.GetStoriesAsync(1, 30, null, "score"))
            .ReturnsAsync(response);

        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetStories(sortBy: "score");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<StoryResponse>(okResult.Value);
        Assert.Equal(200, returnedResponse.Stories.First().Score);
        Assert.Equal(100, returnedResponse.Stories.Last().Score);
    }

    [Fact]
    public async Task GetStories_WhenServiceThrowsException_Returns500Error()
    {
        // Arrange
        _mockStoryService
            .Setup(x => x.GetStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service error"));

        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetStories();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public void GetStory_ReturnsNotFound()
    {
        // Arrange
        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = controller.GetStory(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetStories_WithNoResults_ReturnsEmptyList()
    {
        // Arrange
        var response = new StoryResponse
        {
            Stories = new List<Story>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 30,
            TotalPages = 0
        };

        _mockStoryService
            .Setup(x => x.GetStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        var controller = new StoriesController(_mockStoryService.Object, _mockLogger.Object);

        // Act
        var result = await controller.GetStories(search: "nonexistent");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<StoryResponse>(okResult.Value);
        Assert.Empty(returnedResponse.Stories);
        Assert.Equal(0, returnedResponse.TotalCount);
    }
}
