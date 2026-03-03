using HackerRankAPI.Application.Stories;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HackerRankAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StoriesController(
    IBestStoriesService bestStoriesService,
    IOptions<BestStoriesOptions> options) : ControllerBase
{
    private readonly IBestStoriesService _bestStoriesService = bestStoriesService;
    private readonly BestStoriesOptions _options = options.Value;

    /// <summary>
    /// Returns the first n best stories from Hacker News, sorted by score in descending order.
    /// </summary>
    /// <param name="n">Number of best stories to return. If omitted, the configured default is used.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>Array of best stories matching the required response contract.</returns>
    [HttpGet("best")]
    [EnableRateLimiting("storiesPolicy")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BestStoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IReadOnlyCollection<BestStoryResponse>>> GetBestStories(
        [FromQuery] int? n,
        CancellationToken cancellationToken)
    {
        var requestedCount = n ?? _options.DefaultStoriesCount;
        if (requestedCount <= 0 || requestedCount > _options.MaxStoriesPerRequest)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid query parameter",
                Detail = $"The query parameter 'n' must be between 1 and {_options.MaxStoriesPerRequest}.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var stories = await _bestStoriesService.GetBestStoriesAsync(requestedCount, cancellationToken);
        var result = stories
            .Select(story => new BestStoryResponse(
                story.Title,
                story.Uri,
                story.PostedBy,
                story.Time,
                story.Score,
                story.CommentCount))
            .ToArray();

        return Ok(result);
    }

    public sealed record BestStoryResponse(
        string Title,
        string Uri,
        string PostedBy,
        DateTimeOffset Time,
        int Score,
        int CommentCount);
}