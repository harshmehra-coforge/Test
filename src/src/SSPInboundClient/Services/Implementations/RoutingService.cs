using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Services.Models;
using System.Text.Json;
using System.Text;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Routing service implementation
    /// </summary>
    public class RoutingService : IRoutingService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<RoutingService> _logger;
        private readonly ICacheService _cacheService;

        public RoutingService(
            IHttpClientService httpClientService,
            IConfigurationService configurationService,
            ILogger<RoutingService> logger,
            ICacheService cacheService)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<RoutingResult> RouteAsync(TransformedRequest request, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Starting routing for CorrelationId: {CorrelationId}, RequestType: {RequestType}", 
                    request.CorrelationId, request.RequestType);

                var destination = await GetDestinationAsync(request.RequestType, cancellationToken);
                
                var result = destination.Type switch
                {
                    DestinationType.Http => await RouteToHttpEndpointAsync(request, destination, cancellationToken),
                    DestinationType.MessageQueue => await RouteToMessageQueueAsync(request, destination, cancellationToken),
                    DestinationType.Database => await RouteToDatabaseAsync(request, destination, cancellationToken),
                    DestinationType.File => await RouteToFileAsync(request, destination, cancellationToken),
                    _ => throw new InvalidOperationException($"Unsupported destination type: {destination.Type}")
                };

                result.ProcessingTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogInformation("Routing completed for CorrelationId: {CorrelationId}, Success: {Success}, Duration: {Duration}ms", 
                    request.CorrelationId, result.Success, result.ProcessingTimeMs);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during routing for CorrelationId: {CorrelationId}", request.CorrelationId);
                
                return new RoutingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    ProcessingTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
        }

        public async Task<Destination> GetDestinationAsync(string requestType, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check cache first
                var cacheKey = $"destination:{requestType}";
                var cachedDestination = await _cacheService.GetAsync<Destination>(cacheKey, CacheLevel.Distributed, cancellationToken);
                if (cachedDestination != null)
                {
                    return cachedDestination;
                }

                // Get destination configuration based on request type
                var destination = requestType.ToLowerInvariant() switch
                {
                    "payment" => new Destination
                    {
                        Type = DestinationType.Http,
                        Endpoint = "https://api.payment-service.com/v1/process",
                        TimeoutMs = 30000,
                        Headers = new Dictionary<string, string>
                        {
                            ["Content-Type"] = "application/json",
                            ["X-API-Version"] = "1.0"
                        }
                    },
                    "notification" => new Destination
                    {
                        Type = DestinationType.MessageQueue,
                        QueueName = "notifications",
                        TimeoutMs = 5000
                    },
                    "audit" => new Destination
                    {
                        Type = DestinationType.Database,
                        Endpoint = "audit-database",
                        TimeoutMs = 10000
                    },
                    "report" => new Destination
                    {
                        Type = DestinationType.File,
                        Endpoint = "reports-storage",
                        TimeoutMs = 15000
                    },
                    _ => new Destination
                    {
                        Type = DestinationType.Http,
                        Endpoint = "https://api.default-backend.com/v1/process",
                        TimeoutMs = 30000,
                        Headers = new Dictionary<string, string>
                        {
                            ["Content-Type"] = "application/json"
                        }
                    }
                };

                // Cache the destination
                await _cacheService.SetAsync(cacheKey, destination, TimeSpan.FromHours(1), CacheLevel.Distributed, cancellationToken);
                
                return destination;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting destination for RequestType: {RequestType}", requestType);
                throw;
            }
        }

        private async Task<RoutingResult> RouteToHttpEndpointAsync(
            TransformedRequest request, 
            Destination destination, 
            CancellationToken cancellationToken)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, destination.Endpoint)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(request.Payload),
                        Encoding.UTF8,
                        "application/json")
                };

                // Add correlation headers
                httpRequest.Headers.Add("X-Correlation-ID", request.CorrelationId);
                httpRequest.Headers.Add("X-Request-Source", "SSP-Inbound-Client");
                httpRequest.Headers.Add("X-Request-Type", request.RequestType);
                httpRequest.Headers.Add("X-Transformed-At", request.TransformedAt.ToString("O"));

                // Add destination-specific headers
                foreach (var header in destination.Headers)
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Add custom headers from the request
                foreach (var header in request.Headers)
                {
                    if (!httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var response = await _httpClientService.SendAsync(httpRequest, destination.TimeoutMs, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                return new RoutingResult
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Response = responseContent,
                    Metadata = new Dictionary<string, object>
                    {
                        ["endpoint"] = destination.Endpoint,
                        ["method"] = "POST",
                        ["responseHeaders"] = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
                    }
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP routing failed for CorrelationId: {CorrelationId}, Endpoint: {Endpoint}", 
                    request.CorrelationId, destination.Endpoint);

                return new RoutingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = "HTTP_REQUEST_FAILED",
                    Metadata = new Dictionary<string, object>
                    {
                        ["endpoint"] = destination.Endpoint,
                        ["method"] = "POST"
                    }
                };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "HTTP request timeout for CorrelationId: {CorrelationId}, Endpoint: {Endpoint}", 
                    request.CorrelationId, destination.Endpoint);

                return new RoutingResult
                {
                    Success = false,
                    ErrorMessage = "Request timeout",
                    ErrorType = "HTTP_TIMEOUT",
                    Metadata = new Dictionary<string, object>
                    {
                        ["endpoint"] = destination.Endpoint,
                        ["timeoutMs"] = destination.TimeoutMs
                    }
                };
            }
        }

        private async Task<RoutingResult> RouteToMessageQueueAsync(
            TransformedRequest request, 
            Destination destination, 
            CancellationToken cancellationToken)
        {
            try
            {
                // This would integrate with Azure Service Bus or other message queue
                // For now, simulate message queuing
                
                var message = new
                {
                    CorrelationId = request.CorrelationId,
                    RequestType = request.RequestType,
                    Payload = request.Payload,
                    Headers = request.Headers,
                    TransformedAt = request.TransformedAt,
                    QueuedAt = DateTime.UtcNow
                };

                var messageJson = JsonSerializer.Serialize(message);
                
                // Simulate message sending
                await Task.Delay(100, cancellationToken); // Simulate network delay
                
                _logger.LogInformation("Message queued successfully for CorrelationId: {CorrelationId}, Queue: {QueueName}", 
                    request.CorrelationId, destination.QueueName);

                return new RoutingResult
                {
                    Success = true,
                    StatusCode = 200,
                    Response = "Message queued successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["queueName"] = destination.QueueName ?? "default",
                        ["messageSize"] = messageJson.Length,
                        ["queuedAt"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message queue routing failed for CorrelationId: {CorrelationId}, Queue: {QueueName}", 
                    request.CorrelationId, destination.QueueName);

                return new RoutingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = "MESSAGE_QUEUE_FAILED",
                    Metadata = new Dictionary<string, object>
                    {
                        ["queueName"] = destination.QueueName ?? "default"
                    }
                };
            }
        }

        private async Task<RoutingResult> RouteToDatabaseAsync(
            TransformedRequest request, 
            Destination destination, 
            CancellationToken cancellationToken)
        {
            try
            {
                // This would integrate with database storage
                // For now, simulate database storage
                
                await Task.Delay(50, cancellationToken); // Simulate database operation
                
                _logger.LogInformation("Data stored to database successfully for CorrelationId: {CorrelationId}, Database: {Database}", 
                    request.CorrelationId, destination.Endpoint);

                return new RoutingResult
                {
                    Success = true,
                    StatusCode = 200,
                    Response = "Data stored successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["database"] = destination.Endpoint,
                        ["recordCount"] = 1,
                        ["storedAt"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database routing failed for CorrelationId: {CorrelationId}, Database: {Database}", 
                    request.CorrelationId, destination.Endpoint);

                return new RoutingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = "DATABASE_FAILED",
                    Metadata = new Dictionary<string, object>
                    {
                        ["database"] = destination.Endpoint
                    }
                };
            }
        }

        private async Task<RoutingResult> RouteToFileAsync(
            TransformedRequest request, 
            Destination destination, 
            CancellationToken cancellationToken)
        {
            try
            {
                // This would integrate with Azure Blob Storage or file system
                // For now, simulate file storage
                
                var fileName = $"{request.CorrelationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                var fileContent = JsonSerializer.Serialize(request.Payload, new JsonSerializerOptions { WriteIndented = true });
                
                await Task.Delay(200, cancellationToken); // Simulate file operation
                
                _logger.LogInformation("Data stored to file successfully for CorrelationId: {CorrelationId}, Storage: {Storage}, FileName: {FileName}", 
                    request.CorrelationId, destination.Endpoint, fileName);

                return new RoutingResult
                {
                    Success = true,
                    StatusCode = 200,
                    Response = "File stored successfully",
                    Metadata = new Dictionary<string, object>
                    {
                        ["storage"] = destination.Endpoint,
                        ["fileName"] = fileName,
                        ["fileSize"] = fileContent.Length,
                        ["storedAt"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File routing failed for CorrelationId: {CorrelationId}, Storage: {Storage}", 
                    request.CorrelationId, destination.Endpoint);

                return new RoutingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = "FILE_STORAGE_FAILED",
                    Metadata = new Dictionary<string, object>
                    {
                        ["storage"] = destination.Endpoint
                    }
                };
            }
        }
    }
}