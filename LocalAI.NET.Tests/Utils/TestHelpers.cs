using LocalAI.NET.Client;
using LocalAI.NET.Models.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace LocalAI.NET.Tests.Utils
{
    public static class TestHelpers
    {
        public static ILogger<T> CreateLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }

        public static LocalAIOptions CreateOptions(ProviderOptions providerOptions)
        {
            return new LocalAIOptions
            {
                BaseUrl = "http://localhost:5000",
                Logger = CreateLogger<LocalAIClient>(),
                ProviderOptions = providerOptions
            };
        }

        public static string CreateStreamResponse(string token, bool isComplete = false)
        {
            return $"data: {{\"token\": \"{token}\", \"complete\": {isComplete.ToString().ToLower()}}}\n\n";
        }
    }
}