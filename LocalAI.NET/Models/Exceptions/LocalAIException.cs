namespace LocalAI.NET.Models.Exceptions
{
    /// <summary>
    /// Exception thrown by LocalAI.NET operations
    /// </summary>
    public class LocalAIException : Exception
    {
        /// <summary>
        /// HTTP status code if applicable
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Raw response content if available
        /// </summary>
        public string? ResponseContent { get; }

        /// <summary>
        /// Provider that threw the exception
        /// </summary>
        public string? Provider { get; }

        public LocalAIException(string message) : base(message)
        {
        }

        public LocalAIException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public LocalAIException(string message, string? provider = null, int? statusCode = null, string? responseContent = null)
            : base(message)
        {
            Provider = provider;
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}