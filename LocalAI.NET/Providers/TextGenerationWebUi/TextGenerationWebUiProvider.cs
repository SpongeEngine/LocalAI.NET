using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using LocalAI.NET.Abstractions;
using LocalAI.NET.Models.Common;
using LocalAI.NET.Models.Configuration;
using LocalAI.NET.Models.Exceptions;
using LocalAI.NET.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = LocalAI.NET.Utils.JsonSerializer;

namespace LocalAI.NET.Providers.TextGenerationWebUi
{
    public class TextGenerationWebUiProvider : ILocalAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly TextGenWebOptions _options;
        private readonly JsonSerializerSettings? _jsonSettings;
        private bool _disposed;

        public string Name => "TextGenerationWebUI";
        public string? Version => null;
        public bool SupportsStreaming => true;

        public TextGenerationWebUiProvider(LocalAIOptions options)
        {
            if (options.ProviderOptions is not TextGenWebOptions textGenOptions)
            {
                throw new LocalAIException("Invalid provider options type");
            }

            _options = textGenOptions;
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
                var endpoint = _options.UseOpenAIEndpoint ? "v1/completions" : "api/v1/generate";
                
                var request = new
                {
                    prompt = prompt,
                    model = options?.ModelName,
                    temperature = options?.Temperature,
                    top_p = options?.TopP,
                    max_tokens = options?.MaxTokens,
                    stop = options?.StopSequences?.ToArray(),
                    stream = false
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LocalAIException(
                        "Text Generation WebUI API request failed",
                        Name,
                        (int)response.StatusCode,
                        responseContent);
                }

                var result = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonSettings);
                return _options.UseOpenAIEndpoint 
                    ? result?.choices?[0]?.text?.ToString() ?? string.Empty
                    : result?.results?[0]?.text?.ToString() ?? string.Empty;
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
            var endpoint = _options.UseOpenAIEndpoint ? "v1/completions" : "api/v1/stream";
            
            var request = new
            {
                prompt = prompt,
                model = options?.ModelName,
                temperature = options?.Temperature,
                top_p = options?.TopP,
                max_tokens = options?.MaxTokens,
                stop = options?.StopSequences?.ToArray(),
                stream = true
            };

            HttpResponseMessage? response = null;
            Stream? stream = null;
            StreamReader? reader = null;

            try
            {
                response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
                stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                reader = new StreamReader(stream);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new LocalAIException("Streaming failed", Name, null, ex.Message);
            }

            try
            {
                while (!reader!.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                        continue;

                    var json = line[6..];
                    if (json == "[DONE]")
                        break;

                    var chunk = JsonSerializer.Deserialize<dynamic>(json, _jsonSettings);
                    var token = _options.UseOpenAIEndpoint
                        ? chunk?.choices?[0]?.text?.ToString()
                        : chunk?.token?.text?.ToString();

                    if (!string.IsNullOrEmpty(token))
                        yield return token;
                }
            }
            finally
            {
                await (stream?.DisposeAsync() ?? ValueTask.CompletedTask);
                reader?.Dispose();
                response?.Dispose();
            }
        }

        public async Task<IReadOnlyList<AIModel>> GetAvailableModelsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var endpoint = _options.UseOpenAIEndpoint ? "v1/models" : "api/v1/model";
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
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

                if (_options.UseOpenAIEndpoint)
                {
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
                                SupportsSamplingParams = true,
                                SupportsStopSequences = true
                            }
                        });
                    }
                }
                else
                {
                    var model = result?.result?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(model))
                    {
                        models.Add(new AIModel
                        {
                            Id = model,
                            Name = model,
                            Provider = Name,
                            Capabilities = new ModelCapabilities
                            {
                                SupportsStreaming = true,
                                SupportsSamplingParams = true,
                                SupportsStopSequences = true
                            }
                        });
                    }
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
                var endpoint = _options.UseOpenAIEndpoint ? "v1/models" : "api/v1/model";
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                return response.IsSuccessStatusCode;
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