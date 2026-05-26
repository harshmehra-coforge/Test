using Polly;
using Polly.Extensions.Http;

namespace SSPInboundClient.Configuration
{
    /// <summary>
    /// Retry policies for HTTP clients and other operations
    /// </summary>
    public static class RetryPolicies
    {
        /// <summary>
        /// Get retry policy for HTTP requests
        /// </summary>
        /// <returns>Retry policy</returns>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException and 5XX and 408 status codes
                .OrResult(msg => !msg.IsSuccessStatusCode && (int)msg.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var logger = context.GetLogger();
                        if (outcome.Exception != null)
                        {
                            logger?.LogWarning("HTTP retry attempt {RetryCount} after {Delay}ms due to: {Exception}", 
                                retryCount, timespan.TotalMilliseconds, outcome.Exception.Message);
                        }
                        else
                        {
                            logger?.LogWarning("HTTP retry attempt {RetryCount} after {Delay}ms due to status code: {StatusCode}", 
                                retryCount, timespan.TotalMilliseconds, outcome.Result?.StatusCode);
                        }
                    });
        }

        /// <summary>
        /// Get circuit breaker policy for HTTP requests
        /// </summary>
        /// <returns>Circuit breaker policy</returns>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, timespan) =>
                    {
                        // Log circuit breaker opening
                    },
                    onReset: () =>
                    {
                        // Log circuit breaker closing
                    });
        }

        /// <summary>
        /// Get timeout policy
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Timeout policy</returns>
        public static IAsyncPolicy GetTimeoutPolicy(int timeoutSeconds = 30)
        {
            return Policy.TimeoutAsync(timeoutSeconds);
        }
    }

    /// <summary>
    /// Extension methods for Polly context
    /// </summary>
    public static class PolicyContextExtensions
    {
        private const string LoggerKey = "ILogger";

        public static Context WithLogger(this Context context, ILogger logger)
        {
            context[LoggerKey] = logger;
            return context;
        }

        public static ILogger? GetLogger(this Context context)
        {
            return context.TryGetValue(LoggerKey, out var logger) ? logger as ILogger : null;
        }
    }
}