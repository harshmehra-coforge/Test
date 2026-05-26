using SSPInboundClient.Models.DTOs;

namespace SSPInboundClient.Services.Interfaces
{
    /// <summary>
    /// Processing engine interface for orchestrating request processing
    /// </summary>
    public interface IProcessingEngine
    {
        Task<ProcessingResponse> ProcessAsync(InboundRequest request, CancellationToken cancellationToken = default);
        Task<ProcessingResponse> GetStatusAsync(string correlationId, CancellationToken cancellationToken = default);
        Task<ProcessingResponse[]> ProcessBatchAsync(InboundRequest[] requests, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Validation service interface
    /// </summary>
    public interface IValidationService
    {
        Task<ValidationResult> ValidateAsync(InboundRequest request, CancellationToken cancellationToken = default);
        Task<ValidationResult> ValidateClientAsync(string clientId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Transformation service interface
    /// </summary>
    public interface ITransformationService
    {
        Task<TransformedRequest> TransformAsync(InboundRequest request, TransformationSettingsDto? config = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Routing service interface
    /// </summary>
    public interface IRoutingService
    {
        Task<RoutingResult> RouteAsync(TransformedRequest request, CancellationToken cancellationToken = default);
        Task<Destination> GetDestinationAsync(string requestType, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Authentication service interface
    /// </summary>
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string apiKey, CancellationToken cancellationToken = default);
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Cache service interface
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// HTTP client service interface
    /// </summary>
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, int timeoutMs = 30000, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Configuration service interface
    /// </summary>
    public interface IConfigurationService
    {
        Task<T?> GetConfigurationAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetConfigurationAsync<T>(string key, T value, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetClientConfigurationAsync(int clientId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Metrics collector interface
    /// </summary>
    public interface IMetricsCollector
    {
        void RecordRequestCount(string clientId, string endpoint, string status);
        void RecordProcessingTime(string operation, double durationMs);
        void RecordErrorRate(string errorType, string component);
        void RecordThroughput(int requestsPerSecond);
        void RecordCacheHitRate(string cacheType, bool isHit);
    }

    /// <summary>
    /// Structured logger interface
    /// </summary>
    public interface IStructuredLogger
    {
        void LogRequestReceived(string correlationId, string clientId, string endpoint);
        void LogRequestProcessed(string correlationId, int processingTimeMs, string status, int? errorCount = null);
        void LogValidationFailed(string correlationId, List<ValidationError> errors);
        void LogPerformanceMetric(string operation, int durationMs, Dictionary<string, object>? additionalData = null);
    }
}