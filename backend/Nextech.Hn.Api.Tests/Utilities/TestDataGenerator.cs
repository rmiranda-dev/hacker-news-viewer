using Nextech.Hn.Api.Models;

namespace Nextech.Hn.Api.Tests.Utilities;

/// <summary>
/// Utility class for generating test data for stories and related objects
/// </summary>
public static class TestDataGenerator
{
    /// <summary>
    /// Creates a StoryDto with predictable test data
    /// </summary>
    /// <param name="id">Story ID</param>
    /// <param name="title">Optional title override, defaults to "Story {id}"</param>
    /// <param name="useNullUrlForEvenIds">Whether even IDs should have null URLs</param>
    /// <returns>A StoryDto with test data</returns>
    public static StoryDto CreateStory(int id, string? title = null, bool useNullUrlForEvenIds = true)
    {
        return new StoryDto(
            Id: id,
            Title: title ?? $"Story {id}",
            Url: useNullUrlForEvenIds && id % 2 == 0 ? null : $"https://example.com/story/{id}",
            By: "testuser",
            Time: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );
    }

    /// <summary>
    /// Creates a sequence of story IDs for testing
    /// </summary>
    /// <param name="start">Starting ID (inclusive)</param>
    /// <param name="count">Number of IDs to generate</param>
    /// <returns>Read-only list of story IDs</returns>
    public static IReadOnlyList<int> CreateStoryIds(int start = 1, int count = 50)
    {
        return Enumerable.Range(start, count).ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a PagedResult with test stories
    /// </summary>
    /// <param name="stories">Stories to include in the result</param>
    /// <param name="total">Total count for pagination</param>
    /// <returns>PagedResult with the provided stories</returns>
    public static PagedResult<StoryDto> CreatePagedResult(IReadOnlyList<StoryDto> stories, int total)
    {
        return new PagedResult<StoryDto>(total, stories);
    }

    /// <summary>
    /// Creates a list of stories matching a search pattern
    /// </summary>
    /// <param name="searchTerm">The term to match in titles</param>
    /// <param name="maxStories">Maximum number of stories to consider (for search window simulation)</param>
    /// <returns>List of stories that would match the search term</returns>
    public static List<StoryDto> CreateMatchingStories(string searchTerm, int maxStories = 50)
    {
        var allStories = Enumerable.Range(1, maxStories)
            .Select(id => CreateStory(id))
            .ToList();

        return allStories
            .Where(story => story.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
    }
}
