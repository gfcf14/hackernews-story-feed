using hackernews_api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace hackernews_api.Services;

public interface IStoryService
{
    Task<StoryResponse> GetStoriesAsync(int page = 1, int pageSize = 30, string? search = null, string sortBy = "date");
}

public class StoryService : IStoryService
{
    private readonly IHackerNewsClient _hackerNewsClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<StoryService> _logger;
    private const string CACHE_KEY = "latest_stories";
    private const int CACHE_DURATION_MINUTES = 5;
    private const int TOP_STORIES_TO_FETCH = 200;

    public StoryService(IHackerNewsClient hackerNewsClient, IMemoryCache memoryCache, ILogger<StoryService> logger)
    {
        _hackerNewsClient = hackerNewsClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<StoryResponse> GetStoriesAsync(int page = 1, int pageSize = 30, string? search = null, string sortBy = "date")
    {
        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 30;

            // Try to get from cache first
            if (!_memoryCache.TryGetValue(CACHE_KEY, out List<Story>? cachedStories))
            {
                cachedStories = await FetchAndCacheStoriesAsync();
            }

            if (cachedStories == null || cachedStories.Count == 0)
            {
                return new StoryResponse
                {
                    Stories = new List<Story>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = 0
                };
            }

            // Apply search filter
            var filtered = ApplySearchFilter(cachedStories, search);

            // Apply sorting
            filtered = ApplySorting(filtered, sortBy);

            // Apply pagination
            var totalCount = filtered.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;
            var paginated = filtered.Skip(skip).Take(pageSize).ToList();

            return new StoryResponse
            {
                Stories = paginated,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetStoriesAsync");
            return new StoryResponse
            {
                Stories = new List<Story>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize,
                TotalPages = 0
            };
        }
    }

    private async Task<List<Story>> FetchAndCacheStoriesAsync()
    {
        var storyIds = await _hackerNewsClient.GetLatestStoryIdsAsync();
        var stories = new List<Story>();

        // Fetch top N stories (limit to avoid timeout)
        var topIds = storyIds.Take(TOP_STORIES_TO_FETCH).ToList();

        var tasks = topIds.Select(id => _hackerNewsClient.GetStoryAsync(id)).ToList();
        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result != null && IsValidStory(result))
            {
                var story = MapToStory(result);
                stories.Add(story);
            }
        }

        // Sort by date descending (newest first) by default
        stories = stories.OrderByDescending(s => s.PublishedAt).ToList();

        // Cache the stories
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
        _memoryCache.Set(CACHE_KEY, stories, cacheOptions);

        _logger.LogInformation("Cached {Count} stories", stories.Count);
        return stories;
    }

    private List<Story> ApplySearchFilter(List<Story> stories, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return stories;

        var lowerSearch = search.ToLower();
        return stories.Where(s => 
            s.Title != null && s.Title.ToLower().Contains(lowerSearch)
        ).ToList();
    }

    private List<Story> ApplySorting(List<Story> stories, string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "score" => stories.OrderByDescending(s => s.Score).ToList(),
            "date" => stories.OrderByDescending(s => s.PublishedAt).ToList(),
            _ => stories.OrderByDescending(s => s.PublishedAt).ToList()
        };
    }

    private bool IsValidStory(HackerNewsStory story)
    {
        // Skip stories without titles
        if (string.IsNullOrWhiteSpace(story.Title))
            return false;

        // Skip if not actually a story type
        if (!string.IsNullOrEmpty(story.Type) && story.Type != "story")
            return false;

        return true;
    }

    private Story MapToStory(HackerNewsStory source)
    {
        var unixTimestamp = source.Time;
        var dateTime = UnixTimeStampToDateTime(unixTimestamp);

        return new Story
        {
            Id = source.Id,
            Title = source.Title,
            Url = source.Url,
            Score = source.Score,
            CommentCount = source.Descendants,
            By = source.By,
            PublishedAt = dateTime
        };
    }

    private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }
}
