using Xunit;
using FluentAssertions;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace LocalAI.NET.Tests.Utils
{
    public class HttpClientFactoryTests
    {
        [Fact]
        public void Create_ShouldConfigureHttpClientCorrectly()
        {
            // Arrange
            var options = new LocalAIOptions
            {
                BaseUrl = "http://localhost:5000",
                ApiKey = "test-key",
                Timeout = TimeSpan.FromSeconds(30),
                Logger = new Mock<ILogger>().Object
            };

            // Act
            using var client = HttpClientFactory.Create(options, "TestProvider");

            // Assert
            client.BaseAddress.Should().Be(new Uri(options.BaseUrl));
            client.Timeout.Should().Be(options.Timeout);
            client.DefaultRequestHeaders.Authorization?.Scheme.Should().Be("Bearer");
            client.DefaultRequestHeaders.Authorization?.Parameter.Should().Be(options.ApiKey);
            client.DefaultRequestHeaders.Accept
                .Should().Contain(h => h.MediaType == "application/json");
            client.DefaultRequestHeaders.UserAgent
                .Should().Contain(h => h.Product != null && h.Product.Name == "LocalAI.NET");
        }
    }
}