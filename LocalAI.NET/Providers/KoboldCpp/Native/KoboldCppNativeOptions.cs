using LocalAI.NET.Providers.KoboldCpp.Models;

namespace LocalAI.NET.Providers.KoboldCpp.Native
{
    public class KoboldCppNativeOptions : KoboldCppBaseOptions
    {
        /// <summary>
        /// Base repetition penalty value
        /// </summary>
        public float RepetitionPenalty { get; set; } = 1.1f;

        /// <summary>
        /// Repetition penalty range
        /// </summary>
        public int RepetitionPenaltyRange { get; set; } = 320;

        /// <summary>
        /// Whether to trim stop sequences from response
        /// </summary>
        public bool TrimStop { get; set; } = true;

        /// <summary>
        /// Mirostat settings if enabled
        /// </summary>
        public MirostatSettings? Mirostat { get; set; }
    }
}