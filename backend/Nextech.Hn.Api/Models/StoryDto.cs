namespace Nextech.Hn.Api.Models;

/// <summary>
/// Represents a Hacker News story data transfer object
/// </summary>
/// <param name="Id">The unique identifier of the story</param>
/// <param name="Title">The title of the story</param>
/// <param name="Url">The URL of the story</param>
/// <param name="By">The username of the story author</param>
/// <param name="Time">The Unix timestamp when the story was created</param>
public record StoryDto(
    int Id,
    string? Title,
    string? Url,
    string By,
    long Time
);
