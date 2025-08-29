using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nextech.Hn.Api.Adapters;
using Nextech.Hn.Api.Config;

namespace Nextech.Hn.Api.Tests.Adapters;

public class HackerNewsClientTests
{
    private readonly FakeHttpMessageHandler _fakeHandler;
    private readonly HackerNewsClient _client;
    private readonly ILogger<HackerNewsClient> _logger;

    public HackerNewsClientTests()
    {
        _fakeHandler = new FakeHttpMessageHandler();
        _logger = new FakeLogger<HackerNewsClient>();
        
        var options = Options.Create(new HackerNewsOptions
        {
            BaseUrl = "https://hacker-news.firebaseio.com/v0/"
        });

        var httpClient = new HttpClient(_fakeHandler);
        httpClient.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
        
        _client = new HackerNewsClient(httpClient, options, _logger);
    }

    [Fact]
    public async Task GetNewStoryIdsAsync_ReturnsIds()
    {
        // Arrange
        _fakeHandler.SetResponse("newstories.json", "[1,2,3]");

        // Act
        var result = await _client.GetNewStoryIdsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result.Should().BeAssignableTo<IReadOnlyList<int>>();
        
        // Verify it's actually read-only
        result.GetType().Should().Match(type => 
            type.Name.Contains("ReadOnly") || 
            !type.GetInterfaces().Any(i => i.Name.Contains("IList") && !i.Name.Contains("IReadOnlyList")));
    }

    [Fact]
    public async Task GetItemAsync_MapsFields()
    {
        // Arrange
        var json = """
        {
            "id": 1,
            "title": "T",
            "url": "https://x",
            "by": "u",
            "time": 1724800000,
            "type": "story"
        }
        """;
        _fakeHandler.SetResponse("item/1.json", json);

        // Act
        var result = await _client.GetItemAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("T");
        result.Url.Should().Be("https://x");
        result.Url.Should().NotBeNull();
        result.By.Should().Be("u");
        result.Time.Should().Be(1724800000);
    }

    [Fact]
    public async Task GetItemAsync_NullForNonStoryType()
    {
        // Arrange
        var json = """
        {
            "id": 1,
            "title": "Comment Title",
            "by": "user",
            "time": 1724800000,
            "type": "comment"
        }
        """;
        _fakeHandler.SetResponse("item/1.json", json);

        // Act
        var result = await _client.GetItemAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetItemAsync_NullForDeletedItem()
    {
        // Arrange
        var json = """
        {
            "id": 1,
            "title": "Deleted Story",
            "by": "user",
            "time": 1724800000,
            "type": "story",
            "deleted": true
        }
        """;
        _fakeHandler.SetResponse("item/1.json", json);

        // Act
        var result = await _client.GetItemAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetItemAsync_NullForDeadItem()
    {
        // Arrange
        var json = """
        {
            "id": 1,
            "title": "Dead Story",
            "by": "user",
            "time": 1724800000,
            "type": "story",
            "dead": true
        }
        """;
        _fakeHandler.SetResponse("item/1.json", json);

        // Act
        var result = await _client.GetItemAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetItemAsync_404_ReturnsNull()
    {
        // Arrange
        _fakeHandler.SetNotFoundResponse("item/999.json");

        // Act
        var result = await _client.GetItemAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetItemAsync_WhitespaceUrl_NormalizedToNull()
    {
        // Arrange
        var json = """
        {
            "id": 1,
            "title": "Story with whitespace URL",
            "url": "   ",
            "by": "user",
            "time": 1724800000,
            "type": "story"
        }
        """;
        _fakeHandler.SetResponse("item/1.json", json);

        // Act
        var result = await _client.GetItemAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Url.Should().BeNull();
    }

    [Fact]
    public async Task GetNewStoryIdsAsync_NonSuccess_Throws()
    {
        // Arrange
        _fakeHandler.SetErrorResponse("newstories.json", HttpStatusCode.InternalServerError);

        // Act & Assert
        var act = async () => await _client.GetNewStoryIdsAsync();
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetItemAsync_NonSuccess_Throws()
    {
        // Arrange
        _fakeHandler.SetErrorResponse("item/1.json", HttpStatusCode.InternalServerError);

        // Act & Assert
        var act = async () => await _client.GetItemAsync(1);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task HttpClient_ConfiguredWithCorrectTimeout()
    {
        // Arrange
        _fakeHandler.SetResponse("newstories.json", "[1,2,3]");

        // Act
        await _client.GetNewStoryIdsAsync();

        // Assert
        _fakeHandler.CapturedRequest.Should().NotBeNull();
        
        // Check timeout by creating a fresh client and inspecting timeout
        var testHandler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(testHandler);
        
        var options = Options.Create(new HackerNewsOptions
        {
            BaseUrl = "https://hacker-news.firebaseio.com/v0/"
        });
        
        var testClient = new HackerNewsClient(httpClient, options, _logger);
        
        // The timeout should be configured to 10 seconds
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task HttpClient_SetsUserAgentHeader()
    {
        // Arrange
        _fakeHandler.SetResponse("newstories.json", "[1,2,3]");

        // Act
        await _client.GetNewStoryIdsAsync();

        // Assert
        _fakeHandler.CapturedRequest.Should().NotBeNull();
        var userAgentHeader = _fakeHandler.CapturedRequest!.Headers.UserAgent.ToString();
        userAgentHeader.Should().Contain("NextechHnViewer/1.0");
        userAgentHeader.Should().Contain("github.com/rmiranda-dev/hacker-news-viewer");
    }

}

/// <summary>
/// Fake HTTP message handler that returns canned responses based on request path
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses = new();
    public HttpRequestMessage? CapturedRequest { get; private set; }

    public void SetResponse(string path, string jsonContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        _responses[path] = response;
    }

    public void SetNotFoundResponse(string path)
    {
        _responses[path] = new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    public void SetErrorResponse(string path, HttpStatusCode statusCode)
    {
        _responses[path] = new HttpResponseMessage(statusCode);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CapturedRequest = request;
        
        // Extract path from the URI - handle both absolute and relative paths
        var path = request.RequestUri?.AbsolutePath ?? "";
        if (path.StartsWith("/v0/"))
        {
            path = path.Substring(4); // Remove "/v0/" prefix
        }
        else if (path.StartsWith("/"))
        {
            path = path.Substring(1); // Remove leading "/"
        }
        
        if (_responses.TryGetValue(path, out var response))
        {
            // Clone the response to avoid disposal issues
            var clonedResponse = new HttpResponseMessage(response.StatusCode);
            if (response.Content != null)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                clonedResponse.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
            return Task.FromResult(clonedResponse);
        }

        // Default to 404 if no response configured
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var response in _responses.Values)
            {
                response.Dispose();
            }
            _responses.Clear();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Fake logger implementation for testing
/// </summary>
public class FakeLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
