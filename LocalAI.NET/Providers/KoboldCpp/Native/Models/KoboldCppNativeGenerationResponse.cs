using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.Native.Models
{
    internal class KoboldCppNativeGenerationResponse
    {
        [JsonProperty("results")]
        public List<KoboldCppNativeGenerationResult> Results { get; set; } = new();
    }

    internal class KoboldCppNativeGenerationResult
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("tokens")]
        public int Tokens { get; set; }
    }
}