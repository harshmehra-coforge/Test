using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Repositories.Interfaces;
using System.Text.Json;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Configuration service implementation
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<ConfigurationService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetConfigurationAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check cache first
                var cacheKey = $"config:{key}";
                var cachedValue = await _cacheService.GetAsync<T>(cacheKey, Services.Models.CacheLevel.Distributed, cancellationToken);
                if (cachedValue != null)
                {
                    _logger.LogDebug("Configuration cache hit for key: {Key}", key);
                    return cachedValue;
                }

                // Handle client-specific configuration
                if (key.StartsWith("client:") && int.TryParse(key.Substring(7), out var clientId))
                {
                    var clientConfig = await _unitOfWork.Clients.GetConfigurationAsync(clientId, cancellationToken);
                    if (clientConfig != null)
                    {
                        // Cache the configuration
                        await _cacheService.SetAsync(cacheKey, clientConfig, TimeSpan.FromMinutes(30), Services.Models.CacheLevel.Distributed, cancellationToken);
                        
                        if (clientConfig is T typedConfig)
                        {
                            return typedConfig;
                        }
                    }
                }

                _logger.LogDebug("Configuration not found for key: {Key}", key);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration for key: {Key}", key);
                return default(T);
            }
        }

        public async Task SetConfigurationAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            try
            {
                // Handle client-specific configuration
                if (key.StartsWith("client:") && int.TryParse(key.Substring(7), out var clientId))
                {
                    if (value is Models.DTOs.ClientConfigurationDto clientConfig)
                    {
                        await _unitOfWork.Clients.UpdateConfigurationAsync(clientConfig, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        
                        // Update cache
                        var cacheKey = $"config:{key}";
                        await _cacheService.SetAsync(cacheKey, value, TimeSpan.FromMinutes(30), Services.Models.CacheLevel.Distributed, cancellationToken);
                        
                        _logger.LogInformation("Client configuration updated for ClientId: {ClientId}", clientId);
                        return;
                    }
                }

                // For other configuration types, you would implement storage mechanism
                // For now, just cache the value
                var generalCacheKey = $"config:{key}";
                await _cacheService.SetAsync(generalCacheKey, value, TimeSpan.FromHours(1), Services.Models.CacheLevel.Distributed, cancellationToken);
                
                _logger.LogInformation("Configuration set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting configuration for key: {Key}", key);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetClientConfigurationAsync(int clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = await _unitOfWork.Clients.GetClientSettingsAsync(clientId, cancellationToken);
                return settings.ToDictionary(s => s.ConfigKey, s => s.ConfigValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client configuration for ClientId: {ClientId}", clientId);
                return new Dictionary<string, string>();
            }
        }
    }
}