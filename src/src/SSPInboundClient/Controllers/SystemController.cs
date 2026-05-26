using Microsoft.AspNetCore.Mvc;
using SSPInboundClient.Models.DTOs;
using System.Diagnostics;
using System.Reflection;

namespace SSPInboundClient.Controllers
{
    /// <summary>
    /// Controller for system health checks and metrics
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;

        public SystemController(ILogger<SystemController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get system health status
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(HealthStatus), 200)]
        public IActionResult GetHealth()
        {
            try
            {
                var healthStatus = new HealthStatus
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Checks = new Dictionary<string, object>
                    {
                        ["api"] = "Healthy",
                        ["database"] = "Healthy", // This would be actual health check
                        ["cache"] = "Healthy",    // This would be actual health check
                        ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"
                    }
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                
                return StatusCode(500, new HealthStatus
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Checks = new Dictionary<string, object>
                    {
                        ["error"] = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Get system metrics
        /// </summary>
        /// <returns>System metrics</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(SystemMetrics), 200)]
        public IActionResult GetMetrics()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                
                var metrics = new SystemMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsage = 0, // This would be calculated from performance counters
                    MemoryUsage = process.WorkingSet64 / (1024.0 * 1024.0), // MB
                    RequestsPerSecond = 0, // This would come from metrics collector
                    AverageResponseTime = 0, // This would come from metrics collector
                    ActiveConnections = 0, // This would come from connection tracking
                    CustomMetrics = new Dictionary<string, object>
                    {
                        ["processId"] = process.Id,
                        ["startTime"] = process.StartTime,
                        ["uptime"] = DateTime.UtcNow - process.StartTime,
                        ["threadCount"] = process.Threads.Count,
                        ["gcCollections"] = new Dictionary<string, object>
                        {
                            ["gen0"] = GC.CollectionCount(0),
                            ["gen1"] = GC.CollectionCount(1),
                            ["gen2"] = GC.CollectionCount(2)
                        }
                    }
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "METRICS_ERROR",
                    Message = "Error retrieving system metrics",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get API version information
        /// </summary>
        /// <returns>Version information</returns>
        [HttpGet("version")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult GetVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var buildDate = new FileInfo(assembly.Location).LastWriteTime;

                var versionInfo = new
                {
                    version = version?.ToString() ?? "Unknown",
                    buildDate = buildDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    framework = Environment.Version.ToString(),
                    machineName = Environment.MachineName,
                    timestamp = DateTime.UtcNow
                };

                return Ok(versionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version information");
                
                return StatusCode(500, new ErrorResponse
                {
                    Code = "VERSION_ERROR",
                    Message = "Error retrieving version information",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}