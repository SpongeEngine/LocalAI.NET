using Xunit;
using FluentAssertions;
using LocalAI.NET.Models.Common;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Providers.KoboldCpp.Native;

namespace LocalAI.NET.Tests.Providers.KoboldCpp
{
    public class KoboldCppNativeProviderTests : IDisposable
    {
        private readonly WireMockServer _server;
        private readonly LocalAIOptions _options;
        private readonly KoboldCppNativeProvider _provider;

        public KoboldCppNativeProviderTests()
        {
            _server = WireMockServer.Start();
            
            _options = new LocalAIOptions
            {
                BaseUrl = _server.Urls[0],
                ProviderOptions = new KoboldCppNativeOptions
                {
                    ContextSize = 2048,
                    UseGpu = true,
                    RepetitionPenalty = 1.1f,
                    RepetitionPenaltyRange = 320,
                    TrimStop = true
                }
            };

            _provider = new KoboldCppNativeProvider(_options);
        }

        [Fact]
        public async Task CompleteAsync_WhenSuccessful_ShouldReturnResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            _server
                .Given(Request.Create()
                    .WithPath("/api/v1/generate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"results\": [{{\"text\": \"{expectedResponse}\", \"tokens\": 3}}]}}"));

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
                $"data: {{\"token\": {{\"text\": \"{token}\"}}, \"complete\": {(i == tokens.Length - 1).ToString().ToLower()}}}\n\n");

            _server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/stream")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses))
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
        public async Task GetAvailableModelsAsync_WhenSuccessful_ShouldReturnModel()
        {
            // Arrange
            const string modelName = "test-model";
            _server
                .Given(Request.Create()
                    .WithPath("/api/v1/model")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"result\": \"{modelName}\", \"model_type\": \"llama\", \"context_size\": 2048, \"gpu_layers\": 32, \"vocab_size\": 32000}}"));

            // Act
            var models = await _provider.GetAvailableModelsAsync();

            // Assert
            models.Should().ContainSingle()
                .Which.Should().Match<AIModel>(m =>
                    m.Id == modelName &&
                    m.Name == modelName &&
                    m.Provider == "KoboldCpp" &&
                    m.Capabilities.MaxContextLength == 2048);
        }

        public void Dispose()
        {
            _provider.Dispose();
            _server.Dispose();
        }
    }
}