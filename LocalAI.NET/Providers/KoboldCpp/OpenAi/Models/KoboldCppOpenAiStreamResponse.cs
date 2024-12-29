using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.OpenAi.Models
{
    internal class KoboldCppOpenAiStreamResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("object")]
        public string Object { get; set; } = "text_completion";

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("model")] 
        public string Model { get; set; } = string.Empty;

        [JsonProperty("choices")]
        public List<OpenAiStreamChoice> Choices { get; set; } = new();
    }

    internal class OpenAiStreamChoice
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; }
    }
}