using SSPInboundClient.Models.DTOs;
using System.Text.Json;

namespace SSPInboundClient.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? context.TraceIdentifier;

            _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}", 
                correlationId, context.Request.Path);

            var response = exception switch
            {
                ArgumentNullException argEx => new ErrorResponse
                {
                    Code = "INVALID_ARGUMENT",
                    Message = "Invalid argument provided",
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["parameter"] = argEx.ParamName ?? "Unknown"
                    }
                },
                
                ArgumentException argEx => new ErrorResponse
                {
                    Code = "INVALID_ARGUMENT",
                    Message = argEx.Message,
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                },
                
                InvalidOperationException invOpEx => new ErrorResponse
                {
                    Code = "INVALID_OPERATION",
                    Message = invOpEx.Message,
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                },
                
                TimeoutException timeoutEx => new ErrorResponse
                {
                    Code = "TIMEOUT",
                    Message = "The operation timed out",
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                },
                
                UnauthorizedAccessException => new ErrorResponse
                {
                    Code = "UNAUTHORIZED",
                    Message = "Access denied",
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                },
                
                _ => new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An internal error occurred",
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                }
            };

            var statusCode = exception switch
            {
                ArgumentException => 400,
                ArgumentNullException => 400,
                InvalidOperationException => 400,
                UnauthorizedAccessException => 401,
                TimeoutException => 408,
                _ => 500
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}