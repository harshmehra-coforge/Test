using SSPInboundClient.Models.DTOs;
using SSPInboundClient.Services.Interfaces;
using System.Text.Json;

namespace SSPInboundClient.Middleware
{
    /// <summary>
    /// Rate limiting middleware
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public RateLimitingMiddleware(
            RequestDelegate next, 
            ILogger<RateLimitingMiddleware> logger,
            ICacheService cacheService,
            IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for health checks and system endpoints
            if (context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/api/v1/system"))
            {
                await _next(context);
                return;
            }

            try
            {
                var clientId = GetClientId(context);
                if (string.IsNullOrEmpty(clientId))
                {
                    // If no client ID, apply default rate limiting
                    clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                }

                var isAllowed = await CheckRateLimitAsync(clientId);
                if (!isAllowed)
                {
                    await HandleRateLimitExceeded(context, clientId);
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting middleware");
                // Continue processing even if rate limiting fails
                await _next(context);
            }
        }

        private string? GetClientId(HttpContext context)
        {
            // Try to get client ID from various sources
            
            // 1. From Authorization header (if JWT contains client ID)
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                // This would parse JWT token to extract client ID
                // For now, return null to use IP-based rate limiting
            }

            // 2. From API Key header
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                // This would look up client ID by API key
                // For now, use the API key as client identifier
                return apiKey;
            }

            // 3. From request body (for inbound requests)
            if (context.Request.Method == "POST" && 
                context.Request.ContentType?.Contains("application/json") == true)
            {
                // This would require reading the request body, which is complex in middleware
                // Better to handle this in the controller or a later middleware
            }

            return null;
        }

        private async Task<bool> CheckRateLimitAsync(string clientId)
        {
            try
            {
                var defaultLimit = _configuration.GetValue<int>("RateLimiting:DefaultRequestsPerMinute", 1000);
                var burstLimit = _configuration.GetValue<int>("RateLimiting:BurstRequestsPerMinute", 2000);
                
                var currentMinute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                var rateLimitKey = $"ratelimit:{clientId}:{currentMinute}";
                
                var currentCount = await _cacheService.GetAsync<int>(rateLimitKey, Services.Models.CacheLevel.Distributed);
                
                // Check against burst limit first
                if (currentCount >= burstLimit)
                {
                    _logger.LogWarning("Burst rate limit exceeded for client: {ClientId}, Count: {Count}, Limit: {Limit}", 
                        clientId, currentCount, burstLimit);
                    return false;
                }

                // Check against regular limit
                if (currentCount >= defaultLimit)
                {
                    _logger.LogWarning("Rate limit exceeded for client: {ClientId}, Count: {Count}, Limit: {Limit}", 
                        clientId, currentCount, defaultLimit);
                    return false;
                }

                // Increment counter
                await _cacheService.SetAsync(rateLimitKey, currentCount + 1, TimeSpan.FromMinutes(2), Services.Models.CacheLevel.Distributed);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for client: {ClientId}", clientId);
                // Allow request if rate limiting check fails
                return true;
            }
        }

        private async Task HandleRateLimitExceeded(HttpContext context, string clientId)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? context.TraceIdentifier;

            _logger.LogWarning("Rate limit exceeded for client: {ClientId}, CorrelationId: {CorrelationId}", 
                clientId, correlationId);

            var response = new ErrorResponse
            {
                Code = "RATE_LIMIT_EXCEEDED",
                Message = "Rate limit exceeded. Please try again later.",
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["clientId"] = clientId,
                    ["retryAfter"] = "60 seconds"
                }
            };

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Retry-After", "60");
            context.Response.Headers.Add("X-RateLimit-Limit", "1000");
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString());

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}