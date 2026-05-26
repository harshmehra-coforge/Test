using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Services.Models;
using SSPInboundClient.Models.DTOs;
using SSPInboundClient.Repositories.Interfaces;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Validation service implementation
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ValidationService> _logger;
        private readonly ICacheService _cacheService;

        public ValidationService(
            IUnitOfWork unitOfWork,
            ILogger<ValidationService> logger,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<ValidationResult> ValidateAsync(InboundRequest request, CancellationToken cancellationToken = default)
        {
            var errors = new List<ValidationError>();

            try
            {
                // Basic field validation
                if (string.IsNullOrWhiteSpace(request.CorrelationId))
                {
                    errors.Add(new ValidationError
                    {
                        Field = nameof(request.CorrelationId),
                        Code = "REQUIRED_FIELD",
                        Message = "CorrelationId is required",
                        Severity = "Critical"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ClientId))
                {
                    errors.Add(new ValidationError
                    {
                        Field = nameof(request.ClientId),
                        Code = "REQUIRED_FIELD",
                        Message = "ClientId is required",
                        Severity = "Critical"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.RequestType))
                {
                    errors.Add(new ValidationError
                    {
                        Field = nameof(request.RequestType),
                        Code = "REQUIRED_FIELD",
                        Message = "RequestType is required",
                        Severity = "Critical"
                    });
                }

                // Validate timestamp
                if (request.Timestamp == default || request.Timestamp > DateTime.UtcNow.AddMinutes(5))
                {
                    errors.Add(new ValidationError
                    {
                        Field = nameof(request.Timestamp),
                        Code = "INVALID_TIMESTAMP",
                        Message = "Timestamp is invalid or too far in the future",
                        Severity = "Warning"
                    });
                }

                // Validate client exists and is active
                if (!string.IsNullOrWhiteSpace(request.ClientId))
                {
                    var clientValidation = await ValidateClientAsync(request.ClientId, cancellationToken);
                    if (!clientValidation.IsValid)
                    {
                        errors.AddRange(clientValidation.Errors);
                    }
                }

                // Execute business rules
                var ruleValidation = await ExecuteValidationRulesAsync(request, cancellationToken);
                if (!ruleValidation.IsValid)
                {
                    errors.AddRange(ruleValidation.Errors);
                }

                return errors.Any(e => e.Severity == "Critical") 
                    ? ValidationResult.Failure(errors)
                    : ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during validation for CorrelationId: {CorrelationId}", request.CorrelationId);
                errors.Add(new ValidationError
                {
                    Field = "System",
                    Code = "VALIDATION_ERROR",
                    Message = "An error occurred during validation",
                    Severity = "Critical"
                });
                return ValidationResult.Failure(errors);
            }
        }

        public async Task<ValidationResult> ValidateClientAsync(string clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check cache first
                var cacheKey = $"client:validation:{clientId}";
                var cachedResult = await _cacheService.GetAsync<ValidationResult>(cacheKey, CacheLevel.Both, cancellationToken);
                if (cachedResult != null)
                {
                    return cachedResult;
                }

                // Validate client exists and is active
                if (!int.TryParse(clientId, out var clientIdInt))
                {
                    var result = ValidationResult.Failure(new ValidationError
                    {
                        Field = "ClientId",
                        Code = "INVALID_CLIENT_ID",
                        Message = "ClientId must be a valid integer",
                        Severity = "Critical"
                    });
                    
                    // Cache negative result for short time
                    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), CacheLevel.Both, cancellationToken);
                    return result;
                }

                var isActive = await _unitOfWork.Clients.IsClientActiveAsync(clientIdInt, cancellationToken);
                if (!isActive)
                {
                    var result = ValidationResult.Failure(new ValidationError
                    {
                        Field = "ClientId",
                        Code = "INACTIVE_CLIENT",
                        Message = "Client is not active or does not exist",
                        Severity = "Critical"
                    });
                    
                    // Cache negative result for short time
                    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), CacheLevel.Both, cancellationToken);
                    return result;
                }

                var successResult = ValidationResult.Success();
                // Cache positive result for longer time
                await _cacheService.SetAsync(cacheKey, successResult, TimeSpan.FromMinutes(30), CacheLevel.Both, cancellationToken);
                return successResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating client: {ClientId}", clientId);
                return ValidationResult.Failure(new ValidationError
                {
                    Field = "ClientId",
                    Code = "CLIENT_VALIDATION_ERROR",
                    Message = "Error occurred while validating client",
                    Severity = "Critical"
                });
            }
        }

        private async Task<ValidationResult> ExecuteValidationRulesAsync(InboundRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Get validation rules from cache or database
                var cacheKey = "rules:validation";
                var rules = await _cacheService.GetAsync<List<Models.Entities.ProcessingRule>>(cacheKey, CacheLevel.Distributed, cancellationToken);
                
                if (rules == null)
                {
                    rules = (await _unitOfWork.ProcessingRules.FindAsync(r => r.RuleType == "Validation" && r.IsActive, cancellationToken)).ToList();
                    await _cacheService.SetAsync(cacheKey, rules, TimeSpan.FromMinutes(15), CacheLevel.Distributed, cancellationToken);
                }

                var errors = new List<ValidationError>();

                foreach (var rule in rules.OrderBy(r => r.Priority))
                {
                    try
                    {
                        var ruleResult = await ExecuteRuleAsync(rule, request, cancellationToken);
                        if (!ruleResult.IsValid)
                        {
                            errors.AddRange(ruleResult.Errors);
                            
                            // Stop on first critical error
                            if (ruleResult.Errors.Any(e => e.Severity == "Critical"))
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing validation rule {RuleId}", rule.RuleId);
                        errors.Add(new ValidationError
                        {
                            Code = "RULE_EXECUTION_ERROR",
                            Message = $"Error executing rule {rule.RuleName}",
                            Severity = "Warning",
                            Field = "System"
                        });
                    }
                }

                return errors.Any(e => e.Severity == "Critical") 
                    ? ValidationResult.Failure(errors)
                    : ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing validation rules for CorrelationId: {CorrelationId}", request.CorrelationId);
                return ValidationResult.Failure(new ValidationError
                {
                    Code = "RULES_EXECUTION_ERROR",
                    Message = "Error occurred while executing validation rules",
                    Severity = "Critical",
                    Field = "System"
                });
            }
        }

        private async Task<ValidationResult> ExecuteRuleAsync(Models.Entities.ProcessingRule rule, InboundRequest request, CancellationToken cancellationToken)
        {
            // Simple rule execution - in production, this would use a proper rules engine
            switch (rule.Action)
            {
                case "ValidateRequiredFields":
                    return ValidateRequiredFields(request);
                
                case "ValidateClientCredentials":
                    return await ValidateClientCredentials(request, cancellationToken);
                
                case "CheckRateLimit":
                    return await CheckRateLimit(request, cancellationToken);
                
                default:
                    _logger.LogWarning("Unknown validation rule action: {Action}", rule.Action);
                    return ValidationResult.Success();
            }
        }

        private ValidationResult ValidateRequiredFields(InboundRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Payload == null || !request.Payload.Any())
            {
                errors.Add(new ValidationError
                {
                    Field = nameof(request.Payload),
                    Code = "EMPTY_PAYLOAD",
                    Message = "Request payload cannot be empty",
                    Severity = "Critical"
                });
            }

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        private async Task<ValidationResult> ValidateClientCredentials(InboundRequest request, CancellationToken cancellationToken)
        {
            // This would integrate with the authentication service
            return ValidationResult.Success();
        }

        private async Task<ValidationResult> CheckRateLimit(InboundRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (!int.TryParse(request.ClientId, out var clientId))
                {
                    return ValidationResult.Success(); // Skip rate limiting if client ID is invalid
                }

                var rateLimit = await _unitOfWork.Clients.GetRateLimitAsync(clientId, cancellationToken);
                var currentMinute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                var rateLimitKey = $"ratelimit:{clientId}:{currentMinute}";
                
                var currentCount = await _cacheService.GetAsync<int>(rateLimitKey, CacheLevel.Distributed, cancellationToken);
                
                if (currentCount >= rateLimit)
                {
                    return ValidationResult.Failure(new ValidationError
                    {
                        Field = "RateLimit",
                        Code = "RATE_LIMIT_EXCEEDED",
                        Message = $"Rate limit of {rateLimit} requests per minute exceeded",
                        Severity = "Critical"
                    });
                }

                // Increment counter
                await _cacheService.SetAsync(rateLimitKey, currentCount + 1, TimeSpan.FromMinutes(2), CacheLevel.Distributed, cancellationToken);
                
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for client: {ClientId}", request.ClientId);
                // Don't fail validation due to rate limit check errors
                return ValidationResult.Success();
            }
        }
    }
}