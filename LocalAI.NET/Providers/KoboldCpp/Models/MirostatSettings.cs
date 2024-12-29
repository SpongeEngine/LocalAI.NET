namespace LocalAI.NET.Providers.KoboldCpp.Models
{
    public class MirostatSettings
    {
        /// <summary>
        /// Sets the mirostat mode (0=disabled, 1=mirostat_v1, 2=mirostat_v2)
        /// </summary>
        public int Mode { get; set; } = 0;

        /// <summary>
        /// Mirostat tau value
        /// </summary>
        public float Tau { get; set; } = 0.0f;

        /// <summary>
        /// Mirostat eta value
        /// </summary>
        public float Eta { get; set; } = 0.0f;
    }
}