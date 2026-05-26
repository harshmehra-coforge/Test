using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Services.Models;
using SSPInboundClient.Repositories.Interfaces;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Authentication service implementation
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<AuthenticationService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return new AuthenticationResult
                    {
                        IsAuthenticated = false,
                        ErrorMessage = "API key is required"
                    };
                }

                // Check cache first
                var cacheKey = $"auth:apikey:{apiKey}";
                var cachedResult = await _cacheService.GetAsync<AuthenticationResult>(cacheKey, CacheLevel.Both, cancellationToken);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Authentication cache hit for API key: {ApiKey}", MaskApiKey(apiKey));
                    return cachedResult;
                }

                // Validate API key against database
                var client = await _unitOfWork.Clients.GetByApiKeyAsync(apiKey, cancellationToken);
                if (client == null || !client.IsActive)
                {
                    var failureResult = new AuthenticationResult
                    {
                        IsAuthenticated = false,
                        ErrorMessage = "Invalid or inactive API key"
                    };
                    
                    // Cache negative result for short time to prevent brute force
                    await _cacheService.SetAsync(cacheKey, failureResult, TimeSpan.FromMinutes(5), CacheLevel.Both, cancellationToken);
                    
                    _logger.LogWarning("Authentication failed for API key: {ApiKey}", MaskApiKey(apiKey));
                    return failureResult;
                }

                var successResult = new AuthenticationResult
                {
                    IsAuthenticated = true,
                    ClientId = client.ClientId.ToString(),
                    Claims = new Dictionary<string, object>
                    {
                        ["clientId"] = client.ClientId,
                        ["clientName"] = client.ClientName,
                        ["rateLimitPerMinute"] = client.RateLimitPerMinute,
                        ["authenticatedAt"] = DateTime.UtcNow
                    }
                };

                // Cache positive result for longer time
                await _cacheService.SetAsync(cacheKey, successResult, TimeSpan.FromMinutes(30), CacheLevel.Both, cancellationToken);
                
                _logger.LogInformation("Authentication successful for client: {ClientId}", client.ClientId);
                return successResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for API key: {ApiKey}", MaskApiKey(apiKey));
                return new AuthenticationResult
                {
                    IsAuthenticated = false,
                    ErrorMessage = "Authentication service error"
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                // This would integrate with JWT token validation
                // For now, implement basic token validation
                
                // Check cache first
                var cacheKey = $"auth:token:{token.GetHashCode()}";
                var cachedResult = await _cacheService.GetAsync<bool?>(cacheKey, CacheLevel.Memory, cancellationToken);
                if (cachedResult.HasValue)
                {
                    return cachedResult.Value;
                }

                // Validate token (this would use JWT validation in real implementation)
                var isValid = await ValidateJwtTokenAsync(token, cancellationToken);
                
                // Cache result for short time
                await _cacheService.SetAsync(cacheKey, isValid, TimeSpan.FromMinutes(5), CacheLevel.Memory, cancellationToken);
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        private async Task<bool> ValidateJwtTokenAsync(string token, CancellationToken cancellationToken)
        {
            // This would implement actual JWT token validation
            // For now, return true for non-empty tokens
            await Task.Delay(10, cancellationToken); // Simulate validation delay
            return !string.IsNullOrWhiteSpace(token) && token.Length > 10;
        }

        private string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
                return "***";
            
            return $"{apiKey[..4]}***{apiKey[^4..]}";
        }
    }
}