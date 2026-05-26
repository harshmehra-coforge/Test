using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Services.Models;
using SSPInboundClient.Models.DTOs;
using SSPInboundClient.Repositories.Interfaces;
using System.Text.Json;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Transformation service implementation
    /// </summary>
    public class TransformationService : ITransformationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransformationService> _logger;
        private readonly ICacheService _cacheService;

        public TransformationService(
            IUnitOfWork unitOfWork,
            ILogger<TransformationService> logger,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<TransformedRequest> TransformAsync(
            InboundRequest request, 
            TransformationSettingsDto? config = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting transformation for CorrelationId: {CorrelationId}", request.CorrelationId);

                // Get transformation configuration if not provided
                if (config == null && int.TryParse(request.ClientId, out var clientId))
                {
                    config = await GetTransformationConfigAsync(clientId, cancellationToken);
                }

                var transformed = new TransformedRequest
                {
                    CorrelationId = request.CorrelationId,
                    RequestType = request.RequestType,
                    OriginalFormat = config?.SourceFormat ?? "JSON",
                    TargetFormat = config?.TargetFormat ?? "JSON",
                    Headers = new Dictionary<string, string>(request.Headers),
                    TransformedAt = DateTime.UtcNow
                };

                // Apply field mappings if configuration exists
                if (config != null && config.FieldMappings.Any())
                {
                    transformed.Payload = ApplyFieldMappings(request.Payload, config.FieldMappings);
                }
                else
                {
                    // No transformation needed, copy payload as-is
                    transformed.Payload = new Dictionary<string, object>(request.Payload);
                }

                // Apply transformation rules
                if (config?.Rules != null && config.Rules.Any())
                {
                    foreach (var rule in config.Rules.Where(r => r.IsActive).OrderBy(r => r.Priority))
                    {
                        transformed.Payload = await ApplyTransformationRuleAsync(transformed.Payload, rule, cancellationToken);
                    }
                }

                // Add metadata
                transformed.Headers["X-Original-Format"] = transformed.OriginalFormat;
                transformed.Headers["X-Target-Format"] = transformed.TargetFormat;
                transformed.Headers["X-Transformed-At"] = transformed.TransformedAt.ToString("O");
                transformed.Headers["X-Transformation-Version"] = "1.0";

                // Validate transformed data
                var validationResult = await ValidateTransformedDataAsync(transformed, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Transformed data validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.Message))}");
                }

                _logger.LogInformation("Transformation completed successfully for CorrelationId: {CorrelationId}", request.CorrelationId);
                return transformed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transformation for CorrelationId: {CorrelationId}", request.CorrelationId);
                throw;
            }
        }

        private async Task<TransformationSettingsDto?> GetTransformationConfigAsync(int clientId, CancellationToken cancellationToken)
        {
            try
            {
                // Check cache first
                var cacheKey = $"transformation:config:{clientId}";
                var cachedConfig = await _cacheService.GetAsync<TransformationSettingsDto>(cacheKey, CacheLevel.Distributed, cancellationToken);
                if (cachedConfig != null)
                {
                    return cachedConfig;
                }

                // Get from database
                var clientConfig = await _unitOfWork.Clients.GetConfigurationAsync(clientId, cancellationToken);
                if (clientConfig?.Transformation != null)
                {
                    // Cache the configuration
                    await _cacheService.SetAsync(cacheKey, clientConfig.Transformation, TimeSpan.FromHours(1), CacheLevel.Distributed, cancellationToken);
                    return clientConfig.Transformation;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transformation configuration for ClientId: {ClientId}", clientId);
                return null;
            }
        }

        private Dictionary<string, object> ApplyFieldMappings(
            Dictionary<string, object> source,
            Dictionary<string, string> mappings)
        {
            var result = new Dictionary<string, object>();

            try
            {
                foreach (var mapping in mappings)
                {
                    var sourceField = mapping.Key;
                    var targetField = mapping.Value;

                    if (source.TryGetValue(sourceField, out var value))
                    {
                        // Handle nested field mappings (e.g., "customer.name" -> "clientName")
                        if (sourceField.Contains('.'))
                        {
                            value = GetNestedValue(source, sourceField);
                        }

                        if (value != null)
                        {
                            // Handle nested target fields (e.g., "address.street")
                            if (targetField.Contains('.'))
                            {
                                SetNestedValue(result, targetField, value);
                            }
                            else
                            {
                                result[targetField] = value;
                            }
                        }
                    }
                }

                // Copy unmapped fields if they don't conflict
                foreach (var kvp in source)
                {
                    if (!mappings.ContainsKey(kvp.Key) && !result.ContainsKey(kvp.Key))
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying field mappings");
                throw;
            }
        }

        private object? GetNestedValue(Dictionary<string, object> source, string path)
        {
            var parts = path.Split('.');
            object? current = source;

            foreach (var part in parts)
            {
                if (current is Dictionary<string, object> dict && dict.TryGetValue(part, out var value))
                {
                    current = value;
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        private void SetNestedValue(Dictionary<string, object> target, string path, object value)
        {
            var parts = path.Split('.');
            var current = target;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.TryGetValue(parts[i], out var next) || next is not Dictionary<string, object>)
                {
                    next = new Dictionary<string, object>();
                    current[parts[i]] = next;
                }
                current = (Dictionary<string, object>)next;
            }

            current[parts[^1]] = value;
        }

        private async Task<Dictionary<string, object>> ApplyTransformationRuleAsync(
            Dictionary<string, object> payload,
            TransformationRuleDto rule,
            CancellationToken cancellationToken)
        {
            try
            {
                // Simple rule execution - in production, this would use a proper rules engine
                switch (rule.Type.ToLowerInvariant())
                {
                    case "format":
                        return ApplyFormatRule(payload, rule);
                    
                    case "calculate":
                        return ApplyCalculationRule(payload, rule);
                    
                    case "enrich":
                        return await ApplyEnrichmentRuleAsync(payload, rule, cancellationToken);
                    
                    case "filter":
                        return ApplyFilterRule(payload, rule);
                    
                    default:
                        _logger.LogWarning("Unknown transformation rule type: {Type}", rule.Type);
                        return payload;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying transformation rule: {RuleName}", rule.Name);
                return payload; // Return original payload on error
            }
        }

        private Dictionary<string, object> ApplyFormatRule(Dictionary<string, object> payload, TransformationRuleDto rule)
        {
            // Example: Format date fields, normalize strings, etc.
            var result = new Dictionary<string, object>(payload);

            // Parse rule action for formatting instructions
            var actionParts = rule.Action.Split('|');
            if (actionParts.Length >= 2)
            {
                var fieldName = actionParts[0];
                var formatType = actionParts[1];

                if (result.TryGetValue(fieldName, out var value))
                {
                    switch (formatType.ToLowerInvariant())
                    {
                        case "uppercase":
                            result[fieldName] = value?.ToString()?.ToUpperInvariant() ?? value;
                            break;
                        case "lowercase":
                            result[fieldName] = value?.ToString()?.ToLowerInvariant() ?? value;
                            break;
                        case "date":
                            if (DateTime.TryParse(value?.ToString(), out var date))
                            {
                                result[fieldName] = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            }
                            break;
                    }
                }
            }

            return result;
        }

        private Dictionary<string, object> ApplyCalculationRule(Dictionary<string, object> payload, TransformationRuleDto rule)
        {
            // Example: Calculate derived fields
            var result = new Dictionary<string, object>(payload);

            // Simple calculation example: total = quantity * price
            if (rule.Action.Contains("total") && 
                result.TryGetValue("quantity", out var qtyObj) && 
                result.TryGetValue("price", out var priceObj))
            {
                if (decimal.TryParse(qtyObj?.ToString(), out var quantity) && 
                    decimal.TryParse(priceObj?.ToString(), out var price))
                {
                    result["total"] = quantity * price;
                }
            }

            return result;
        }

        private async Task<Dictionary<string, object>> ApplyEnrichmentRuleAsync(
            Dictionary<string, object> payload, 
            TransformationRuleDto rule, 
            CancellationToken cancellationToken)
        {
            // Example: Enrich with additional data from external sources
            var result = new Dictionary<string, object>(payload);

            // Add timestamp if not present
            if (!result.ContainsKey("processedAt"))
            {
                result["processedAt"] = DateTime.UtcNow.ToString("O");
            }

            // Add system metadata
            result["enrichedBy"] = "SSP-Inbound-Client";
            result["enrichmentVersion"] = "1.0";

            return result;
        }

        private Dictionary<string, object> ApplyFilterRule(Dictionary<string, object> payload, TransformationRuleDto rule)
        {
            // Example: Remove sensitive or unnecessary fields
            var result = new Dictionary<string, object>(payload);

            // Parse rule action for fields to remove
            var fieldsToRemove = rule.Action.Split(',').Select(f => f.Trim()).ToList();
            foreach (var field in fieldsToRemove)
            {
                result.Remove(field);
            }

            return result;
        }

        private async Task<ValidationResult> ValidateTransformedDataAsync(TransformedRequest transformed, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationError>();

            try
            {
                // Basic validation
                if (transformed.Payload == null || !transformed.Payload.Any())
                {
                    errors.Add(new ValidationError
                    {
                        Field = "Payload",
                        Code = "EMPTY_TRANSFORMED_PAYLOAD",
                        Message = "Transformed payload cannot be empty",
                        Severity = "Critical"
                    });
                }

                // Validate JSON serialization
                try
                {
                    JsonSerializer.Serialize(transformed.Payload);
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError
                    {
                        Field = "Payload",
                        Code = "INVALID_JSON",
                        Message = $"Transformed payload is not valid JSON: {ex.Message}",
                        Severity = "Critical"
                    });
                }

                return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transformed data for CorrelationId: {CorrelationId}", transformed.CorrelationId);
                return ValidationResult.Failure(new ValidationError
                {
                    Field = "System",
                    Code = "VALIDATION_ERROR",
                    Message = "Error occurred during transformed data validation",
                    Severity = "Critical"
                });
            }
        }
    }
}