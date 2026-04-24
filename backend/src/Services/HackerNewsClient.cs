using System.Text.Json.Serialization;

namespace hackernews_api.Services;

public class HackerNewsStory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("descendants")]
    public int Descendants { get; set; }

    [JsonPropertyName("by")]
    public string? By { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public interface IHackerNewsClient
{
    Task<List<int>> GetLatestStoryIdsAsync();
    Task<HackerNewsStory?> GetStoryAsync(int id);
}

public class HackerNewsClient : IHackerNewsClient
{
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerNewsClient> _logger;

    public HackerNewsClient(HttpClient httpClient, ILogger<HackerNewsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<int>> GetLatestStoryIdsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}/newstories.json?print=pretty");
            return response ?? new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest story IDs from Hacker News API");
            return new List<int>();
        }
    }

    public async Task<HackerNewsStory?> GetStoryAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<HackerNewsStory?>($"{BaseUrl}/item/{id}.json?print=pretty");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching story {StoryId} from Hacker News API", id);
            return null;
        }
    }
}
