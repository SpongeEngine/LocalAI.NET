using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.Native.Models
{
    internal class KoboldCppNativeModelInfo
    {
        [JsonProperty("result")]
        public string ModelName { get; set; } = string.Empty;

        [JsonProperty("model_type")]
        public string ModelType { get; set; } = string.Empty;

        [JsonProperty("context_size")]
        public int ContextSize { get; set; }

        [JsonProperty("gpu_layers")]
        public int GpuLayers { get; set; }

        [JsonProperty("vocab_size")]
        public int VocabSize { get; set; }
    }
}