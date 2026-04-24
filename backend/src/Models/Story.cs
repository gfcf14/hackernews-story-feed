namespace hackernews_api.Models;

public class Story
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public int Score { get; set; }
    public int CommentCount { get; set; }
    public string? By { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class StoryResponse
{
    public List<Story> Stories { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
