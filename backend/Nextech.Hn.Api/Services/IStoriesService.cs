using Nextech.Hn.Api.Models;

namespace Nextech.Hn.Api.Services;

/// <summary>
/// Service for retrieving and managing Hacker News stories with caching, search, and pagination capabilities.
/// </summary>
public interface IStoriesService
{
    /// <summary>
    /// Gets the newest stories with optional search and pagination.
    /// </summary>
    /// <param name="offset">The number of items to skip. Must be >= 0.</param>
    /// <param name="limit">The maximum number of items to return. Must be between 1 and 100.</param>
    /// <param name="search">Optional search term to filter stories by title (case-insensitive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged result containing the total count and the requested stories.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or limit are outside valid ranges.</exception>
    Task<PagedResult<StoryDto>> GetNewestAsync(int offset, int limit, string? search, CancellationToken ct = default);
}
