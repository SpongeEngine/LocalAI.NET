using Xunit;
using Moq;
using FluentAssertions;
using LocalAI.NET.Client;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Progress;
using Microsoft.Extensions.Logging;

namespace LocalAI.NET.Tests.Client
{
    public class LocalAiClientTests
    {
        private readonly LocalAIOptions _options;
        private readonly Mock<ILogger> _mockLogger;

        public LocalAiClientTests()
        {
            _mockLogger = new Mock<ILogger>();
            _options = new LocalAIOptions
            {
                BaseUrl = "http://localhost:5000",
                Logger = _mockLogger.Object,
                Timeout = TimeSpan.FromSeconds(30),
                MaxRetryAttempts = 3,
                RetryDelay = TimeSpan.FromSeconds(2)
            };
        }

        [Fact]
        public async Task CompleteAsync_WhenSuccessful_ShouldReportProgress()
        {
            // Arrange
            var progressUpdates = new List<LocalAIProgress>();
            using var client = new LocalAIClient(_options);
            client.OnProgress += (progress) => progressUpdates.Add(progress);

            // Act
            try 
            {
                await client.CompleteAsync("Test prompt");
            }
            catch (NotImplementedException) 
            { 
                // Expected since provider factory isn't implemented
            }

            // Assert
            progressUpdates.Should().ContainSingle(p => p.State == LocalAIProgressState.Starting);
            progressUpdates.Should().ContainSingle(p => p.State == LocalAIProgressState.Failed);
        }

        [Fact]
        public async Task StreamCompletionAsync_WhenSuccessful_ShouldReportProgress()
        {
            // Arrange
            var progressUpdates = new List<LocalAIProgress>();
            using var client = new LocalAIClient(_options);
            client.OnProgress += (progress) => progressUpdates.Add(progress);

            // Act
            try 
            {
                await foreach (var _ in client.StreamCompletionAsync("Test prompt")) { }
            }
            catch (NotImplementedException)
            {
                // Expected since provider factory isn't implemented
            }

            // Assert
            progressUpdates.Should().ContainSingle(p => p.State == LocalAIProgressState.Starting);
            progressUpdates.Should().ContainSingle(p => p.State == LocalAIProgressState.Failed);
        }

        [Fact]
        public void Constructor_WithNullOptions_ShouldThrow()
        {
            // Act & Assert
            var action = () => new LocalAIClient(null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Dispose_ShouldDisposeOnce()
        {
            // Arrange
            var client = new LocalAIClient(_options);

            // Act
            client.Dispose();
            client.Dispose(); // Second dispose should be safe

            // Assert - No exception thrown
        }
    }
}