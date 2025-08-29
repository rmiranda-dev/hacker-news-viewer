using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nextech.Hn.Api.Adapters;
using Nextech.Hn.Api.Models;
using Nextech.Hn.Api.Services;
using Nextech.Hn.Api.Tests.Utilities;

namespace Nextech.Hn.Api.Tests.Services;

public class StoriesServiceTests
{
    private readonly Mock<IHackerNewsClient> _mockClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<StoriesService> _logger;
    private readonly StoriesService _service;

    public StoriesServiceTests()
    {
        _mockClient = new Mock<IHackerNewsClient>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _logger = NullLogger<StoriesService>.Instance;
        _service = new StoriesService(_mockClient.Object, _memoryCache, _logger);
    }

    [Fact]
    public async Task NoSearch_PagesCorrectly_PreservesOrder()
    {
        // Arrange
        var ids = Enumerable.Range(1, 30).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        // Setup adapter to return StoryDto for each id
        foreach (var id in ids)
        {
            _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new StoryDto(
                           Id: id,
                           Title: $"Story {id}",
                           Url: $"https://example.com/{id}",
                           By: "testuser",
                           Time: 1724800000));
        }

        // Act
        var result = await _service.GetNewestAsync(offset: 10, limit: 5, search: null);

        // Assert
        result.Total.Should().Be(30);
        result.Items.Should().HaveCount(5);
        result.Items.Select(x => x.Id).Should().BeEquivalentTo(new[] { 11, 12, 13, 14, 15 }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Search_FiltersCaseInsensitive_PaginatesAndTotals()
    {
        // Arrange
        var ids = Enumerable.Range(1, 40).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        // Setup stories with "Azure" in titles for ids 5, 12, 33, 34
        var azureIds = new[] { 5, 12, 33, 34 };
        
        foreach (var id in ids)
        {
            var title = azureIds.Contains(id) ? $"Story about Azure {id}" : $"Story {id}";
            _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new StoryDto(
                           Id: id,
                           Title: title,
                           Url: $"https://example.com/{id}",
                           By: "testuser",
                           Time: 1724800000));
        }

        // Act - search for "azure" with offset=1, limit=2 (2nd and 3rd matches)
        var result = await _service.GetNewestAsync(offset: 1, limit: 2, search: "azure");

        // Assert
        result.Total.Should().Be(4); // Total matches found
        result.Items.Should().HaveCount(2); // Requested page size
        result.Items.Select(x => x.Id).Should().BeEquivalentTo(new[] { 12, 33 }, options => options.WithStrictOrdering());
        result.Items.All(x => x.Title!.Contains("Azure", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task UsesCache_ForIds_SecondCallDoesNotReFetch()
    {
        // Arrange
        var ids = Enumerable.Range(1, 10).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        foreach (var id in ids)
        {
            _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new StoryDto(
                           Id: id,
                           Title: $"Story {id}",
                           Url: $"https://example.com/{id}",
                           By: "testuser",
                           Time: 1724800000));
        }

        // Act - Make two identical calls
        await _service.GetNewestAsync(offset: 0, limit: 5, search: null);
        await _service.GetNewestAsync(offset: 0, limit: 5, search: null);

        // Assert - GetNewStoryIdsAsync should be called only once due to caching
        _mockClient.Verify(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UsesCache_ForItems_SecondCallAvoidsRefetch()
    {
        // Arrange
        var ids = Enumerable.Range(1, 10).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        foreach (var id in ids)
        {
            _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new StoryDto(
                           Id: id,
                           Title: $"Story {id}",
                           Url: $"https://example.com/{id}",
                           By: "testuser",
                           Time: 1724800000));
        }

        // Act - Make two calls that would fetch the same items (1-5)
        await _service.GetNewestAsync(offset: 0, limit: 5, search: null);
        await _service.GetNewestAsync(offset: 0, limit: 5, search: null);

        // Assert - Each item should be fetched only once due to caching
        foreach (var id in ids.Take(5))
        {
            _mockClient.Verify(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task SkipsNullItems_Defensive()
    {
        // Arrange
        var ids = Enumerable.Range(1, 5).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        // Setup id=3 to return null (deleted/dead), others return valid stories
        foreach (var id in ids)
        {
            if (id == 3)
            {
                _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((StoryDto?)null);
            }
            else
            {
                _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new StoryDto(
                               Id: id,
                               Title: $"Story {id}",
                               Url: $"https://example.com/{id}",
                               By: "testuser",
                               Time: 1724800000));
            }
        }

        // Act
        var result = await _service.GetNewestAsync(offset: 0, limit: 10, search: null);

        // Assert
        result.Total.Should().Be(5); // Total reflects all IDs including the null one
        result.Items.Should().HaveCount(4); // But returned items exclude the null
        result.Items.Select(x => x.Id).Should().BeEquivalentTo(new[] { 1, 2, 4, 5 }, options => options.WithStrictOrdering());
    }

    [Theory]
    [InlineData(-1, 5)] // negative offset
    [InlineData(0, 0)]  // limit too small
    [InlineData(0, 101)] // limit too large
    public async Task Throws_OnInvalidOffsetOrLimit(int offset, int limit)
    {
        // Act & Assert
        var act = async () => await _service.GetNewestAsync(offset, limit, search: null);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task RespectsCancellationToken()
    {
        // Arrange
        var ids = Enumerable.Range(1, 10).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        // Setup a slow GetItemAsync call that respects cancellation
        _mockClient.Setup(x => x.GetItemAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .Returns(async (int id, CancellationToken ct) =>
                   {
                       await Task.Delay(TestSettings.Timing.MockAsyncDelay, ct); // Short delay that will be cancelled
                       return new StoryDto(
                           Id: id,
                           Title: $"Story {id}",
                           Url: $"https://example.com/{id}",
                           By: "testuser",
                           Time: 1724800000);
                   });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TestSettings.Timing.ShortCancellationTimeout); // Cancel after very short time to make test fast and deterministic

        // Act & Assert
        var act = async () => await _service.GetNewestAsync(offset: 0, limit: 5, search: null, ct: cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task BoundedConcurrency_ProcessesWithinReasonableTime()
    {
        // Arrange
        var ids = Enumerable.Range(1, 20).ToList(); // 20 items to test concurrency
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ids.AsReadOnly());

        // Setup each GetItemAsync to take 100ms - with 10 concurrent, should take ~200ms total for 20 items
        foreach (var id in ids)
        {
            _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                       .Returns(async () =>
                       {
                           await Task.Delay(100);
                           return new StoryDto(
                               Id: id,
                               Title: $"Story {id}",
                               Url: $"https://example.com/{id}",
                               By: "testuser",
                               Time: 1724800000);
                       });
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.GetNewestAsync(offset: 0, limit: 20, search: null);
        stopwatch.Stop();

        // Assert
        result.Items.Should().HaveCount(20);
        
        // With bounded concurrency of 10, 20 items taking 100ms each should complete in roughly 200ms
        // Allow some tolerance for test execution overhead
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            "bounded concurrency should prevent this from taking 2000ms (20 * 100ms sequential)");
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(150, 
            "should take at least 2 batches worth of time");
    }

    [Fact]
    public async Task Search_OnlySearchesWithinWindow()
    {
        // Arrange - Create more IDs than the search window (500)
        var allIds = Enumerable.Range(1, 600).ToList();
        _mockClient.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(allIds.AsReadOnly());

        // Only setup responses for the first 500 IDs (within search window)
        for (int id = 1; id <= 500; id++)
        {
            var title = id == 250 ? "Special Azure Story" : $"Story {id}";
            _mockClient.Setup(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new StoryDto(
                           Id: id,
                           Title: title,
                           Url: $"https://example.com/{id}",
                           By: "testuser",
                           Time: 1724800000));
        }

        // Act - Search for "azure"
        var result = await _service.GetNewestAsync(offset: 0, limit: 10, search: "azure");

        // Assert
        result.Total.Should().Be(1); // Only one match within the 500-item window
        result.Items.Should().HaveCount(1);
        result.Items.First().Id.Should().Be(250);

        // Verify that only items within the search window were fetched
        for (int id = 501; id <= 600; id++)
        {
            _mockClient.Verify(x => x.GetItemAsync(id, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
