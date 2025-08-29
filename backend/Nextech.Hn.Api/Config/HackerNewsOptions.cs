namespace Nextech.Hn.Api.Config;

/// <summary>
/// Configuration options for Hacker News API integration
/// </summary>
public class HackerNewsOptions
{
    /// <summary>
    /// Gets or sets the base URL for the Hacker News Firebase API
    /// </summary>
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
}
