using System.Diagnostics;

namespace SSPInboundClient.Middleware
{
    /// <summary>
    /// Request logging middleware for structured logging
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? Guid.NewGuid().ToString();

            // Add correlation ID to response headers
            context.Response.Headers.Add("X-Correlation-ID", correlationId);

            // Add correlation ID to logging context
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method,
                ["UserAgent"] = context.Request.Headers["User-Agent"].FirstOrDefault(),
                ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString(),
                ["RequestId"] = context.TraceIdentifier
            });

            try
            {
                _logger.LogInformation("Request started: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation("Request completed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms", 
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Response.StatusCode, 
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex, "Request failed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms", 
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Response.StatusCode, 
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}