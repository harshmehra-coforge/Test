using System.ComponentModel.DataAnnotations;

namespace SSPInboundClient.Models.DTOs
{
    /// <summary>
    /// Represents the response from processing an inbound request
    /// </summary>
    public class ProcessingResponse
    {
        /// <summary>
        /// Correlation ID from the original request
        /// </summary>
        [Required]
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Processing status (Success, Failed, Pending)
        /// </summary>
        [Required]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Response data
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Validation or processing errors
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// When the request was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Processing time in milliseconds
        /// </summary>
        public int ProcessingTimeMs { get; set; }

        /// <summary>
        /// Transaction identifier
        /// </summary>
        public string? TransactionId { get; set; }
    }

    /// <summary>
    /// Represents a validation error
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Field that failed validation
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error severity (Critical, Warning, Info)
        /// </summary>
        public string Severity { get; set; } = "Error";
    }
}