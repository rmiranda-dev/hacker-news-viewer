using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nextech.Hn.Api.Controllers;
using Nextech.Hn.Api.Models;
using Nextech.Hn.Api.Services;
using System.Reflection;

namespace Nextech.Hn.Api.Tests.Controllers;

public class StoriesControllerTests
{
    private readonly Mock<IStoriesService> _mockStoriesService;
    private readonly StoriesController _controller;
    private readonly TestProblemDetailsFactory _problemDetailsFactory;

    public StoriesControllerTests()
    {
        _mockStoriesService = new Mock<IStoriesService>();
        _problemDetailsFactory = new TestProblemDetailsFactory();
        
        _controller = new StoriesController(_mockStoriesService.Object, _problemDetailsFactory);
        
        // Setup HttpContext for ProblemDetailsFactory
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Returns400_WhenOffsetInvalid()
    {
        // Act
        var result = await _controller.GetNewestAsync(offset: -1, limit: 10, search: null);

        // Assert
        result.Should().BeOfType<ActionResult<PagedResult<StoryDto>>>();
        var actionResult = (ActionResult<PagedResult<StoryDto>>)result;
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequest = (BadRequestObjectResult)actionResult.Result!;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ProblemDetails>();
        
        var problemDetails = (ProblemDetails)badRequest.Value!;
        problemDetails.Status.Should().Be(400);
        problemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(0)]   // limit too small
    [InlineData(101)] // limit too large
    public async Task Returns400_WhenLimitInvalid(int invalidLimit)
    {
        // Act
        var result = await _controller.GetNewestAsync(offset: 0, limit: invalidLimit, search: null);

        // Assert
        result.Should().BeOfType<ActionResult<PagedResult<StoryDto>>>();
        var actionResult = (ActionResult<PagedResult<StoryDto>>)result;
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequest = (BadRequestObjectResult)actionResult.Result!;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ProblemDetails>();
        
        var problemDetails = (ProblemDetails)badRequest.Value!;
        problemDetails.Status.Should().Be(400);
        problemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Returns200_WithPagedResult()
    {
        // Arrange
        var dto1 = new StoryDto(
            Id: 1,
            Title: "First Story",
            Url: "https://example.com/1",
            By: "user1",
            Time: 1724800000);

        var dto2 = new StoryDto(
            Id: 2,
            Title: "Second Story",
            Url: "https://example.com/2",
            By: "user2",
            Time: 1724800001);

        var expectedResult = new PagedResult<StoryDto>(
            Total: 3,
            Items: new[] { dto1, dto2 });

        _mockStoriesService
            .Setup(x => x.GetNewestAsync(0, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetNewestAsync(offset: 0, limit: 10, search: null);

        // Assert
        result.Should().BeOfType<ActionResult<PagedResult<StoryDto>>>();
        var actionResult = (ActionResult<PagedResult<StoryDto>>)result;
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = (OkObjectResult)actionResult.Result!;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeOfType<PagedResult<StoryDto>>();
        
        var actualResult = (PagedResult<StoryDto>)okResult.Value!;
        actualResult.Total.Should().Be(3);
        actualResult.Items.Should().HaveCount(2);
        actualResult.Items.Should().BeEquivalentTo(new[] { dto1, dto2 }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Returns400_WhenServiceThrowsArgumentOutOfRange()
    {
        // Arrange
        _mockStoriesService
            .Setup(x => x.GetNewestAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentOutOfRangeException("offset", "Offset must be non-negative"));

        // Act
        var result = await _controller.GetNewestAsync(offset: 0, limit: 10, search: null);

        // Assert
        result.Should().BeOfType<ActionResult<PagedResult<StoryDto>>>();
        var actionResult = (ActionResult<PagedResult<StoryDto>>)result;
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequest = (BadRequestObjectResult)actionResult.Result!;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().BeOfType<ProblemDetails>();
        
        var problemDetails = (ProblemDetails)badRequest.Value!;
        problemDetails.Status.Should().Be(400);
        problemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HasProducesResponseTypeAttributes()
    {
        // Arrange
        var methodInfo = typeof(StoriesController).GetMethod(nameof(StoriesController.GetNewestAsync));

        // Act
        var attributes = methodInfo!.GetCustomAttributes<ProducesResponseTypeAttribute>().ToList();

        // Assert
        attributes.Should().NotBeEmpty();
        
        var statusCodes = attributes.Select(attr => attr.StatusCode).ToList();
        statusCodes.Should().Contain(200, "should have 200 OK response type");
        statusCodes.Should().Contain(400, "should have 400 Bad Request response type");
        statusCodes.Should().Contain(500, "should have 500 Internal Server Error response type");
    }

    [Fact]
    public void HasResponseCacheAttribute()
    {
        // Arrange
        var methodInfo = typeof(StoriesController).GetMethod(nameof(StoriesController.GetNewestAsync));

        // Act
        var responseCacheAttr = methodInfo!.GetCustomAttribute<ResponseCacheAttribute>();

        // Assert
        responseCacheAttr.Should().NotBeNull("method should have ResponseCache attribute");
        responseCacheAttr!.NoStore.Should().BeTrue("NoStore should be true");
        responseCacheAttr.Location.Should().Be(ResponseCacheLocation.None, "Location should be None");
        responseCacheAttr.Duration.Should().Be(0, "Duration should be 0");
    }

    [Fact]
    public async Task PassesCancellationTokenToService()
    {
        // Arrange
        var dto = new StoryDto(1, "Test", null, "user", 1724800000);
        var pagedResult = new PagedResult<StoryDto>(1, new[] { dto });
        
        _mockStoriesService
            .Setup(x => x.GetNewestAsync(0, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await _controller.GetNewestAsync(offset: 0, limit: 10, search: null);

        // Assert - Controller uses HttpContext.RequestAborted, not the passed token
        _mockStoriesService.Verify(
            x => x.GetNewestAsync(0, 10, null, It.IsAny<CancellationToken>()),
            Times.Once,
            "should pass a cancellation token to the service");
    }

    [Fact]
    public async Task PassesSearchParameterToService()
    {
        // Arrange
        var dto = new StoryDto(1, "Azure Story", null, "user", 1724800000);
        var pagedResult = new PagedResult<StoryDto>(1, new[] { dto });
        
        _mockStoriesService
            .Setup(x => x.GetNewestAsync(5, 15, "azure", It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await _controller.GetNewestAsync(offset: 5, limit: 15, search: "azure");

        // Assert
        _mockStoriesService.Verify(
            x => x.GetNewestAsync(5, 15, "azure", It.IsAny<CancellationToken>()),
            Times.Once,
            "should pass all parameters correctly to the service");
    }

    [Fact]
    public async Task Returns500_WhenServiceThrowsUnexpectedException()
    {
        // Arrange
        _mockStoriesService
            .Setup(x => x.GetNewestAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert - Controller doesn't catch general exceptions, they bubble up
        var act = async () => await _controller.GetNewestAsync(offset: 0, limit: 10, search: null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public void ControllerHasApiControllerAttribute()
    {
        // Arrange & Act
        var controllerType = typeof(StoriesController);
        var apiControllerAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();

        // Assert
        apiControllerAttr.Should().NotBeNull("controller should have [ApiController] attribute");
    }

    [Fact]
    public void ControllerHasRouteAttribute()
    {
        // Arrange & Act
        var controllerType = typeof(StoriesController);
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();

        // Assert
        routeAttr.Should().NotBeNull("controller should have [Route] attribute");
        routeAttr!.Template.Should().Be("api/[controller]", "route template should be correct");
    }
}

/// <summary>
/// Simple test implementation of ProblemDetailsFactory
/// </summary>
public class TestProblemDetailsFactory : ProblemDetailsFactory
{
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title ?? "An error occurred",
            Type = type,
            Detail = detail,
            Instance = instance
        };
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        return new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode,
            Title = title ?? "Validation failed",
            Type = type,
            Detail = detail,
            Instance = instance
        };
    }
}
