using Nextech.Hn.Api.Models;

namespace Nextech.Hn.Api.Adapters;

/// <summary>
/// Interface for communicating with the Hacker News Firebase API
/// </summary>
public interface IHackerNewsClient
{
    /// <summary>
    /// Gets the newest story IDs from Hacker News API endpoint /v0/newstories.json
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Read-only list of story IDs (never null, empty if no results)</returns>
    Task<IReadOnlyList<int>> GetNewStoryIdsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a story item from Hacker News API endpoint /v0/item/{id}.json
    /// </summary>
    /// <param name="id">Story ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Story details or null for 404/invalid items (deleted/dead/non-story)</returns>
    Task<StoryDto?> GetItemAsync(int id, CancellationToken ct = default);
}
