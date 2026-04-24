using Xunit;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using hackernews_api.Services;
using hackernews_api.Models;

namespace hackernews_api.Tests;

public class StoryServiceTests
{
    private readonly Mock<IHackerNewsClient> _mockHackerNewsClient;
    private readonly Mock<ILogger<StoryService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;

    public StoryServiceTests()
    {
        _mockHackerNewsClient = new Mock<IHackerNewsClient>();
        _mockLogger = new Mock<ILogger<StoryService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task GetStoriesAsync_WithValidStories_ReturnsPaginatedResults()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 50).ToList();
        var stories = GenerateTestStories(50);

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        foreach (var story in stories)
        {
            _mockHackerNewsClient
                .Setup(x => x.GetStoryAsync(story.Id))
                .ReturnsAsync(new HackerNewsStory
                {
                    Id = story.Id,
                    Title = story.Title,
                    Url = story.Url,
                    Score = story.Score,
                    Descendants = story.CommentCount,
                    By = story.By,
                    Time = new DateTimeOffset(story.PublishedAt).ToUnixTimeSeconds(),
                    Type = "story"
                });
        }

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act
        var result = await service.GetStoriesAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Stories.Count);
        Assert.Equal(50, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(5, result.TotalPages);
    }

    [Fact]
    public async Task GetStoriesAsync_WithSearch_FiltersStories()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 3).ToList();
        var stories = new List<HackerNewsStory>
        {
            new() { Id = 1, Title = "C# Best Practices", Url = "https://example.com", Score = 100, Descendants = 10, By = "user1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Type = "story" },
            new() { Id = 2, Title = ".NET Core Tutorial", Url = "https://example.com", Score = 50, Descendants = 5, By = "user2", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Type = "story" },
            new() { Id = 3, Title = "Python Guide", Url = "https://example.com", Score = 75, Descendants = 8, By = "user3", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Type = "story" }
        };

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        foreach (var story in stories)
        {
            _mockHackerNewsClient
                .Setup(x => x.GetStoryAsync(story.Id))
                .ReturnsAsync(story);
        }

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act
        var result = await service.GetStoriesAsync(page: 1, pageSize: 30, search: ".NET");

        // Assert
        Assert.Single(result.Stories);
        Assert.Equal("Tutorial", result.Stories.First().Title.Split(' ').Last());
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetStoriesAsync_WithoutUrl_ReturnsStoryWithNullUrl()
    {
        // Arrange - Test edge case where story has no URL
        var storyIds = new List<int> { 1 };
        var story = new HackerNewsStory
        {
            Id = 1,
            Title = "Ask HN: Story without URL",
            Url = null, // Edge case: missing URL
            Score = 100,
            Descendants = 5,
            By = "user1",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Type = "story"
        };

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        _mockHackerNewsClient
            .Setup(x => x.GetStoryAsync(1))
            .ReturnsAsync(story);

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act
        var result = await service.GetStoriesAsync();

        // Assert
        Assert.Single(result.Stories);
        Assert.Null(result.Stories.First().Url);
        Assert.NotNull(result.Stories.First().Title);
    }

    [Fact]
    public async Task GetStoriesAsync_SkipsStoriesWithoutTitle_ValidatesData()
    {
        // Arrange - Test edge case where story has no title
        var storyIds = new List<int> { 1, 2 };
        var validStory = new HackerNewsStory
        {
            Id = 1,
            Title = "Valid Story",
            Url = "https://example.com",
            Score = 100,
            Descendants = 5,
            By = "user1",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Type = "story"
        };

        var invalidStory = new HackerNewsStory
        {
            Id = 2,
            Title = null, // Edge case: no title
            Url = "https://example.com",
            Score = 50,
            Descendants = 2,
            By = "user2",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Type = "story"
        };

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        _mockHackerNewsClient
            .Setup(x => x.GetStoryAsync(1))
            .ReturnsAsync(validStory);

        _mockHackerNewsClient
            .Setup(x => x.GetStoryAsync(2))
            .ReturnsAsync(invalidStory);

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act
        var result = await service.GetStoriesAsync();

        // Assert
        Assert.Single(result.Stories); // Only the valid story should be returned
        Assert.Equal("Valid Story", result.Stories.First().Title);
    }

    [Fact]
    public async Task GetStoriesAsync_WithInvalidPageNumber_DefaultsToPageOne()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 50).ToList();
        var stories = GenerateTestStories(50);

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        foreach (var story in stories)
        {
            _mockHackerNewsClient
                .Setup(x => x.GetStoryAsync(story.Id))
                .ReturnsAsync(new HackerNewsStory
                {
                    Id = story.Id,
                    Title = story.Title,
                    Url = story.Url,
                    Score = story.Score,
                    Descendants = story.CommentCount,
                    By = story.By,
                    Time = new DateTimeOffset(story.PublishedAt).ToUnixTimeSeconds(),
                    Type = "story"
                });
        }

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act - Request page 0 (invalid)
        var result = await service.GetStoriesAsync(page: 0, pageSize: 10);

        // Assert
        Assert.Equal(1, result.Page); // Should default to page 1
    }

    [Fact]
    public async Task GetStoriesAsync_SortsByScore_WhenSortByScoreRequested()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3 };
        var stories = new List<HackerNewsStory>
        {
            new() { Id = 1, Title = "Story 1", Url = "https://example.com", Score = 100, Descendants = 5, By = "user1", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Type = "story" },
            new() { Id = 2, Title = "Story 2", Url = "https://example.com", Score = 200, Descendants = 10, By = "user2", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Type = "story" },
            new() { Id = 3, Title = "Story 3", Url = "https://example.com", Score = 50, Descendants = 2, By = "user3", Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Type = "story" }
        };

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        foreach (var story in stories)
        {
            _mockHackerNewsClient
                .Setup(x => x.GetStoryAsync(story.Id))
                .ReturnsAsync(story);
        }

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act
        var result = await service.GetStoriesAsync(sortBy: "score");

        // Assert
        Assert.Equal(3, result.Stories.Count);
        Assert.Equal(200, result.Stories[0].Score); // Highest score first
        Assert.Equal(100, result.Stories[1].Score);
        Assert.Equal(50, result.Stories[2].Score);
    }

    [Fact]
    public async Task GetStoriesAsync_Caches_StoriesToReduceApiCalls()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 10).ToList();
        var stories = GenerateTestStories(10);

        _mockHackerNewsClient
            .Setup(x => x.GetLatestStoryIdsAsync())
            .ReturnsAsync(storyIds);

        foreach (var story in stories)
        {
            _mockHackerNewsClient
                .Setup(x => x.GetStoryAsync(story.Id))
                .ReturnsAsync(new HackerNewsStory
                {
                    Id = story.Id,
                    Title = story.Title,
                    Url = story.Url,
                    Score = story.Score,
                    Descendants = story.CommentCount,
                    By = story.By,
                    Time = new DateTimeOffset(story.PublishedAt).ToUnixTimeSeconds(),
                    Type = "story"
                });
        }

        var service = new StoryService(_mockHackerNewsClient.Object, _memoryCache, _mockLogger.Object);

        // Act - First call
        var result1 = await service.GetStoriesAsync();
        var apiCallsAfterFirst = _mockHackerNewsClient.Invocations.Count;

        // Second call should use cache
        var result2 = await service.GetStoriesAsync();
        var apiCallsAfterSecond = _mockHackerNewsClient.Invocations.Count;

        // Assert
        Assert.Equal(result1.Stories.Count, result2.Stories.Count);
        Assert.Equal(apiCallsAfterFirst, apiCallsAfterSecond); // No new API calls
    }

    private List<Story> GenerateTestStories(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Story
            {
                Id = i,
                Title = $"Story {i}",
                Url = $"https://example.com/{i}",
                Score = 100 - i,
                CommentCount = 10 + i,
                By = $"user{i}",
                PublishedAt = DateTime.UtcNow.AddHours(-i)
            })
            .ToList();
    }
}
