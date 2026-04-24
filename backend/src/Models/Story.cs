namespace hackernews_api.Models;

public class Story
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public int Score { get; set; }
    public int CommentCount { get; set; }
    public DateTime PublishedAt { get; set; }
}
