using Microsoft.Extensions.Caching.Memory;
using Nextech.Hn.Api.Adapters;
using Nextech.Hn.Api.Models;

namespace Nextech.Hn.Api.Services;

/// <summary>
/// Service for retrieving and managing Hacker News stories with caching, search, and pagination capabilities.
/// </summary>
public class StoriesService : IStoriesService, IDisposable
{
    private readonly IHackerNewsClient _hackerNewsClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<StoriesService> _logger;
    private readonly SemaphoreSlim _concurrencySemaphore;

    private const string NewStoriesIdsKey = "hn:newstories:ids";
    private const int SearchWindowSize = 500;
    private const int MaxConcurrentFetches = 10;

    public StoriesService(
        IHackerNewsClient hackerNewsClient,
        IMemoryCache memoryCache,
        ILogger<StoriesService> logger)
    {
        _hackerNewsClient = hackerNewsClient ?? throw new ArgumentNullException(nameof(hackerNewsClient));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _concurrencySemaphore = new SemaphoreSlim(MaxConcurrentFetches, MaxConcurrentFetches);
    }

    /// <inheritdoc />
    public async Task<PagedResult<StoryDto>> GetNewestAsync(int offset, int limit, string? search, CancellationToken ct = default)
    {
        // Validate parameters
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be >= 0");
        
        if (limit < 1 || limit > 100)
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be between 1 and 100");

        // Get newest story IDs (cached or from API)
        var newestIds = await GetNewestIdsAsync(ct);
        
        if (string.IsNullOrWhiteSpace(search))
        {
            // Flow WITHOUT search: paginate IDs first, then fetch only that slice
            return await GetStoriesWithoutSearchAsync(newestIds, offset, limit, ct);
        }
        else
        {
            // Flow WITH search: fetch window, filter by title, then paginate filtered results
            return await GetStoriesWithSearchAsync(newestIds, offset, limit, search.Trim(), ct);
        }
    }

    private async Task<IReadOnlyList<int>> GetNewestIdsAsync(CancellationToken ct)
    {
        var result = await GetOrCreateAsync(
            NewStoriesIdsKey,
            async () =>
            {
                _logger.LogDebug("Fetching newest story IDs from API");
                return await _hackerNewsClient.GetNewStoryIdsAsync(ct);
            },
            TimeSpan.FromSeconds(Random.Shared.Next(60, 121)), // 60-120 seconds TTL with jitter
            ct);
        
        return result ?? Array.Empty<int>();
    }

    private async Task<PagedResult<StoryDto>> GetStoriesWithoutSearchAsync(
        IReadOnlyList<int> allIds, 
        int offset, 
        int limit, 
        CancellationToken ct)
    {
        var totalCount = allIds.Count;
        var pageIds = allIds.Skip(offset).Take(limit).ToList();
        
        _logger.LogDebug("Fetching {Count} stories for page (offset={Offset}, limit={Limit})", 
            pageIds.Count, offset, limit);
        
        var stories = await FetchStoriesConcurrentlyAsync(pageIds, ct);
        
        return new PagedResult<StoryDto>(totalCount, stories);
    }

    private async Task<PagedResult<StoryDto>> GetStoriesWithSearchAsync(
        IReadOnlyList<int> allIds, 
        int offset, 
        int limit, 
        string searchTerm, 
        CancellationToken ct)
    {
        // Limit to first 500 IDs for search window (performance optimization)
        var windowSize = Math.Min(SearchWindowSize, allIds.Count);
        var searchIds = allIds.Take(windowSize).ToList();
        
        _logger.LogDebug("Searching in first {WindowSize} stories for term: '{SearchTerm}'", windowSize, searchTerm);
        
        // Fetch stories in the search window with order preservation
        var windowStories = await FetchStoriesConcurrentlyAsync(searchIds, ct);
        
        // Filter by search term (case-insensitive title search)
        var filteredStories = windowStories
            .Where(story => story.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
        
        _logger.LogDebug("Found {FilteredCount} stories matching '{SearchTerm}' out of {WindowSize} fetched", 
            filteredStories.Count, searchTerm, windowStories.Count);
        
        var totalCount = filteredStories.Count;
        var pageStories = filteredStories.Skip(offset).Take(limit).ToList();
        
        return new PagedResult<StoryDto>(totalCount, pageStories);
    }

    private async Task<IReadOnlyList<StoryDto>> FetchStoriesConcurrentlyAsync(IReadOnlyList<int> ids, CancellationToken ct)
    {
        if (!ids.Any())
            return Array.Empty<StoryDto>();

        // Create indexed tasks to preserve order during concurrent execution
        var indexedTasks = ids
            .Select((id, index) => new { Id = id, Index = index, Task = FetchStoryWithSemaphoreAsync(id, ct) })
            .ToList();

        // Execute all tasks concurrently
        await Task.WhenAll(indexedTasks.Select(x => x.Task));

        // Reconstruct results in original order, skipping nulls
        var orderedResults = indexedTasks
            .OrderBy(x => x.Index)
            .Select(x => x.Task.Result)
            .Where(story => story != null)
            .ToList();

        _logger.LogDebug("Fetched {SuccessCount} valid stories out of {RequestedCount} IDs", 
            orderedResults.Count, ids.Count);

        return orderedResults!;
    }

    private async Task<StoryDto?> FetchStoryWithSemaphoreAsync(int id, CancellationToken ct)
    {
        await _concurrencySemaphore.WaitAsync(ct);
        try
        {
            return await GetStoryAsync(id, ct);
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }

    private async Task<StoryDto?> GetStoryAsync(int id, CancellationToken ct)
    {
        var cacheKey = $"hn:item:{id}";
        
        return await GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Fetching story {StoryId} from API", id);
                var story = await _hackerNewsClient.GetItemAsync(id, ct);
                
                // Log whether we got a valid story or null (deleted/dead/404)
                if (story == null)
                {
                    _logger.LogDebug("Story {StoryId} returned null (deleted/dead/404)", id);
                }
                
                return story;
            },
            // Use different TTL for null vs valid items
            item => item == null 
                ? TimeSpan.FromSeconds(60) // Short TTL for nulls (allows recovery)
                : TimeSpan.FromMinutes(Random.Shared.Next(5, 11)), // 5-10 minutes for valid items
            ct);
    }

    private async Task<T?> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T?>> factory, 
        TimeSpan expiration, 
        CancellationToken ct) where T : class
    {
        return await GetOrCreateAsync(key, factory, _ => expiration, ct);
    }

    private async Task<T?> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T?>> factory, 
        Func<T?, TimeSpan> expirationFactory, 
        CancellationToken ct) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}, fetching from source", key);
        
        var value = await factory();
        var expiration = expirationFactory(value);
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            Size = 1 // Simple size tracking
        };
        
        // Cache the value (even if null) with appropriate TTL
        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cached value for key: {CacheKey} with TTL: {TTL}s (isNull: {IsNull})", 
            key, expiration.TotalSeconds, value == null);
        
        return value;
    }

    public void Dispose()
    {
        _concurrencySemaphore?.Dispose();
    }
}
