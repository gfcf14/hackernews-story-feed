using Microsoft.AspNetCore.Mvc;
using hackernews_api.Models;

namespace hackernews_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private static readonly List<Story> MockStories = new()
    {
        new Story 
        { 
            Id = 1, 
            Title = "Show HN: Minimal Full-Stack Boilerplate", 
            Url = "https://github.com", 
            Score = 342, 
            CommentCount = 48, 
            PublishedAt = DateTime.UtcNow.AddHours(-2) 
        },
        new Story 
        { 
            Id = 2, 
            Title = "The Art of Minimal Dependencies", 
            Url = "https://github.com", 
            Score = 256, 
            CommentCount = 32, 
            PublishedAt = DateTime.UtcNow.AddHours(-4) 
        },
        new Story 
        { 
            Id = 3, 
            Title = ".NET 8 Released with New Features", 
            Url = "https://github.com", 
            Score = 512, 
            CommentCount = 128, 
            PublishedAt = DateTime.UtcNow.AddHours(-6) 
        }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Story>> GetStories()
    {
        return Ok(MockStories);
    }

    [HttpGet("{id}")]
    public ActionResult<Story> GetStory(int id)
    {
        var story = MockStories.FirstOrDefault(s => s.Id == id);
        if (story == null)
            return NotFound();
        
        return Ok(story);
    }

    [HttpPost]
    public ActionResult<Story> CreateStory(Story story)
    {
        story.Id = MockStories.Max(s => s.Id) + 1;
        story.PublishedAt = DateTime.UtcNow;
        MockStories.Add(story);
        
        return CreatedAtAction(nameof(GetStory), new { id = story.Id }, story);
    }
}
