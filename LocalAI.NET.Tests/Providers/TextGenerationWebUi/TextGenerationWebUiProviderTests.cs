using Xunit;
using FluentAssertions;
using LocalAI.NET.Models.Common;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Providers.TextGenerationWebUi;

namespace LocalAI.NET.Tests.Providers.TextGenerationWebUi
{
    public class TextGenerationWebUiProviderTests : IDisposable
    {
        private readonly WireMockServer _server;
        private readonly LocalAIOptions _options;
        private readonly TextGenerationWebUiProvider _provider;

        public TextGenerationWebUiProviderTests()
        {
            _server = WireMockServer.Start();
            
            _options = new LocalAIOptions
            {
                BaseUrl = _server.Urls[0],
                ProviderOptions = new TextGenWebOptions
                {
                    UseOpenAIEndpoint = true
                }
            };

            _provider = new TextGenerationWebUiProvider(_options);
        }

        [Fact]
        public async Task CompleteAsync_WithOpenAiMode_ShouldReturnResponse()
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
        public async Task CompleteAsync_WithNativeMode_ShouldReturnResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            _options.ProviderOptions = new TextGenWebOptions { UseOpenAIEndpoint = false };
            
            _server
                .Given(Request.Create()
                    .WithPath("/api/v1/generate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"results\": [{{\"text\": \"{expectedResponse}\"}}]}}"));

            // Act
            var result = await _provider.CompleteAsync("Test prompt");

            // Assert
            result.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task StreamCompletionAsync_WithOpenAiMode_ShouldStreamTokens()
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
        public async Task StreamCompletionAsync_WithNativeMode_ShouldStreamTokens()
        {
            // Arrange
            _options.ProviderOptions = new TextGenWebOptions { UseOpenAIEndpoint = false };
            var tokens = new[] { "Hello", " world", "!" };
            var streamResponses = tokens.Select((token, i) => 
                $"data: {{\"token\": {{\"text\": \"{token}\"}}}}\n\n");

            _server
                .Given(Request.Create()
                    .WithPath("/api/v1/stream")
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
        public async Task GetAvailableModelsAsync_WithOpenAiMode_ShouldReturnModels()
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
                    m.Provider == "TextGenerationWebUI" &&
                    m.Capabilities.SupportsStreaming);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_WithNativeMode_ShouldReturnModels()
        {
            // Arrange
            _options.ProviderOptions = new TextGenWebOptions { UseOpenAIEndpoint = false };
            
            _server
                .Given(Request.Create()
                    .WithPath("/api/v1/model")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"result\": \"local-model\"}"));

            // Act
            var models = await _provider.GetAvailableModelsAsync();

            // Assert
            models.Should().ContainSingle()
                .Which.Should().Match<AIModel>(m =>
                    m.Id == "local-model" &&
                    m.Name == "local-model" &&
                    m.Provider == "TextGenerationWebUI" &&
                    m.Capabilities.SupportsStreaming);
        }

        public void Dispose()
        {
            _provider.Dispose();
            _server.Dispose();
        }
    }
}