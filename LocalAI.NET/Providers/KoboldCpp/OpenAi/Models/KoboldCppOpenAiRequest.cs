using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.OpenAi.Models
{
    internal class KoboldCppOpenAiRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonProperty("temperature")]
        public float? Temperature { get; set; }

        [JsonProperty("top_p")]
        public float? TopP { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; }

        [JsonProperty("stop")]
        public List<string>? Stop { get; set; }

        [JsonProperty("presence_penalty")]
        public float? PresencePenalty { get; set; }

        [JsonProperty("frequency_penalty")]
        public float? FrequencyPenalty { get; set; }

        [JsonProperty("logit_bias")]
        public Dictionary<string, float>? LogitBias { get; set; }
    }
}