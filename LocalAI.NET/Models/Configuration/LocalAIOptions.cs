using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LocalAI.NET.Models.Configuration
{
    /// <summary>
    /// Configuration options for LocalAI client
    /// </summary>
    public class LocalAIOptions
    {
        /// <summary>
        /// Base URL for the provider's API endpoint
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:5000";

        /// <summary>
        /// API key if required by the provider
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Timeout for HTTP requests
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Optional logger instance
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Custom JSON serializer settings
        /// </summary>
        public JsonSerializerSettings? JsonSettings { get; set; }

        /// <summary>
        /// Provider-specific options
        /// </summary>
        public ProviderOptions? ProviderOptions { get; set; }
    }
}