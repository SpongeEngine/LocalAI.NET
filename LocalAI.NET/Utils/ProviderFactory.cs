using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Providers.KoboldCpp;
using LocalAI.NET.Providers.KoboldCpp.Models;
using LocalAI.NET.Providers.Ollama;
using LocalAI.NET.Providers.LmStudio;
using LocalAI.NET.Providers.TextGenerationWebUi;

namespace LocalAI.NET.Utils
{
    /// <summary>
    /// Factory for creating provider instances
    /// </summary>
    public static class ProviderFactory
    {
        /// <summary>
        /// Creates a provider instance based on the provided options
        /// </summary>
        public static ILocalAIProvider Create(LocalAIOptions options)
        {
            if (options.ProviderOptions == null)
            {
                throw new LocalAIException("Provider options must be specified");
            }

            return options.ProviderOptions switch
            {
                KoboldCppBaseOptions koboldOptions => KoboldCppProviderFactory.Create(options),
                OllamaOptions => new OllamaProvider(options),
                TextGenWebOptions => new TextGenerationWebUiProvider(options),
                LMStudioOptions => new LmStudioProvider(options),
                _ => throw new LocalAIException($"Unknown provider type: {options.ProviderOptions.GetType().Name}")
            };
        }

        /// <summary>
        /// Detects the provider type from the base URL (if possible)
        /// </summary>
        public static ProviderOptions? DetectProvider(string baseUrl)
        {
            // We'll implement auto-detection logic later
            return null;
        }
    }
}