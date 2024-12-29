// Client/LocalAIClient.cs

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Polly;
using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Common;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Progress;
using LocalAI.NET.Utils;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace LocalAI.NET.Client
{
    public class LocalAIClient : ILocalAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly LocalAIOptions _options;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncTimeoutPolicy _timeoutPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ILocalAIProvider _provider;
        private bool _disposed;

        public event Action<LocalAIProgress>? OnProgress;

        public LocalAIClient(LocalAIOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = options.Logger;

            // Create policies
            _retryPolicy = PolicyHelper.CreateRetryPolicy(options);
            _timeoutPolicy = PolicyHelper.CreateTimeoutPolicy(options);
            _circuitBreakerPolicy = PolicyHelper.CreateCircuitBreakerPolicy(options);

            // Create HTTP client
            _httpClient = HttpClientFactory.Create(options);

            // Create provider (we'll implement provider factory later)
            _provider = CreateProvider(options);
        }

        public async Task<string> CompleteAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ReportProgress(LocalAIProgressState.Starting, "Starting completion...");

                var result = await Policy.WrapAsync(
                    _retryPolicy,
                    _timeoutPolicy,
                    _circuitBreakerPolicy)
                    .ExecuteAsync(async () => 
                        await _provider.CompleteAsync(prompt, options, cancellationToken));

                ReportProgress(LocalAIProgressState.Complete, "Completion finished");
                return result;
            }
            catch (Exception ex)
            {
                ReportProgress(LocalAIProgressState.Failed, $"Completion failed: {ex.Message}");
                throw;
            }
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(
            string prompt,
            CompletionOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ReportProgress(LocalAIProgressState.Starting, "Starting streaming completion...");
    
            await using var enumerator = _provider.StreamCompletionAsync(
                prompt, options, cancellationToken).GetAsyncEnumerator(cancellationToken);
        
            bool hasMore;
            do
            {
                string? current = null;
                try
                {
                    hasMore = await enumerator.MoveNextAsync();
                    if (hasMore)
                    {
                        current = enumerator.Current;
                        ReportProgress(LocalAIProgressState.Streaming, "Receiving tokens...");
                    }
                    else
                    {
                        ReportProgress(LocalAIProgressState.Complete, "Streaming complete");
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(LocalAIProgressState.Failed, $"Streaming failed: {ex.Message}");
                    throw;
                }

                if (hasMore && current != null)
                {
                    yield return current;
                }
            } while (hasMore);
        }

        public async Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default)
        {
            return await Policy.WrapAsync(
                _retryPolicy,
                _timeoutPolicy,
                _circuitBreakerPolicy)
                .ExecuteAsync(async () => 
                    await _provider.GetAvailableModelsAsync(cancellationToken));
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _provider.IsAvailableAsync(cancellationToken);
            }
            catch
            {
                return false;
            }
        }

        private void ReportProgress(LocalAIProgressState state, string message)
        {
            OnProgress?.Invoke(new LocalAIProgress
            {
                State = state,
                Message = message,
                ElapsedTime = DateTime.UtcNow - DateTime.UtcNow // We'll implement proper timing later
            });
        }

        private ILocalAIProvider CreateProvider(LocalAIOptions options)
        {
            // We'll implement provider factory pattern later
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}