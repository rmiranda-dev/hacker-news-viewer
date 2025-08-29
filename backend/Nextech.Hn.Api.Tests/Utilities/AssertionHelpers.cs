using FluentAssertions;
using Nextech.Hn.Api.Models;
using System.Net;

namespace Nextech.Hn.Api.Tests.Utilities;

/// <summary>
/// Utility methods for common test assertions
/// </summary>
public static class AssertionHelpers
{
    /// <summary>
    /// Asserts that an HTTP response is a successful JSON response
    /// </summary>
    /// <param name="response">HTTP response to validate</param>
    public static void ShouldBeSuccessfulJsonResponse(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    /// <summary>
    /// Asserts that an HTTP response is a bad request with problem details
    /// </summary>
    /// <param name="response">HTTP response to validate</param>
    public static void ShouldBeBadRequestWithProblemDetails(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    /// <summary>
    /// Asserts that a paged result has the expected structure and counts
    /// </summary>
    /// <param name="result">Paged result to validate</param>
    /// <param name="expectedTotal">Expected total count</param>
    /// <param name="expectedItemCount">Expected number of items in current page</param>
    public static void ShouldHaveCorrectPagination(this PagedResult<StoryDto>? result, int expectedTotal, int expectedItemCount)
    {
        result.Should().NotBeNull();
        result!.Total.Should().Be(expectedTotal);
        result.Items.Should().HaveCount(expectedItemCount);
    }

    /// <summary>
    /// Asserts that all stories follow the expected URL pattern
    /// </summary>
    /// <param name="stories">Stories to validate</param>
    /// <param name="evenIdsShouldHaveNullUrl">Whether even IDs should have null URLs</param>
    public static void ShouldFollowUrlPattern(this IEnumerable<StoryDto> stories, bool evenIdsShouldHaveNullUrl = true)
    {
        foreach (var story in stories)
        {
            if (evenIdsShouldHaveNullUrl && story.Id % 2 == 0)
            {
                story.Url.Should().BeNull($"Even ID {story.Id} should have null URL");
            }
            else
            {
                story.Url.Should().NotBeNull($"Odd ID {story.Id} should have non-null URL");
            }
        }
    }

    /// <summary>
    /// Asserts that all stories contain the search term in their titles
    /// </summary>
    /// <param name="stories">Stories to validate</param>
    /// <param name="searchTerm">Search term that should be contained in titles</param>
    public static void ShouldAllContainInTitle(this IEnumerable<StoryDto> stories, string searchTerm)
    {
        foreach (var story in stories)
        {
            story.Title.Should().NotBeNull();
            story.Title!.ToLower().Should().Contain(searchTerm.ToLower(), 
                "because search should be case-insensitive");
        }
    }

    /// <summary>
    /// Asserts that stories have sequential IDs starting from a specific offset
    /// </summary>
    /// <param name="stories">Stories to validate</param>
    /// <param name="startingId">Expected starting ID</param>
    public static void ShouldHaveSequentialIds(this IEnumerable<StoryDto> stories, int startingId)
    {
        var expectedIds = stories.Select((_, index) => startingId + index).ToArray();
        var actualIds = stories.Select(s => s.Id).ToArray();
        actualIds.Should().BeEquivalentTo(expectedIds, options => options.WithStrictOrdering());
    }
}
