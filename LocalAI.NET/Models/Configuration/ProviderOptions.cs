namespace LocalAI.NET.Models.Configuration
{
    /// <summary>
    /// Base class for provider-specific options
    /// </summary>
    public abstract class ProviderOptions
    {
        /// <summary>
        /// Gets the provider name this options class is for
        /// </summary>
        public abstract string ProviderName { get; }
    }

    /// <summary>
    /// Options specific to KoboldCPP provider
    /// </summary>
    public class KoboldCPPOptions : ProviderOptions
    {
        public override string ProviderName => "KoboldCpp";

        /// <summary>
        /// Context size in tokens
        /// </summary>
        public int ContextSize { get; set; } = 2048;

        /// <summary>
        /// Whether to use GPU acceleration
        /// </summary>
        public bool UseGpu { get; set; } = true;
    }

    /// <summary>
    /// Options specific to Ollama provider
    /// </summary>
    public class OllamaOptions : ProviderOptions
    {
        public override string ProviderName => "Ollama";

        /// <summary>
        /// Number of concurrent requests to allow
        /// </summary>
        public int ConcurrentRequests { get; set; } = 1;
    }

    /// <summary>
    /// Options for Text Generation WebUI provider
    /// </summary>
    public class TextGenWebOptions : ProviderOptions
    {
        public override string ProviderName => "TextGenerationWebUI";

        /// <summary>
        /// Whether to use the OpenAI-compatible API mode
        /// </summary>
        public bool UseOpenAIEndpoint { get; set; } = true;
    }

    /// <summary>
    /// Options for LM Studio provider
    /// </summary>
    public class LMStudioOptions : ProviderOptions
    {
        public override string ProviderName => "LMStudio";

        /// <summary>
        /// Whether to use the OpenAI-compatible API mode
        /// </summary>
        public bool UseOpenAIEndpoint { get; set; } = true;
    }
}