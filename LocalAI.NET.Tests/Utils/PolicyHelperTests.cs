using Xunit;
using FluentAssertions;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace LocalAI.NET.Tests.Utils
{
    public class PolicyHelperTests
    {
        private readonly LocalAIOptions _options;
        private readonly Mock<ILogger> _mockLogger;

        public PolicyHelperTests()
        {
            _mockLogger = new Mock<ILogger>();
            _options = new LocalAIOptions
            {
                MaxRetryAttempts = 3,
                RetryDelay = TimeSpan.FromSeconds(2),
                Timeout = TimeSpan.FromSeconds(30),
                Logger = _mockLogger.Object
            };
        }

        [Fact]
        public async Task RetryPolicy_ShouldRetryOnHttpRequestException()
        {
            // Arrange
            var policy = PolicyHelper.CreateRetryPolicy(_options);
            var attempts = 0;

            // Act
            try
            {
                await policy.ExecuteAsync(() =>
                {
                    attempts++;
                    throw new HttpRequestException("Test error");
                });
            }
            catch (HttpRequestException) { }

            // Assert
            attempts.Should().Be(_options.MaxRetryAttempts + 1);
        }

        [Fact]
        public async Task TimeoutPolicy_ShouldTimeoutAfterSpecifiedDuration()
        {
            // Arrange
            var policy = PolicyHelper.CreateTimeoutPolicy(_options);

            // Act & Assert
            await policy.Invoking(p => 
                p.ExecuteAsync(async () => 
                {
                    await Task.Delay(_options.Timeout + TimeSpan.FromSeconds(1));
                    return true;
                }))
                .Should().ThrowAsync<TimeoutRejectedException>();
        }

        [Fact]
        public async Task CircuitBreakerPolicy_ShouldOpenAfterConsecutiveFailures()
        {
            // Arrange
            var policy = PolicyHelper.CreateCircuitBreakerPolicy(_options);
            var attempts = 0;

            // Act & Assert
            for (var i = 0; i < 6; i++)  // More than exceptionsAllowedBeforeBreaking
            {
                try
                {
                    await policy.ExecuteAsync(() =>
                    {
                        attempts++;
                        throw new HttpRequestException("Test error");
                    });
                }
                catch (HttpRequestException) { }
                catch (BrokenCircuitException)
                {
                    // Circuit should be open after 5 failures
                    attempts.Should().Be(5);
                    return;
                }
            }

            Assert.Fail("Circuit breaker did not open");
        }
    }
}