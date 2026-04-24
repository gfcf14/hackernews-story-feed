using Microsoft.AspNetCore.Mvc;
using hackernews_api.Models;
using hackernews_api.Services;

namespace hackernews_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _storyService;
    private readonly ILogger<StoriesController> _logger;

    public StoriesController(IStoryService storyService, ILogger<StoriesController> logger)
    {
        _storyService = storyService;
        _logger = logger;
    }

    /// <summary>
    /// Get stories with optional search and pagination
    /// </summary>
    /// <param name="page">Page number (1-based), default 1</param>
    /// <param name="pageSize">Number of stories per page, default 30, max 100</param>
    /// <param name="search">Filter stories by title</param>
    /// <param name="sortBy">Sort by "score" or "date", default "date"</param>
    [HttpGet]
    public async Task<ActionResult<StoryResponse>> GetStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "date")
    {
        try
        {
            var result = await _storyService.GetStoriesAsync(page, pageSize, search, sortBy);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stories");
            return StatusCode(500, new { message = "Error retrieving stories", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a single story by ID (from HackerNews API)
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<Story> GetStory(int id)
    {
        // Individual story retrieval would require another service call
        // For now, users can get stories from the list endpoint
        return NotFound(new { message = "Use the list endpoint with pagination" });
    }
}
