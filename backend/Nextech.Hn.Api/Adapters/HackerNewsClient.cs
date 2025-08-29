using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Nextech.Hn.Api.Config;
using Nextech.Hn.Api.Models;

namespace Nextech.Hn.Api.Adapters;

/// <summary>
/// HTTP client for communicating with the Hacker News Firebase API
/// </summary>
public class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;
    private readonly HackerNewsOptions _options;
    private readonly ILogger<HackerNewsClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HackerNewsClient(
        HttpClient httpClient,
        IOptions<HackerNewsOptions> options,
        ILogger<HackerNewsClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure HttpClient
        ConfigureHttpClient();

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetNewStoryIdsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching newest story IDs from /v0/newstories.json");

            var response = await _httpClient.GetAsync("newstories.json", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var storyIds = JsonSerializer.Deserialize<int[]>(json, _jsonOptions) ?? Array.Empty<int>();

            _logger.LogInformation("Retrieved {Count} story IDs", storyIds.Length);
            return Array.AsReadOnly(storyIds);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching newest story IDs");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error while parsing newest story IDs");
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout occurred while fetching newest story IDs");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<StoryDto?> GetItemAsync(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching story {StoryId} from /v0/item/{{id}}.json", id);

            var response = await _httpClient.GetAsync($"item/{id}.json", ct);

            // Handle 404 as null (item doesn't exist)
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Story {StoryId} not found (404)", id);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var item = JsonSerializer.Deserialize<HackerNewsItem>(json, _jsonOptions);

            if (item == null)
            {
                _logger.LogWarning("Story {StoryId} deserialized to null", id);
                return null;
            }

            // Filter out invalid items
            if (item.Type != "story" || item.Dead == true || item.Deleted == true)
            {
                _logger.LogDebug("Story {StoryId} filtered out: Type={Type}, Dead={Dead}, Deleted={Deleted}", 
                    id, item.Type, item.Dead, item.Deleted);
                return null;
            }

            // Map to StoryDto
            var story = new StoryDto(
                Id: item.Id,
                Title: item.Title,
                Url: string.IsNullOrWhiteSpace(item.Url) ? null : item.Url,
                By: item.By ?? "",
                Time: item.Time
            );

            _logger.LogDebug("Successfully mapped story {StoryId}: '{Title}'", story.Id, story.Title);
            return story;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching story {StoryId}", id);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error while parsing story {StoryId}", id);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout occurred while fetching story {StoryId}", id);
            throw;
        }
    }

    private void ConfigureHttpClient()
    {
        // Normalize BaseAddress to ensure it ends with /v0/
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        if (!baseUrl.EndsWith("/v0"))
        {
            baseUrl += "/v0";
        }
        baseUrl += "/";

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);

        // Set User-Agent header
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "NextechHnViewer/1.0 (+https://github.com/rmiranda-dev/hacker-news-viewer)");

        _logger.LogDebug("HttpClient configured with BaseAddress: {BaseAddress}, Timeout: {Timeout}s", 
            _httpClient.BaseAddress, _httpClient.Timeout.TotalSeconds);
    }
}
