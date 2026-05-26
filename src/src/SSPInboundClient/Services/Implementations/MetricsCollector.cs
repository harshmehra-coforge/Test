using Microsoft.ApplicationInsights;
using SSPInboundClient.Services.Interfaces;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Metrics collector implementation using Application Insights
    /// </summary>
    public class MetricsCollector : IMetricsCollector
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<MetricsCollector> _logger;

        public MetricsCollector(TelemetryClient telemetryClient, ILogger<MetricsCollector> logger)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void RecordRequestCount(string clientId, string endpoint, string status)
        {
            try
            {
                _telemetryClient.TrackMetric("RequestCount", 1, new Dictionary<string, string>
                {
                    ["ClientId"] = clientId,
                    ["Endpoint"] = endpoint,
                    ["Status"] = status,
                    ["Timestamp"] = DateTime.UtcNow.ToString("O")
                });

                _logger.LogDebug("Recorded request count metric: ClientId={ClientId}, Endpoint={Endpoint}, Status={Status}", 
                    clientId, endpoint, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording request count metric");
            }
        }

        public void RecordProcessingTime(string operation, double durationMs)
        {
            try
            {
                _telemetryClient.TrackMetric("ProcessingTime", durationMs, new Dictionary<string, string>
                {
                    ["Operation"] = operation,
                    ["Timestamp"] = DateTime.UtcNow.ToString("O")
                });

                // Also track as dependency for better visualization
                _telemetryClient.TrackDependency("Internal", operation, DateTime.UtcNow.AddMilliseconds(-durationMs), 
                    TimeSpan.FromMilliseconds(durationMs), true);

                _logger.LogDebug("Recorded processing time metric: Operation={Operation}, Duration={Duration}ms", 
                    operation, durationMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording processing time metric");
            }
        }

        public void RecordErrorRate(string errorType, string component)
        {
            try
            {
                _telemetryClient.TrackMetric("ErrorRate", 1, new Dictionary<string, string>
                {
                    ["ErrorType"] = errorType,
                    ["Component"] = component,
                    ["Timestamp"] = DateTime.UtcNow.ToString("O")
                });

                // Also track as exception for better error tracking
                _telemetryClient.TrackException(new Exception($"{errorType} in {component}"), new Dictionary<string, string>
                {
                    ["ErrorType"] = errorType,
                    ["Component"] = component
                });

                _logger.LogDebug("Recorded error rate metric: ErrorType={ErrorType}, Component={Component}", 
                    errorType, component);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording error rate metric");
            }
        }

        public void RecordThroughput(int requestsPerSecond)
        {
            try
            {
                _telemetryClient.TrackMetric("Throughput", requestsPerSecond, new Dictionary<string, string>
                {
                    ["Timestamp"] = DateTime.UtcNow.ToString("O")
                });

                _logger.LogDebug("Recorded throughput metric: RequestsPerSecond={RequestsPerSecond}", requestsPerSecond);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording throughput metric");
            }
        }

        public void RecordCacheHitRate(string cacheType, bool isHit)
        {
            try
            {
                _telemetryClient.TrackMetric("CacheHitRate", isHit ? 1 : 0, new Dictionary<string, string>
                {
                    ["CacheType"] = cacheType,
                    ["Result"] = isHit ? "Hit" : "Miss",
                    ["Timestamp"] = DateTime.UtcNow.ToString("O")
                });

                _logger.LogDebug("Recorded cache hit rate metric: CacheType={CacheType}, IsHit={IsHit}", 
                    cacheType, isHit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache hit rate metric");
            }
        }
    }
}