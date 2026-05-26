using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Models.DTOs;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Structured logger implementation for consistent logging across the application
    /// </summary>
    public class StructuredLogger : IStructuredLogger
    {
        private readonly ILogger<StructuredLogger> _logger;

        public StructuredLogger(ILogger<StructuredLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogRequestReceived(string correlationId, string clientId, string endpoint)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["ClientId"] = clientId,
                ["Endpoint"] = endpoint,
                ["EventType"] = "RequestReceived"
            });

            _logger.LogInformation("Request received from client {ClientId} at endpoint {Endpoint}", clientId, endpoint);
        }

        public void LogRequestProcessed(string correlationId, int processingTimeMs, string status, int? errorCount = null)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["ProcessingTimeMs"] = processingTimeMs,
                ["Status"] = status,
                ["ErrorCount"] = errorCount,
                ["EventType"] = "RequestProcessed"
            });

            if (status == "Success")
            {
                _logger.LogInformation("Request processed successfully in {ProcessingTimeMs}ms", processingTimeMs);
            }
            else
            {
                _logger.LogWarning("Request processing completed with status {Status} in {ProcessingTimeMs}ms, Errors: {ErrorCount}", 
                    status, processingTimeMs, errorCount ?? 0);
            }
        }

        public void LogValidationFailed(string correlationId, List<ValidationError> errors)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["ValidationErrors"] = errors.Select(e => new { e.Field, e.Code, e.Message, e.Severity }),
                ["ErrorCount"] = errors.Count,
                ["EventType"] = "ValidationFailed"
            });

            var criticalErrors = errors.Count(e => e.Severity == "Critical");
            var warningErrors = errors.Count(e => e.Severity == "Warning");

            _logger.LogWarning("Request validation failed with {ErrorCount} errors (Critical: {CriticalCount}, Warnings: {WarningCount})", 
                errors.Count, criticalErrors, warningErrors);

            // Log each critical error individually for better tracking
            foreach (var error in errors.Where(e => e.Severity == "Critical"))
            {
                _logger.LogError("Critical validation error - Field: {Field}, Code: {Code}, Message: {Message}", 
                    error.Field, error.Code, error.Message);
            }
        }

        public void LogPerformanceMetric(string operation, int durationMs, Dictionary<string, object>? additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["DurationMs"] = durationMs,
                ["EventType"] = "PerformanceMetric",
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            using var scope = _logger.BeginScope(logData);

            if (durationMs > 5000) // Log as warning if operation takes more than 5 seconds
            {
                _logger.LogWarning("Slow operation detected: {Operation} took {DurationMs}ms", operation, durationMs);
            }
            else if (durationMs > 1000) // Log as info if operation takes more than 1 second
            {
                _logger.LogInformation("Operation {Operation} completed in {DurationMs}ms", operation, durationMs);
            }
            else
            {
                _logger.LogDebug("Operation {Operation} completed in {DurationMs}ms", operation, durationMs);
            }
        }
    }
}