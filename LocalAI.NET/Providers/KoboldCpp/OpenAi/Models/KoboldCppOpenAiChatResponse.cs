using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.OpenAi.Models
{
    internal class KoboldCppOpenAiChatResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("object")]
        public string Object { get; set; } = "chat.completion";

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("choices")]
        public List<OpenAiChatChoice> Choices { get; set; } = new();
    }
    
    internal class OpenAiChatChoice
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public OpenAiChatMessage Message { get; set; } = new();

        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; }
    }
}