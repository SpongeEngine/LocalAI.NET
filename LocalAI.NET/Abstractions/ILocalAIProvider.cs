using LocalAI.NET.Models.Common;

namespace LocalAI.NET.Abstractions
{
    /// <summary>
    /// Interface for specific AI provider implementations
    /// </summary>
    public interface ILocalAIProvider : IDisposable
    {
        /// <summary>
        /// Name of the provider (e.g., "KoboldCPP", "Ollama")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Provider version if available
        /// </summary>
        string? Version { get; }

        /// <summary>
        /// Whether the provider supports streaming responses
        /// </summary>
        bool SupportsStreaming { get; }

        /// <summary>
        /// Performs text completion
        /// </summary>
        Task<string> CompleteAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams completion tokens
        /// </summary>
        IAsyncEnumerable<string> StreamCompletionAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets available models
        /// </summary>
        Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks provider availability
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}