using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Common;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Providers.KoboldCpp.Native.Models;
using LocalAI.NET.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = LocalAI.NET.Utils.JsonSerializer;

namespace LocalAI.NET.Providers.KoboldCpp.Native
{
    public class KoboldCppNativeProvider : ILocalAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly KoboldCppNativeOptions _options;
        private readonly JsonSerializerSettings? _jsonSettings;
        private bool _disposed;

        public string Name => "KoboldCpp";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public KoboldCppNativeProvider(LocalAIOptions options)
        {
            if (options.ProviderOptions is not KoboldCppNativeOptions koboldOptions)
            {
                throw new LocalAIException("Invalid provider options type");
            }

            _options = koboldOptions;
            _logger = options.Logger;
            _jsonSettings = options.JsonSettings;
            _httpClient = HttpClientFactory.Create(options);
        }

        public async Task<string> CompleteAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new KoboldCppNativeGenerationRequest
                {
                    Prompt = prompt,
                    MaxLength = options?.MaxTokens ?? 80,
                    MaxContextLength = _options.ContextSize,
                    Temperature = options?.Temperature ?? 0.7f,
                    TopP = options?.TopP ?? 0.9f,
                    StopSequences = options?.StopSequences?.ToList(),
                    TrimStop = _options.TrimStop,
                    RepetitionPenalty = _options.RepetitionPenalty,
                    RepetitionPenaltyRange = _options.RepetitionPenaltyRange,
                    Stream = false
                };

                if (_options.Mirostat != null)
                {
                    request.MirostatMode = _options.Mirostat.Mode;
                    request.MirostatTau = _options.Mirostat.Tau;
                    request.MirostatEta = _options.Mirostat.Eta;
                }

                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/v1/generate", content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LocalAIException(
                        "KoboldCpp API request failed",
                        Name,
                        (int)response.StatusCode,
                        responseContent);
                }

                var result = JsonSerializer.Deserialize<KoboldCppNativeGenerationResponse>(
                    responseContent, _jsonSettings);
            
                return result?.Results.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex) when (ex is not LocalAIException)
            {
                throw new LocalAIException("Failed to complete prompt", Name, null, ex.Message);
            }
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(
            string prompt,
            CompletionOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = new KoboldCppNativeGenerationRequest
            {
                Prompt = prompt,
                MaxLength = options?.MaxTokens ?? 80,
                MaxContextLength = _options.ContextSize,
                Temperature = options?.Temperature ?? 0.7f,
                TopP = options?.TopP ?? 0.9f,
                StopSequences = options?.StopSequences?.ToList(),
                TrimStop = _options.TrimStop,
                RepetitionPenalty = _options.RepetitionPenalty,
                RepetitionPenaltyRange = _options.RepetitionPenaltyRange,
                Stream = true
            };

            if (_options.Mirostat != null)
            {
                request.MirostatMode = _options.Mirostat.Mode;
                request.MirostatTau = _options.Mirostat.Tau;
                request.MirostatEta = _options.Mirostat.Eta;
            }

            var response = await _httpClient.PostAsJsonAsync(
                "api/extra/generate/stream", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new LocalAIException(
                    "Streaming request failed",
                    Name,
                    (int)response.StatusCode,
                    await response.Content.ReadAsStringAsync(cancellationToken));
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                    continue;

                var json = line[6..];
                if (json == "[DONE]")
                    break;

                var chunk = JsonSerializer.Deserialize<KoboldCppNativeStreamResponse>(
                    json, _jsonSettings);

                if (!string.IsNullOrEmpty(chunk?.Token?.Text))
                    yield return chunk.Token.Text;
            }
        }

        public async Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get model info
                var response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LocalAIException(
                        "Failed to get model info",
                        Name,
                        (int)response.StatusCode,
                        content);
                }

                var result = JsonSerializer.Deserialize<KoboldCppNativeModelInfo>(content, _jsonSettings);
                
                if (string.IsNullOrEmpty(result?.ModelName))
                    return Array.Empty<AIModel>();

                // Create AIModel instance
                var model = new AIModel
                {
                    Id = result.ModelName,
                    Name = result.ModelName,
                    Provider = Name,
                    Capabilities = new ModelCapabilities
                    {
                        SupportsStreaming = true,
                        MaxContextLength = result.ContextSize,
                        MaxGenerationLength = 2048, // Default max generation length
                        SupportsSamplingParams = true,
                        SupportsStopSequences = true
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["model_type"] = result.ModelType,
                        ["gpu_layers"] = result.GpuLayers,
                        ["vocab_size"] = result.VocabSize
                    }
                };

                return new[] { model };
            }
            catch (Exception ex) when (ex is not LocalAIException)
            {
                throw new LocalAIException("Failed to get models", Name, null, ex.Message);
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    // Get version info
                    var versionResponse = await _httpClient.GetAsync("api/extra/version", cancellationToken);
                    if (versionResponse.IsSuccessStatusCode)
                    {
                        var content = await versionResponse.Content.ReadAsStringAsync(cancellationToken);
                        var versionInfo = JsonSerializer.Deserialize<dynamic>(content, _jsonSettings);
                        Version = versionInfo?.version?.ToString();
                    }
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