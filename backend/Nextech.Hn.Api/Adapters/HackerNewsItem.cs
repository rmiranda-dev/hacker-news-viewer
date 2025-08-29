namespace Nextech.Hn.Api.Adapters;

/// <summary>
/// Internal wire model matching Hacker News API JSON structure
/// </summary>
internal class HackerNewsItem
{
    /// <summary>
    /// The item's unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the story, poll or job (can be null)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The URL of the story (can be null or empty)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The username of the item's author (can be null for deleted items)
    /// </summary>
    public string? By { get; set; }

    /// <summary>
    /// Creation date of the item in Unix timestamp
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// The type of item (story, comment, job, poll, pollopt)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// True if the item is dead
    /// </summary>
    public bool? Dead { get; set; }

    /// <summary>
    /// True if the item is deleted
    /// </summary>
    public bool? Deleted { get; set; }
}
