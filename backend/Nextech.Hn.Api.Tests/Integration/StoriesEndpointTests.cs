using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Nextech.Hn.Api.Adapters;
using Nextech.Hn.Api.Models;
using Nextech.Hn.Api.Tests.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace Nextech.Hn.Api.Tests.Integration;

/// <summary>
/// Integration tests for the Stories API endpoints using a test server
/// </summary>
public class StoriesEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public StoriesEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNewest_WithDefaultPaging_ShouldReturnCorrectFormat()
    {
        // Act
        var (response, result) = await _client.GetStoriesWithResultAsync();

        // Assert
        response.ShouldBeSuccessfulJsonResponse();
        result.ShouldHaveCorrectPagination(TestConstants.TestData.TotalStories, TestConstants.TestData.DefaultPageSize);
    }

    [Fact]
    public async Task GetNewest_ShouldReturnStoriesWithCorrectUrlPattern()
    {
        // Act
        var (response, result) = await _client.GetStoriesWithResultAsync();

        // Assert
        response.ShouldBeSuccessfulJsonResponse();
        result.Should().NotBeNull();
        result!.Items.ShouldFollowUrlPattern();
    }

    [Fact]
    public async Task GetNewest_WithCustomPaging_ShouldReturnCorrectRange()
    {
        // Act
        var (response, result) = await _client.GetStoriesWithResultAsync(offset: 10, limit: 5);

        // Assert
        response.ShouldBeSuccessfulJsonResponse();
        result.ShouldHaveCorrectPagination(TestConstants.TestData.TotalStories, 5);
        result!.Items.ShouldHaveSequentialIds(startingId: 11);
    }

    [Theory]
    [InlineData("Story 3", TestConstants.SearchResults.Story3Matches)] // Should match "Story 3", "Story 13", "Story 23", "Story 30", "Story 31", "Story 32", "Story 33", "Story 34", "Story 35", "Story 36", "Story 37"
    [InlineData("STORY 3", TestConstants.SearchResults.Story3Matches)] // Case insensitive, same as above
    [InlineData("Story", TestConstants.SearchResults.StoryMatches)]  // Should match all stories
    [InlineData("Story 1", TestConstants.SearchResults.Story1Matches)] // Should match "Story 1", "Story 10", "Story 11", "Story 12", "Story 13", "Story 14", "Story 15", "Story 16", "Story 17", "Story 18", "Story 19"
    [InlineData("Story 50", TestConstants.SearchResults.Story50Matches)] // Should match only "Story 50"
    [InlineData("nonexistent", TestConstants.SearchResults.NoMatches)] // Should match nothing
    public async Task GetNewest_WithSearch_ShouldFilterCorrectly(string searchTerm, int expectedTotal)
    {
        // Act
        var (response, result) = await _client.GetStoriesWithResultAsync(search: searchTerm);

        // Assert
        response.ShouldBeSuccessfulJsonResponse();
        result.ShouldHaveCorrectPagination(expectedTotal, Math.Min(expectedTotal, TestConstants.TestData.DefaultPageSize));
        
        if (result!.Items.Any())
        {
            result.Items.ShouldAllContainInTitle(searchTerm);
        }
    }

    [Fact]
    public async Task GetNewest_WithSearchAndPaging_ShouldWorkCorrectly()
    {
        // Search for "Story 1" which should match Story 1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 = 11 items
        // Act
        var (response, result) = await _client.GetStoriesWithResultAsync(offset: 5, limit: 3, search: "Story 1");

        // Assert
        response.ShouldBeSuccessfulJsonResponse();
        result.ShouldHaveCorrectPagination(TestConstants.SearchResults.Story1Matches, 3);
        result!.Items.ShouldAllContainInTitle("Story 1");
    }

    [Theory]
    [InlineData(-1, 20, TestConstants.ErrorMessages.NegativeOffset)]
    [InlineData(0, 0, TestConstants.ErrorMessages.InvalidLimitTooLow)]
    [InlineData(0, 101, TestConstants.ErrorMessages.InvalidLimitTooHigh)]
    public async Task GetNewest_WithInvalidParameters_ShouldReturnBadRequest(int offset, int limit, string expectedError)
    {
        // Act
        var response = await _client.GetStoriesAsync(offset, limit);

        // Assert
        response.ShouldBeBadRequestWithProblemDetails();
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(expectedError);
    }

    [Fact]
    public async Task GetNewest_ShouldReturnConsistentResults()
    {
        // Make multiple requests to ensure deterministic behavior
        var (response1, result1) = await _client.GetStoriesWithResultAsync(limit: 10);
        var (response2, result2) = await _client.GetStoriesWithResultAsync(limit: 10);

        // Both should succeed
        response1.ShouldBeSuccessfulJsonResponse();
        response2.ShouldBeSuccessfulJsonResponse();

        // Results should be identical (deterministic)
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task GetNewest_WithHighOffset_ShouldReturnEmptyResults()
    {
        // Act - Request beyond available data
        var (response, result) = await _client.GetStoriesWithResultAsync(offset: 100, limit: 10);

        // Assert
        response.ShouldBeSuccessfulJsonResponse();
        result.ShouldHaveCorrectPagination(TestConstants.TestData.TotalStories, 0);
    }
}

/// <summary>
/// Custom WebApplicationFactory that replaces IHackerNewsClient with a fake implementation
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all existing IHackerNewsClient registrations
            var descriptors = services.Where(d => d.ServiceType == typeof(IHackerNewsClient)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add our fake implementation
            services.AddScoped<IHackerNewsClient, FakeHnClient>();
        });
    }
}

/// <summary>
/// Fake implementation of IHackerNewsClient for deterministic testing
/// </summary>
public class FakeHnClient : IHackerNewsClient
{
    /// <summary>
    /// Returns story IDs 1 through 50
    /// </summary>
    public Task<IReadOnlyList<int>> GetNewStoryIdsAsync(CancellationToken ct = default)
    {
        var ids = TestDataGenerator.CreateStoryIds(start: 1, count: TestConstants.TestData.TotalStories);
        return Task.FromResult(ids);
    }

    /// <summary>
    /// Returns StoryDto with Title "Story {id}", and Url = null for even IDs
    /// </summary>
    public Task<StoryDto?> GetItemAsync(int id, CancellationToken ct = default)
    {
        // Return null for IDs outside our range (shouldn't happen in these tests)
        if (id < 1 || id > TestConstants.TestData.TotalStories)
        {
            return Task.FromResult<StoryDto?>(null);
        }

        var story = TestDataGenerator.CreateStory(id, useNullUrlForEvenIds: true);
        return Task.FromResult<StoryDto?>(story);
    }
}
