using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Services.Models;
using SSPInboundClient.Models.DTOs;
using SSPInboundClient.Models.Entities;
using SSPInboundClient.Repositories.Interfaces;
using System.Diagnostics;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Main processing engine that orchestrates request processing
    /// </summary>
    public class ProcessingEngine : IProcessingEngine
    {
        private readonly IValidationService _validationService;
        private readonly ITransformationService _transformationService;
        private readonly IRoutingService _routingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessingEngine> _logger;
        private readonly IStructuredLogger _structuredLogger;
        private readonly IMetricsCollector _metricsCollector;
        private readonly ICacheService _cacheService;

        public ProcessingEngine(
            IValidationService validationService,
            ITransformationService transformationService,
            IRoutingService routingService,
            IUnitOfWork unitOfWork,
            ILogger<ProcessingEngine> logger,
            IStructuredLogger structuredLogger,
            IMetricsCollector metricsCollector,
            ICacheService cacheService)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _transformationService = transformationService ?? throw new ArgumentNullException(nameof(transformationService));
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _structuredLogger = structuredLogger ?? throw new ArgumentNullException(nameof(structuredLogger));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<ProcessingResponse> ProcessAsync(InboundRequest request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new ProcessingResponse
            {
                CorrelationId = request.CorrelationId,
                ProcessedAt = DateTime.UtcNow
            };

            RequestLog? requestLog = null;

            try
            {
                _structuredLogger.LogRequestReceived(request.CorrelationId, request.ClientId, "ProcessAsync");
                _logger.LogInformation("Starting request processing for CorrelationId: {CorrelationId}", request.CorrelationId);

                // Create request log entry
                requestLog = await CreateRequestLogAsync(request, cancellationToken);

                // Step 1: Validation
                _logger.LogDebug("Starting validation for CorrelationId: {CorrelationId}", request.CorrelationId);
                var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                
                if (!validationResult.IsValid)
                {
                    _structuredLogger.LogValidationFailed(request.CorrelationId, validationResult.Errors);
                    response.Status = "Failed";
                    response.Message = "Validation failed";
                    response.Errors = validationResult.Errors;
                    
                    await LogErrorsAsync(requestLog.RequestId, validationResult.Errors, cancellationToken);
                    _metricsCollector.RecordRequestCount(request.ClientId, "ProcessAsync", "ValidationFailed");
                    
                    return response;
                }

                // Step 2: Transformation
                _logger.LogDebug("Starting transformation for CorrelationId: {CorrelationId}", request.CorrelationId);
                var transformedRequest = await _transformationService.TransformAsync(request, cancellationToken: cancellationToken);

                // Step 3: Routing
                _logger.LogDebug("Starting routing for CorrelationId: {CorrelationId}", request.CorrelationId);
                var routingResult = await _routingService.RouteAsync(transformedRequest, cancellationToken);

                // Step 4: Build response
                if (routingResult.Success)
                {
                    response.Status = "Success";
                    response.Message = "Request processed successfully";
                    response.Data = new Dictionary<string, object>
                    {
                        ["routingResult"] = routingResult.Response ?? "No response data",
                        ["destination"] = routingResult.Metadata.GetValueOrDefault("endpoint", "Unknown"),
                        ["statusCode"] = routingResult.StatusCode
                    };
                    response.TransactionId = Guid.NewGuid().ToString();
                    
                    _metricsCollector.RecordRequestCount(request.ClientId, "ProcessAsync", "Success");
                }
                else
                {
                    response.Status = "Failed";
                    response.Message = "Routing failed";
                    response.Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "Routing",
                            Code = routingResult.ErrorType ?? "ROUTING_ERROR",
                            Message = routingResult.ErrorMessage ?? "Unknown routing error",
                            Severity = "Critical"
                        }
                    };
                    
                    await LogErrorsAsync(requestLog.RequestId, response.Errors, cancellationToken);
                    _metricsCollector.RecordRequestCount(request.ClientId, "ProcessAsync", "RoutingFailed");
                }

                stopwatch.Stop();
                response.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

                // Update request log
                await UpdateRequestLogAsync(requestLog, response, cancellationToken);

                // Record metrics
                _metricsCollector.RecordProcessingTime("ProcessAsync", stopwatch.ElapsedMilliseconds);
                _structuredLogger.LogRequestProcessed(
                    request.CorrelationId, 
                    response.ProcessingTimeMs, 
                    response.Status, 
                    response.Errors?.Count);

                _logger.LogInformation("Request processing completed for CorrelationId: {CorrelationId}, Status: {Status}, Duration: {Duration}ms", 
                    request.CorrelationId, response.Status, response.ProcessingTimeMs);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error processing request for CorrelationId: {CorrelationId}", request.CorrelationId);

                response.Status = "Failed";
                response.Message = "Internal processing error";
                response.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;
                response.Errors = new List<ValidationError>
                {
                    new ValidationError
                    {
                        Field = "System",
                        Code = "INTERNAL_ERROR",
                        Message = "An internal error occurred during processing",
                        Severity = "Critical"
                    }
                };

                // Log error
                if (requestLog != null)
                {
                    await LogErrorsAsync(requestLog.RequestId, response.Errors, cancellationToken);
                    await UpdateRequestLogAsync(requestLog, response, cancellationToken);
                }

                _metricsCollector.RecordRequestCount(request.ClientId, "ProcessAsync", "InternalError");
                _metricsCollector.RecordErrorRate("InternalError", "ProcessingEngine");

                return response;
            }
        }

        public async Task<ProcessingResponse> GetStatusAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting status for CorrelationId: {CorrelationId}", correlationId);

                // Check cache first
                var cacheKey = $"status:{correlationId}";
                var cachedStatus = await _cacheService.GetAsync<ProcessingResponse>(cacheKey, CacheLevel.Distributed, cancellationToken);
                if (cachedStatus != null)
                {
                    _metricsCollector.RecordCacheHitRate("StatusCache", true);
                    return cachedStatus;
                }

                _metricsCollector.RecordCacheHitRate("StatusCache", false);

                // Get from database
                var requestLog = await _unitOfWork.RequestLogs
                    .FirstOrDefaultAsync(r => r.CorrelationId == correlationId, cancellationToken);

                if (requestLog == null)
                {
                    return new ProcessingResponse
                    {
                        CorrelationId = correlationId,
                        Status = "NotFound",
                        Message = "Request not found",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var response = new ProcessingResponse
                {
                    CorrelationId = correlationId,
                    Status = requestLog.StatusCode >= 200 && requestLog.StatusCode < 300 ? "Success" : "Failed",
                    ProcessedAt = requestLog.ResponseTimestamp ?? requestLog.RequestTimestamp,
                    ProcessingTimeMs = requestLog.ProcessingTimeMs ?? 0
                };

                // Get error details if any
                var errors = await _unitOfWork.ErrorLogs
                    .FindAsync(e => e.RequestId == requestLog.RequestId, cancellationToken);

                if (errors.Any())
                {
                    response.Errors = errors.Select(e => new ValidationError
                    {
                        Field = "System",
                        Code = e.ErrorType,
                        Message = e.ErrorMessage,
                        Severity = e.Severity
                    }).ToList();
                }

                // Cache the result
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), CacheLevel.Distributed, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for CorrelationId: {CorrelationId}", correlationId);
                
                return new ProcessingResponse
                {
                    CorrelationId = correlationId,
                    Status = "Error",
                    Message = "Error retrieving status",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<ProcessingResponse[]> ProcessBatchAsync(InboundRequest[] requests, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting batch processing for {RequestCount} requests", requests.Length);

                var semaphore = new SemaphoreSlim(10); // Limit concurrent processing
                var responses = new List<ProcessingResponse>();

                var tasks = requests.Select(async request =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return await ProcessAsync(request, cancellationToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var results = await Task.WhenAll(tasks);
                
                _logger.LogInformation("Batch processing completed for {RequestCount} requests", requests.Length);
                _metricsCollector.RecordRequestCount("Batch", "ProcessBatchAsync", "Completed");

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch processing");
                _metricsCollector.RecordErrorRate("BatchProcessingError", "ProcessingEngine");
                
                // Return error responses for all requests
                return requests.Select(request => new ProcessingResponse
                {
                    CorrelationId = request.CorrelationId,
                    Status = "Failed",
                    Message = "Batch processing error",
                    ProcessedAt = DateTime.UtcNow,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "System",
                            Code = "BATCH_PROCESSING_ERROR",
                            Message = "Error occurred during batch processing",
                            Severity = "Critical"
                        }
                    }
                }).ToArray();
            }
        }

        private async Task<RequestLog> CreateRequestLogAsync(InboundRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var requestLog = new RequestLog
                {
                    ClientId = int.TryParse(request.ClientId, out var clientId) ? clientId : 0,
                    RequestPath = "/api/v1/inbound/process",
                    HttpMethod = "POST",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(request),
                    CorrelationId = request.CorrelationId,
                    RequestTimestamp = DateTime.UtcNow,
                    UserAgent = "SSP-Inbound-Client",
                    IpAddress = "127.0.0.1" // This would come from HttpContext in real implementation
                };

                await _unitOfWork.RequestLogs.AddAsync(requestLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return requestLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request log for CorrelationId: {CorrelationId}", request.CorrelationId);
                throw;
            }
        }

        private async Task UpdateRequestLogAsync(RequestLog requestLog, ProcessingResponse response, CancellationToken cancellationToken)
        {
            try
            {
                requestLog.ResponseBody = System.Text.Json.JsonSerializer.Serialize(response);
                requestLog.StatusCode = response.Status == "Success" ? 200 : 400;
                requestLog.ResponseTimestamp = DateTime.UtcNow;
                requestLog.ProcessingTimeMs = response.ProcessingTimeMs;

                await _unitOfWork.RequestLogs.UpdateAsync(requestLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request log for RequestId: {RequestId}", requestLog.RequestId);
                // Don't throw here as it's not critical to the main processing flow
            }
        }

        private async Task LogErrorsAsync(long requestId, List<ValidationError> errors, CancellationToken cancellationToken)
        {
            try
            {
                var errorLogs = errors.Select(error => new ErrorLog
                {
                    RequestId = requestId,
                    ErrorType = error.Code,
                    ErrorMessage = error.Message,
                    Source = error.Field,
                    Severity = error.Severity,
                    Timestamp = DateTime.UtcNow
                });

                await _unitOfWork.ErrorLogs.AddRangeAsync(errorLogs, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging errors for RequestId: {RequestId}", requestId);
                // Don't throw here as it's not critical to the main processing flow
            }
        }
    }
}