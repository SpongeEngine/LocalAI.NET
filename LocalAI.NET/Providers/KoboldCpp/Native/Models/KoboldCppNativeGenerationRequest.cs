// Native/Models/KoboldCppGenerationRequest.cs
using Newtonsoft.Json;

namespace LocalAI.NET.Providers.KoboldCpp.Native.Models
{
    internal class KoboldCppNativeGenerationRequest
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonProperty("max_length")]
        public int? MaxLength { get; set; }

        [JsonProperty("max_context_length")]
        public int? MaxContextLength { get; set; }

        [JsonProperty("temperature")]
        public float? Temperature { get; set; }

        [JsonProperty("top_p")]
        public float? TopP { get; set; }

        [JsonProperty("top_k")]
        public int? TopK { get; set; }

        [JsonProperty("top_a")]
        public float? TopA { get; set; }

        [JsonProperty("typical")]
        public float? Typical { get; set; }

        [JsonProperty("tfs")]
        public float? Tfs { get; set; }

        [JsonProperty("rep_pen")]
        public float? RepetitionPenalty { get; set; }

        [JsonProperty("rep_pen_range")]
        public int? RepetitionPenaltyRange { get; set; }

        [JsonProperty("mirostat")]
        public int? MirostatMode { get; set; }

        [JsonProperty("mirostat_tau")]
        public float? MirostatTau { get; set; }

        [JsonProperty("mirostat_eta")]
        public float? MirostatEta { get; set; }

        [JsonProperty("stop_sequence")]
        public List<string>? StopSequences { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; }

        [JsonProperty("genkey")]
        public string GenKey { get; set; } = string.Empty;

        [JsonProperty("trim_stop")]
        public bool TrimStop { get; set; } = true;

        [JsonProperty("render_special")]
        public bool RenderSpecial { get; set; }

        [JsonProperty("bypass_eos")]
        public bool BypassEos { get; set; }

        [JsonProperty("grammar")]
        public string? Grammar { get; set; }

        [JsonProperty("grammar_retain_state")]
        public bool GrammarRetainState { get; set; }

        [JsonProperty("memory")]
        public string? Memory { get; set; }

        [JsonProperty("banned_tokens")]
        public List<string>? BannedTokens { get; set; }

        [JsonProperty("logit_bias")] 
        public Dictionary<string, float>? LogitBias { get; set; }
    }
}