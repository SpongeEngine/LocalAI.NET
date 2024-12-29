using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Common;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Providers.KoboldCpp.OpenAi.Models;
using LocalAI.NET.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = LocalAI.NET.Utils.JsonSerializer;

namespace LocalAI.NET.Providers.KoboldCpp.OpenAi
{
    public class KoboldCppOpenAiProvider : ILocalAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly KoboldCppOpenAiOptions _options;
        private readonly JsonSerializerSettings? _jsonSettings;
        private bool _disposed;

        public string Name => "KoboldCpp";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public KoboldCppOpenAiProvider(LocalAIOptions options)
        {
            if (options.ProviderOptions is not KoboldCppOpenAiOptions koboldOptions)
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
                var endpoint = _options.UseChatCompletions 
                    ? "v1/chat/completions"
                    : "v1/completions";
                
                var request = _options.UseChatCompletions 
                    ? CreateChatRequest(prompt, options)
                    : CreateCompletionRequest(prompt, options);

                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LocalAIException(
                        "KoboldCpp OpenAI API request failed",
                        Name,
                        (int)response.StatusCode,
                        responseContent);
                }

                if (_options.UseChatCompletions)
                {
                    var chatResult = JsonSerializer.Deserialize<KoboldCppOpenAiChatResponse>(
                        responseContent, _jsonSettings);
                    return chatResult?.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;
                }
                else
                {
                    var result = JsonSerializer.Deserialize<KoboldCppOpenAiResponse>(
                        responseContent, _jsonSettings);
                    return result?.Choices.FirstOrDefault()?.Text ?? string.Empty;
                }
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
            var endpoint = _options.UseChatCompletions 
                ? "v1/chat/completions" 
                : "v1/completions";

            var request = _options.UseChatCompletions
                ? CreateChatRequest(prompt, options, true)
                : CreateCompletionRequest(prompt, options, true);

            var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);

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

                if (_options.UseChatCompletions)
                {
                    var chunk = JsonSerializer.Deserialize<KoboldCppOpenAiChatStreamResponse>(
                        json, _jsonSettings);
                    var content = chunk?.Choices.FirstOrDefault()?.Delta?.Content;
                    if (!string.IsNullOrEmpty(content))
                        yield return content;
                }
                else
                {
                    var chunk = JsonSerializer.Deserialize<KoboldCppOpenAiStreamResponse>(
                        json, _jsonSettings);
                    var text = chunk?.Choices.FirstOrDefault()?.Text;
                    if (!string.IsNullOrEmpty(text))
                        yield return text;
                }
            }
        }

        public async Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("v1/models", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LocalAIException(
                        "Failed to get models",
                        Name,
                        (int)response.StatusCode,
                        content);
                }

                var result = JsonSerializer.Deserialize<dynamic>(content, _jsonSettings);
                var models = new List<AIModel>();

                foreach (var model in result?.data ?? Array.Empty<dynamic>())
                {
                    models.Add(new AIModel
                    {
                        Id = model?.id?.ToString() ?? "",
                        Name = model?.id?.ToString() ?? "",
                        Provider = Name,
                        Capabilities = new ModelCapabilities
                        {
                            SupportsStreaming = true,
                            MaxContextLength = _options.ContextSize,
                            MaxGenerationLength = 2048,  // Default max generation length
                            SupportsSamplingParams = true,
                            SupportsStopSequences = true
                        }
                    });
                }

                return models;
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
                var response = await _httpClient.GetAsync("v1/models", cancellationToken);
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

        private object CreateCompletionRequest(string prompt, CompletionOptions? options, bool stream = false)
        {
            return new
            {
                model = options?.ModelName ?? _options.ModelName,
                prompt = prompt,
                max_tokens = options?.MaxTokens ?? 80,
                temperature = options?.Temperature ?? 0.7f,
                top_p = options?.TopP ?? 0.9f,
                stop = options?.StopSequences?.ToArray(),
                stream = stream
            };
        }

        private object CreateChatRequest(string prompt, CompletionOptions? options, bool stream = false)
        {
            return new
            {
                model = options?.ModelName ?? _options.ModelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = options?.MaxTokens ?? 80,
                temperature = options?.Temperature ?? 0.7f,
                top_p = options?.TopP ?? 0.9f,
                stop = options?.StopSequences?.ToArray(),
                stream = stream
            };
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