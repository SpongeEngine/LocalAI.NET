namespace LocalAI.NET.Models.Common
{
    /// <summary>
    /// Represents an AI model available through a provider
    /// </summary>
    public class AIModel
    {
        /// <summary>
        /// Unique identifier for the model
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// Display name of the model
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Model provider name
        /// </summary>
        public string Provider { get; init; } = string.Empty;

        /// <summary>
        /// Model capabilities and configuration
        /// </summary>
        public ModelCapabilities Capabilities { get; init; } = new();

        /// <summary>
        /// Provider-specific model metadata
        /// </summary>
        public IDictionary<string, object> Metadata { get; init; }
            = new Dictionary<string, object>();
    }
}