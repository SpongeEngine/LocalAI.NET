using Xunit;
using FluentAssertions;
using LocalAI.NET.Models.Common;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Providers.LmStudio;

namespace LocalAI.NET.Tests.Providers.LmStudio
{
    public class LmStudioProviderTests : IDisposable
    {
        private readonly WireMockServer _server;
        private readonly LocalAIOptions _options;
        private readonly LmStudioProvider _provider;

        public LmStudioProviderTests()
        {
            _server = WireMockServer.Start();
            
            _options = new LocalAIOptions
            {
                BaseUrl = _server.Urls[0],
                ProviderOptions = new LMStudioOptions
                {
                    UseOpenAIEndpoint = true
                }
            };

            _provider = new LmStudioProvider(_options);
        }

        [Fact]
        public async Task CompleteAsync_WhenSuccessful_ShouldReturnResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            _server
                .Given(Request.Create()
                    .WithPath("/v1/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"choices\": [{{\"message\": {{\"content\": \"{expectedResponse}\"}}}}]}}"));

            // Act
            var result = await _provider.CompleteAsync("Test prompt");

            // Assert
            result.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task StreamCompletionAsync_WhenSuccessful_ShouldStreamTokens()
        {
            // Arrange
            var tokens = new[] { "Hello", " world", "!" };
            var streamResponses = tokens.Select((token, i) => 
                $"data: {{\"choices\": [{{\"delta\": {{\"content\": \"{token}\"}}}}]}}\n\n");

            _server
                .Given(Request.Create()
                    .WithPath("/v1/chat/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses) + "data: [DONE]\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _provider.StreamCompletionAsync("Test prompt"))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_WhenSuccessful_ShouldReturnModels()
        {
            // Arrange
            _server
                .Given(Request.Create()
                    .WithPath("/v1/models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"data\": [{\"id\": \"local-model\"}]}"));

            // Act
            var models = await _provider.GetAvailableModelsAsync();

            // Assert
            models.Should().ContainSingle()
                .Which.Should().Match<AIModel>(m =>
                    m.Id == "local-model" &&
                    m.Name == "local-model" &&
                    m.Provider == "LMStudio" &&
                    m.Capabilities.SupportsStreaming);
        }

        public void Dispose()
        {
            _provider.Dispose();
            _server.Dispose();
        }
    }
}