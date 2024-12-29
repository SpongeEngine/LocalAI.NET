namespace LocalAI.NET.Models.Common
{
    /// <summary>
    /// Options for controlling text completion
    /// </summary>
    public class CompletionOptions
    {
        /// <summary>
        /// Model to use for completion
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Maximum number of tokens to generate
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Temperature for controlling randomness (0.0-1.0)
        /// </summary>
        public float? Temperature { get; set; }

        /// <summary>
        /// Top-p sampling parameter (0.0-1.0)
        /// </summary>
        public float? TopP { get; set; }

        /// <summary>
        /// Stop sequences that will halt generation
        /// </summary>
        public IList<string> StopSequences { get; set; } = new List<string>();

        /// <summary>
        /// Provider-specific parameters
        /// </summary>
        public IDictionary<string, object> ProviderParameters { get; set; }
            = new Dictionary<string, object>();
    }
}