using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SSPInboundClient.Controllers
{
    /// <summary>
    /// Controller for handling inbound client requests
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "ApiAccess")]
    public class InboundController : ControllerBase
    {
        private readonly IProcessingEngine _processingEngine;
        private readonly ILogger<InboundController> _logger;
        private readonly IStructuredLogger _structuredLogger;
        private readonly IMetricsCollector _metricsCollector;

        public InboundController(
            IProcessingEngine processingEngine,
            ILogger<InboundController> logger,
            IStructuredLogger structuredLogger,
            IMetricsCollector metricsCollector)
        {
            _processingEngine = processingEngine ?? throw new ArgumentNullException(nameof(processingEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _structuredLogger = structuredLogger ?? throw new ArgumentNullException(nameof(structuredLogger));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        }

        /// <summary>
        /// Process an inbound request
        /// </summary>
        /// <param name="request">The inbound request to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processing response</returns>
        [HttpPost("process")]
        [ProducesResponseType(typeof(ProcessingResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 429)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> ProcessInboundRequest(
            [FromBody] InboundRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = "INVALID_REQUEST",
                        Message = "Request body cannot be null",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Ensure correlation ID is set
                if (string.IsNullOrWhiteSpace(request.CorrelationId))
                {
                    request.CorrelationId = Guid.NewGuid().ToString();
                }

                _structuredLogger.LogRequestReceived(request.CorrelationId, request.ClientId, "process");
                
                var response = await _processingEngine.ProcessAsync(request, cancellationToken);
                
                _metricsCollector.RecordRequestCount(request.ClientId, "process", response.Status);
                
                return response.Status == "Success" ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in ProcessInboundRequest");
                _metricsCollector.RecordErrorRate("UnhandledException", "InboundController");
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An internal error occurred",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get the processing status of a request
        /// </summary>
        /// <param name="correlationId">The correlation ID of the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processing status</returns>
        [HttpGet("status/{correlationId}")]
        [ProducesResponseType(typeof(ProcessingResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetProcessingStatus(
            [FromRoute] [Required] string correlationId,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correlationId))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = "INVALID_CORRELATION_ID",
                        Message = "Correlation ID cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var response = await _processingEngine.GetStatusAsync(correlationId, cancellationToken);
                
                return response.Status == "NotFound" ? NotFound(response) : Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting processing status for CorrelationId: {CorrelationId}", correlationId);
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An internal error occurred while retrieving status",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Process multiple requests in batch
        /// </summary>
        /// <param name="requests">Array of inbound requests</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array of processing responses</returns>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(ProcessingResponse[]), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 413)] // Payload too large
        public async Task<IActionResult> ProcessBatchRequests(
            [FromBody] InboundRequest[] requests,
            CancellationToken cancellationToken)
        {
            try
            {
                if (requests == null || !requests.Any())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = "EMPTY_BATCH",
                        Message = "Batch request cannot be empty",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (requests.Length > 100) // Configurable limit
                {
                    return StatusCode(413, new ErrorResponse
                    {
                        Code = "BATCH_TOO_LARGE",
                        Message = "Batch size cannot exceed 100 requests",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Ensure all requests have correlation IDs
                foreach (var request in requests.Where(r => string.IsNullOrWhiteSpace(r.CorrelationId)))
                {
                    request.CorrelationId = Guid.NewGuid().ToString();
                }

                _logger.LogInformation("Processing batch of {RequestCount} requests", requests.Length);
                
                var responses = await _processingEngine.ProcessBatchAsync(requests, cancellationToken);
                
                _metricsCollector.RecordRequestCount("Batch", "batch", "Completed");
                
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch requests");
                _metricsCollector.RecordErrorRate("BatchProcessingError", "InboundController");
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "BATCH_PROCESSING_ERROR",
                    Message = "An error occurred while processing the batch",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}