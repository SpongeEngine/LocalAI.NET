namespace LocalAI.NET.Models.Progress
{
    /// <summary>
    /// Represents the current state of an operation
    /// </summary>
    public enum LocalAIProgressState
    {
        /// <summary>
        /// Operation is starting
        /// </summary>
        Starting,

        /// <summary>
        /// Request is being processed
        /// </summary>
        Processing,

        /// <summary>
        /// Tokens are being streamed
        /// </summary>
        Streaming,

        /// <summary>
        /// Operation completed successfully
        /// </summary>
        Complete,

        /// <summary>
        /// Operation failed
        /// </summary>
        Failed
    }
}