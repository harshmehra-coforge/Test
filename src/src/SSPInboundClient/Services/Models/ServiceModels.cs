namespace SSPInboundClient.Services.Models
{
    /// <summary>
    /// Validation result model
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Failure(List<ValidationError> errors) => new ValidationResult { IsValid = false, Errors = errors };
        public static ValidationResult Failure(ValidationError error) => new ValidationResult { IsValid = false, Errors = new List<ValidationError> { error } };
    }

    /// <summary>
    /// Transformed request model
    /// </summary>
    public class TransformedRequest
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string OriginalFormat { get; set; } = string.Empty;
        public string TargetFormat { get; set; } = string.Empty;
        public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public DateTime TransformedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Routing result model
    /// </summary>
    public class RoutingResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorType { get; set; }
        public int ProcessingTimeMs { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Destination model for routing
    /// </summary>
    public class Destination
    {
        public DestinationType Type { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string? QueueName { get; set; }
        public int TimeoutMs { get; set; } = 30000;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Destination types
    /// </summary>
    public enum DestinationType
    {
        Http,
        MessageQueue,
        Database,
        File
    }

    /// <summary>
    /// Authentication result model
    /// </summary>
    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public string? ClientId { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Cache levels
    /// </summary>
    public enum CacheLevel
    {
        Memory,
        Distributed,
        Both
    }
}