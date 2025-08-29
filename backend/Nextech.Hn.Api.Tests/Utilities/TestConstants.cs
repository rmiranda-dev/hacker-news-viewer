namespace Nextech.Hn.Api.Tests.Utilities;

/// <summary>
/// Constants and configuration values for testing
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// Default test data configuration
    /// </summary>
    public static class TestData
    {
        public const int TotalStories = 50;
        public const int DefaultPageSize = 20;
        public const string TestUsername = "testuser";
        public const string BaseUrl = "https://example.com/story/";
    }

    /// <summary>
    /// API endpoint paths
    /// </summary>
    public static class ApiPaths
    {
        public const string NewStories = "/api/stories/new";
    }

    /// <summary>
    /// Expected search result counts for test data
    /// </summary>
    public static class SearchResults
    {
        // For "Story 3" - matches Story 3, 13, 23, 30-37
        public const int Story3Matches = 11;
        
        // For "Story 1" - matches Story 1, 10-19  
        public const int Story1Matches = 11;
        
        // For "Story 50" - matches only Story 50
        public const int Story50Matches = 1;
        
        // For "Story" - matches all stories
        public const int StoryMatches = 50;
        
        // For non-existent terms
        public const int NoMatches = 0;
    }

    /// <summary>
    /// HTTP content types
    /// </summary>
    public static class ContentTypes
    {
        public const string ApplicationJson = "application/json";
        public const string ProblemJson = "application/problem+json";
    }

    /// <summary>
    /// Error messages that should be returned by the API
    /// </summary>
    public static class ErrorMessages
    {
        public const string NegativeOffset = "Offset must be >= 0";
        public const string InvalidLimitTooLow = "Limit must be between 1 and 100";
        public const string InvalidLimitTooHigh = "Limit must be between 1 and 100";
    }
}
