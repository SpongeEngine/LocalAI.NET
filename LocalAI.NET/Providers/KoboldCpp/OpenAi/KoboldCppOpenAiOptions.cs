using LocalAI.NET.Providers.KoboldCpp.Models;

namespace LocalAI.NET.Providers.KoboldCpp.OpenAi
{
    public class KoboldCppOpenAiOptions : KoboldCppBaseOptions
    {
        /// <summary>
        /// Model name to use for requests
        /// </summary>
        public string ModelName { get; set; } = "koboldcpp";

        /// <summary>
        /// Whether to use chat completion endpoints
        /// </summary>
        public bool UseChatCompletions { get; set; } = false;
    }
}