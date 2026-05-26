using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SSPInboundClient.Controllers
{
    /// <summary>
    /// Controller for managing client configurations
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "ApiAccess")]
    public class ClientsController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<ClientsController> _logger;
        private readonly IMetricsCollector _metricsCollector;

        public ClientsController(
            IConfigurationService configurationService,
            ILogger<ClientsController> logger,
            IMetricsCollector metricsCollector)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        }

        /// <summary>
        /// Get client configuration
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Client configuration</returns>
        [HttpGet("{clientId}/config")]
        [ProducesResponseType(typeof(ClientConfigurationDto), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetClientConfiguration(
            [FromRoute] [Required] int clientId,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting configuration for ClientId: {ClientId}", clientId);
                
                var config = await _configurationService.GetConfigurationAsync<ClientConfigurationDto>($"client:{clientId}", cancellationToken);
                
                if (config == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Code = "CLIENT_NOT_FOUND",
                        Message = $"Client with ID {clientId} not found",
                        Timestamp = DateTime.UtcNow
                    });
                }

                _metricsCollector.RecordRequestCount(clientId.ToString(), "get-config", "Success");
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client configuration for ClientId: {ClientId}", clientId);
                _metricsCollector.RecordErrorRate("ConfigurationError", "ClientsController");
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while retrieving client configuration",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Update client configuration
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="config">Updated configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated client configuration</returns>
        [HttpPut("{clientId}/config")]
        [ProducesResponseType(typeof(ClientConfigurationDto), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> UpdateClientConfiguration(
            [FromRoute] [Required] int clientId,
            [FromBody] ClientConfigurationDto config,
            CancellationToken cancellationToken)
        {
            try
            {
                if (config == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = "INVALID_CONFIG",
                        Message = "Configuration cannot be null",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (config.ClientId != clientId)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = "CLIENT_ID_MISMATCH",
                        Message = "Client ID in URL does not match configuration",
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Updating configuration for ClientId: {ClientId}", clientId);
                
                await _configurationService.SetConfigurationAsync($"client:{clientId}", config, cancellationToken);
                
                _metricsCollector.RecordRequestCount(clientId.ToString(), "update-config", "Success");
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client configuration for ClientId: {ClientId}", clientId);
                _metricsCollector.RecordErrorRate("ConfigurationUpdateError", "ClientsController");
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while updating client configuration",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}