namespace LocalAI.NET.Models.Progress
{
    /// <summary>
    /// Represents progress information for LocalAI operations
    /// </summary>
    public class LocalAIProgress
    {
        /// <summary>
        /// Current state of the operation
        /// </summary>
        public LocalAIProgressState State { get; init; }

        /// <summary>
        /// Progress message
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Current attempt number if retrying
        /// </summary>
        public int Attempt { get; init; }

        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxAttempts { get; init; }

        /// <summary>
        /// Time elapsed since operation start
        /// </summary>
        public TimeSpan ElapsedTime { get; init; }

        /// <summary>
        /// Optional percentage complete (0-100)
        /// </summary>
        public double? PercentComplete { get; init; }
    }
}