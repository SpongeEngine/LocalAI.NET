namespace LocalAI.NET.Models.Common
{
    /// <summary>
    /// Represents the capabilities of an AI model
    /// </summary>
    public class ModelCapabilities
    {
        /// <summary>
        /// Whether the model supports streaming responses
        /// </summary>
        public bool SupportsStreaming { get; init; }

        /// <summary>
        /// Maximum context length in tokens
        /// </summary>
        public int MaxContextLength { get; init; }

        /// <summary>
        /// Maximum number of tokens that can be generated
        /// </summary>
        public int MaxGenerationLength { get; init; }

        /// <summary>
        /// Whether the model supports custom sampling parameters
        /// </summary>
        public bool SupportsSamplingParams { get; init; } = true;

        /// <summary>
        /// Whether the model supports stop sequences
        /// </summary>
        public bool SupportsStopSequences { get; init; } = true;

        /// <summary>
        /// Model-specific capabilities
        /// </summary>
        public IDictionary<string, object> ModelSpecificCapabilities { get; init; }
            = new Dictionary<string, object>();
    }
}