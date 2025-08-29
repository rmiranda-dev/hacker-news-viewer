using FluentAssertions;
using Nextech.Hn.Api.Models;
using System.Net;
using System.Net.Http.Json;

namespace Nextech.Hn.Api.Tests.Utilities;

/// <summary>
/// Extension methods for HttpClient to simplify API testing
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Gets stories from the API with optional parameters
    /// </summary>
    /// <param name="client">HTTP client</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="search">Optional search term</param>
    /// <returns>HTTP response message</returns>
    public static async Task<HttpResponseMessage> GetStoriesAsync(
        this HttpClient client, 
        int offset = 0, 
        int limit = 20, 
        string? search = null)
    {
        var queryParams = new List<string>
        {
            $"offset={offset}",
            $"limit={limit}"
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        }

        var queryString = string.Join("&", queryParams);
        return await client.GetAsync($"/api/stories/new?{queryString}");
    }

    /// <summary>
    /// Gets and deserializes a paged result from the stories API
    /// </summary>
    /// <param name="client">HTTP client</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="search">Optional search term</param>
    /// <returns>Tuple of response and deserialized result</returns>
    public static async Task<(HttpResponseMessage Response, PagedResult<StoryDto>? Result)> GetStoriesWithResultAsync(
        this HttpClient client,
        int offset = 0,
        int limit = 20,
        string? search = null)
    {
        var response = await client.GetStoriesAsync(offset, limit, search);
        var result = response.IsSuccessStatusCode 
            ? await response.Content.ReadFromJsonAsync<PagedResult<StoryDto>>()
            : null;
        
        return (response, result);
    }
}
