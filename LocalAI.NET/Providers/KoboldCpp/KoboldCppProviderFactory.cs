using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Providers.KoboldCpp.Native;
using LocalAI.NET.Providers.KoboldCpp.OpenAi;

namespace LocalAI.NET.Providers.KoboldCpp
{
    public static class KoboldCppProviderFactory 
    {
        public static ILocalAIProvider Create(LocalAIOptions options)
        {
            // First validate we have provider options
            if (options.ProviderOptions == null)
            {
                throw new LocalAIException("Provider options must be specified");
            }

            // Then handle each provider type
            return options.ProviderOptions switch
            {
                KoboldCppNativeOptions nativeOptions => new KoboldCppNativeProvider(options),
                KoboldCppOpenAiOptions openAiOptions => new KoboldCppOpenAiProvider(options),
                _ => throw new LocalAIException($"Invalid provider options type: {options.ProviderOptions.GetType().Name}")
            };
        }
    }
}