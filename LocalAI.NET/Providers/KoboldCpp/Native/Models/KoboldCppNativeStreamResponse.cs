using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.Native.Models 
{
    internal class KoboldCppNativeStreamResponse
    {
        [JsonProperty("token")]
        public KoboldCppNativeStreamToken? Token { get; set; }

        [JsonProperty("full_text")] 
        public string FullText { get; set; } = string.Empty;

        [JsonProperty("complete")]
        public bool IsComplete { get; set; }
    }

    internal class KoboldCppNativeStreamToken
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
}