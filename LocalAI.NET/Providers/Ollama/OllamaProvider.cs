using System.Runtime.CompilerServices;
using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Common;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models;

namespace LocalAI.NET.Providers.Ollama
{
    public class OllamaProvider : ILocalAIProvider
    {
        private readonly IOllamaApiClient _client;
        private readonly ILogger? _logger;
        private readonly OllamaOptions _options;
        private bool _disposed;

        public string Name => "Ollama";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public OllamaProvider(LocalAIOptions options)
        {
            if (options.ProviderOptions is not OllamaOptions ollamaOptions)
            {
                throw new LocalAIException("Invalid provider options type");
            }

            _options = ollamaOptions;
            _logger = options.Logger;
            
            var uri = new Uri(options.BaseUrl);
            _client = new OllamaApiClient(uri);
        }

        public async Task<string> CompleteAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GenerateRequest
                {
                    Model = options?.ModelName ?? "default",
                    Prompt = prompt,
                    Stream = false,
                    Options = new RequestOptions
                    {
                        Temperature = options?.Temperature ?? 0.7f,
                        TopP = options?.TopP ?? 0.9f,
                        NumPredict = options?.MaxTokens ?? -1,
                        Stop = options?.StopSequences?.ToArray()
                    }
                };

                var responseBuilder = new System.Text.StringBuilder();
                await foreach (var response in _client.GenerateAsync(request, cancellationToken))
                {
                    if (response?.Response != null)
                    {
                        responseBuilder.Append(response.Response);
                    }
                }

                return responseBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new LocalAIException("Failed to complete prompt", Name, null, ex.Message);
            }
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(
            string prompt,
            CompletionOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = new GenerateRequest
            {
                Model = options?.ModelName ?? "default",
                Prompt = prompt,
                Stream = true,
                Options = new RequestOptions
                {
                    Temperature = options?.Temperature ?? 0.7f,
                    TopP = options?.TopP ?? 0.9f,
                    NumPredict = options?.MaxTokens ?? -1,
                    Stop = options?.StopSequences?.ToArray()
                }
            };

            await foreach (var response in _client.GenerateAsync(request, cancellationToken))
            {
                if (!string.IsNullOrEmpty(response?.Response))
                {
                    yield return response.Response;
                }
            }
        }

        public async Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var models = await _client.ListLocalModelsAsync(cancellationToken);
                return models.Select(m => new AIModel
                {
                    Id = m.Name,
                    Name = m.Name,
                    Provider = Name,
                    Capabilities = new ModelCapabilities
                    {
                        SupportsStreaming = true,
                        SupportsSamplingParams = true,
                        SupportsStopSequences = true
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["size"] = m.Size,
                        ["digest"] = m.Digest,
                        ["modified_at"] = m.ModifiedAt
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new LocalAIException("Failed to get models", Name, null, ex.Message);
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _client.IsRunningAsync(cancellationToken))
                {
                    var version = await _client.GetVersionAsync(cancellationToken);
                    Version = version.ToString();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    (_client as IDisposable)?.Dispose();
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