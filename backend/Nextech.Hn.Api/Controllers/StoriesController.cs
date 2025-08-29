using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nextech.Hn.Api.Models;
using Nextech.Hn.Api.Services;

namespace Nextech.Hn.Api.Controllers;

/// <summary>
/// Controller for managing Hacker News stories with search and pagination capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoriesService _storiesService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    /// <summary>
    /// Initializes a new instance of the StoriesController.
    /// </summary>
    /// <param name="storiesService">Service for retrieving stories.</param>
    /// <param name="problemDetailsFactory">Factory for creating consistent problem details.</param>
    public StoriesController(
        IStoriesService storiesService,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _storiesService = storiesService ?? throw new ArgumentNullException(nameof(storiesService));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    /// <summary>
    /// Returns newest Hacker News stories with optional title search and paging.
    /// </summary>
    /// <param name="offset">The number of items to skip. Must be >= 0.</param>
    /// <param name="limit">The maximum number of items to return. Must be between 1 and 100.</param>
    /// <param name="search">Optional search term to filter stories by title (case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>
    /// A paged result containing stories and total count.
    /// Total semantics: Without search = count of all newest IDs. 
    /// With search = filtered count within the scanned window (first 500 IDs).
    /// </returns>
    /// <response code="200">Returns the paged list of stories.</response>
    /// <response code="400">Invalid query parameters provided.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("new")]
    [ProducesResponseType(typeof(PagedResult<StoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<PagedResult<StoryDto>>> GetNewestAsync(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        // Light controller validation for immediate feedback
        if (offset < 0)
        {
            return BadRequest(CreateProblemDetails("Offset must be >= 0"));
        }

        if (limit < 1 || limit > 100)
        {
            return BadRequest(CreateProblemDetails("Limit must be between 1 and 100"));
        }

        try
        {
            // Call service with request cancellation token
            var result = await _storiesService.GetNewestAsync(
                offset, limit, search, HttpContext.RequestAborted);
            
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Catch service validation exceptions and translate to consistent 400 response
            return BadRequest(CreateProblemDetails(ex.Message));
        }
    }

    /// <summary>
    /// Creates a ProblemDetails object with consistent formatting.
    /// </summary>
    /// <param name="detail">The error detail message.</param>
    /// <param name="statusCode">The HTTP status code (defaults to 400).</param>
    /// <returns>A properly formatted ProblemDetails object.</returns>
    private ProblemDetails CreateProblemDetails(string detail, int statusCode = StatusCodes.Status400BadRequest)
    {
        return _problemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: statusCode,
            detail: detail);
    }
}
