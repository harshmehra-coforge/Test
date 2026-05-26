using System.ComponentModel.DataAnnotations;

namespace SSPInboundClient.Models.DTOs
{
    /// <summary>
    /// Represents an inbound request from a client
    /// </summary>
    public class InboundRequest
    {
        /// <summary>
        /// Unique identifier for tracking the request
        /// </summary>
        [Required]
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Client identifier
        /// </summary>
        [Required]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Request timestamp
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Type of request being processed
        /// </summary>
        [Required]
        public string RequestType { get; set; } = string.Empty;

        /// <summary>
        /// Request payload data
        /// </summary>
        public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Additional headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Request priority level
        /// </summary>
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// Whether a callback is required
        /// </summary>
        public bool RequiresCallback { get; set; } = false;

        /// <summary>
        /// Callback URL if required
        /// </summary>
        public string? CallbackUrl { get; set; }
    }
}