using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace LocalAI.NET.Utils
{
    /// <summary>
    /// Helper for creating Polly policies
    /// </summary>
    public static class PolicyHelper
    {
        /// <summary>
        /// Creates a retry policy based on the provided options
        /// </summary>
        public static AsyncRetryPolicy CreateRetryPolicy(LocalAIOptions options)
        {
            return Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<LocalAIException>(ex => ex.StatusCode >= 500)
                .WaitAndRetryAsync(
                    options.MaxRetryAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), // Exponential backoff
                    onRetry: (ex, timeSpan, attempt, ctx) =>
                    {
                        options.Logger?.LogWarning(ex,
                            "Attempt {Attempt} of {MaxAttempts} failed, retrying after {Delay}ms",
                            attempt, 
                            options.MaxRetryAttempts,
                            timeSpan.TotalMilliseconds);
                    });
        }

        /// <summary>
        /// Creates a timeout policy based on the provided options
        /// </summary>
        public static AsyncTimeoutPolicy CreateTimeoutPolicy(LocalAIOptions options)
        {
            return Policy.TimeoutAsync(options.Timeout);
        }

        /// <summary>
        /// Creates a circuit breaker policy
        /// </summary>
        public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(LocalAIOptions options)
        {
            return Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<LocalAIException>(ex => ex.StatusCode >= 500)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (ex, duration) =>
                    {
                        options.Logger?.LogWarning(ex,
                            "Circuit breaker opened for {Duration}s", 
                            duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        options.Logger?.LogInformation("Circuit breaker reset");
                    });
        }
    }
}