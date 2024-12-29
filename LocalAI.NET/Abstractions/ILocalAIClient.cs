using LocalAI.NET.Models.Common;
using LocalAI.NET.Models.Progress;

namespace LocalAI.NET.Abstractions
{
    /// <summary>
    /// Main interface for interacting with local AI models
    /// </summary>
    public interface ILocalAIClient : IDisposable
    {
        /// <summary>
        /// Event that provides progress updates during operations
        /// </summary>
        event Action<LocalAIProgress>? OnProgress;

        /// <summary>
        /// Completes the given prompt using the configured AI provider
        /// </summary>
        Task<string> CompleteAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams completion tokens as they are generated
        /// </summary>
        IAsyncEnumerable<string> StreamCompletionAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets available models from the provider
        /// </summary>
        Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the configured provider is available and responding
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}