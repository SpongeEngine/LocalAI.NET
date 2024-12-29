using System.Net.Http.Headers;
using LocalAI.NET.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace LocalAI.NET.Utils
{
    /// <summary>
    /// Factory for creating configured HttpClient instances
    /// </summary>
    public static class HttpClientFactory
    {
        /// <summary>
        /// Creates an HttpClient configured with the provided options
        /// </summary>
        public static HttpClient Create(LocalAIOptions options, string? provider = null)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(options.BaseUrl),
                Timeout = options.Timeout
            };

            // Set default headers
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("LocalAI.NET", 
                    typeof(HttpClientFactory).Assembly.GetName().Version?.ToString() ?? "1.0.0"));

            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiKey);
            }

            options.Logger?.LogDebug(
                "Created HttpClient for {Provider} with base URL: {BaseUrl}",
                provider ?? "Unknown",
                options.BaseUrl);

            return client;
        }
    }
}