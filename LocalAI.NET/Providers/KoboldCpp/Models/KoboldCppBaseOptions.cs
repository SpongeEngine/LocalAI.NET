using LocalAI.NET.Models.Configuration;

namespace LocalAI.NET.Providers.KoboldCpp.Models
{
    /// <summary>
    /// Base options shared between Native and OpenAI implementations
    /// </summary>
    public abstract class KoboldCppBaseOptions : ProviderOptions
    {
        public override string ProviderName => "KoboldCpp";

        /// <summary>
        /// Maximum context size in tokens
        /// </summary> 
        public int ContextSize { get; set; } = 2048;

        /// <summary>
        /// Whether to use GPU acceleration
        /// </summary>
        public bool UseGpu { get; set; } = true;
        
        /// <summary>
        /// Whether to use the OpenAI-compatible API endpoints
        /// </summary>
        public bool UseOpenAiApi { get; set; }
    }
}