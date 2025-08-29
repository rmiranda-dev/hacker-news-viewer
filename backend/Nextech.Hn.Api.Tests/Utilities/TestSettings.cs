namespace Nextech.Hn.Api.Tests.Utilities;

/// <summary>
/// Centralized test settings and configurations for deterministic testing
/// </summary>
public static class TestSettings
{
    /// <summary>
    /// Timing configurations for non-flaky tests
    /// </summary>
    public static class Timing
    {
        /// <summary>
        /// Very short timeout for cancellation tests (milliseconds)
        /// </summary>
        public const int ShortCancellationTimeout = 10;
        
        /// <summary>
        /// Delay used in mocked async operations (milliseconds)
        /// </summary>
        public const int MockAsyncDelay = 100;
        
        /// <summary>
        /// Maximum time to wait for concurrent operations in tests (milliseconds)
        /// </summary>
        public const int ConcurrencyTestTimeout = 5000;
    }

    /// <summary>
    /// Cache configurations for testing
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Default cache size limit for tests
        /// </summary>
        public const int DefaultSizeLimit = 1000;
        
        /// <summary>
        /// Whether to use sliding expiration in tests (false for deterministic tests)
        /// </summary>
        public const bool UseSlidingExpiration = false;
    }

    /// <summary>
    /// HTTP configurations for testing
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Default timeout for HTTP operations in tests (milliseconds)
        /// </summary>
        public const int DefaultTimeoutMs = 30000;
    }
}
