using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.OpenAi.Models
{
    internal class KoboldCppOpenAiChatStreamResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("object")]
        public string Object { get; set; } = "chat.completion.chunk";

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("choices")]
        public List<OpenAiChatStreamChoice> Choices { get; set; } = new();
    }
    
    internal class OpenAiChatStreamChoice
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("delta")]
        public OpenAiChatMessage Delta { get; set; } = new();

        [JsonProperty("finish_reason")]
        public string? FinishReason { get; set; }
    }
    
    internal class OpenAiChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; } = "assistant";

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }
}