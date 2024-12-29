using Xunit;
using FluentAssertions;
using LocalAI.NET.Models.Common;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Providers.Ollama;

namespace LocalAI.NET.Tests.Providers.Ollama
{
    public class OllamaProviderTests : IDisposable
    {
        private readonly WireMockServer _server;
        private readonly LocalAIOptions _options;
        private readonly OllamaProvider _provider;

        public OllamaProviderTests()
        {
            _server = WireMockServer.Start();
            
            _options = new LocalAIOptions
            {
                BaseUrl = _server.Urls[0],
                ProviderOptions = new OllamaOptions
                {
                    ConcurrentRequests = 1
                }
            };

            _provider = new OllamaProvider(_options);
        }

        [Fact]
        public async Task CompleteAsync_WhenSuccessful_ShouldReturnResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            _server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"choices\": [{{\"text\": \"{expectedResponse}\"}}]}}"));

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
                $"data: {{\"choices\": [{{\"text\": \"{token}\"}}]}}\n\n");

            _server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
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
                    .WithBody("{\"data\": [{\"id\": \"llama2\", \"created\": 1234567890, \"owned_by\": \"library\"}]}"));

            // Act
            var models = await _provider.GetAvailableModelsAsync();

            // Assert
            models.Should().ContainSingle()
                .Which.Should().Match<AIModel>(m =>
                    m.Id == "llama2" &&
                    m.Name == "llama2" &&
                    m.Provider == "Ollama" &&
                    m.Capabilities.SupportsStreaming &&
                    m.Metadata["owned_by"].ToString() == "library");
        }

        public void Dispose()
        {
            _provider.Dispose();
            _server.Dispose();
        }
    }
}