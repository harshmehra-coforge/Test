using Microsoft.EntityFrameworkCore;
using SSPInboundClient.Data;
using SSPInboundClient.Models.Entities;
using SSPInboundClient.Models.DTOs;
using SSPInboundClient.Repositories.Interfaces;
using System.Text.Json;

namespace SSPInboundClient.Repositories.Implementations
{
    /// <summary>
    /// Client repository implementation
    /// </summary>
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDbContext context, ILogger<ClientRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<Client?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(c => c.Configurations)
                    .Include(c => c.TransformationMappings)
                    .FirstOrDefaultAsync(c => c.ApiKey == apiKey && c.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client by API key: {ApiKey}", apiKey);
                throw;
            }
        }

        public async Task<ClientConfigurationDto?> GetConfigurationAsync(int clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = await _dbSet
                    .Include(c => c.Configurations)
                    .Include(c => c.TransformationMappings)
                    .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

                if (client == null)
                    return null;

                var config = new ClientConfigurationDto
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName,
                    ApiKey = client.ApiKey,
                    IsActive = client.IsActive,
                    RateLimitPerMinute = client.RateLimitPerMinute,
                    CustomSettings = client.Configurations.ToDictionary(c => c.ConfigKey, c => c.ConfigValue)
                };

                // Get transformation settings
                var transformationMapping = client.TransformationMappings.FirstOrDefault(tm => tm.IsActive);
                if (transformationMapping != null)
                {
                    var mappingRules = JsonSerializer.Deserialize<Dictionary<string, string>>(transformationMapping.MappingRules) 
                        ?? new Dictionary<string, string>();

                    config.Transformation = new TransformationSettingsDto
                    {
                        SourceFormat = transformationMapping.SourceFormat,
                        TargetFormat = transformationMapping.TargetFormat,
                        FieldMappings = mappingRules
                    };
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client configuration for ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<ClientConfigurationDto> UpdateConfigurationAsync(ClientConfigurationDto config, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = await _dbSet
                    .Include(c => c.Configurations)
                    .Include(c => c.TransformationMappings)
                    .FirstOrDefaultAsync(c => c.ClientId == config.ClientId, cancellationToken);

                if (client == null)
                    throw new InvalidOperationException($"Client with ID {config.ClientId} not found");

                // Update client properties
                client.ClientName = config.ClientName;
                client.IsActive = config.IsActive;
                client.RateLimitPerMinute = config.RateLimitPerMinute;
                client.LastModified = DateTime.UtcNow;

                // Update configurations
                foreach (var setting in config.CustomSettings)
                {
                    var existingConfig = client.Configurations.FirstOrDefault(c => c.ConfigKey == setting.Key);
                    if (existingConfig != null)
                    {
                        existingConfig.ConfigValue = setting.Value;
                        existingConfig.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        client.Configurations.Add(new ClientConfiguration
                        {
                            ClientId = client.ClientId,
                            ConfigKey = setting.Key,
                            ConfigValue = setting.Value,
                            DataType = "String",
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }

                // Update transformation settings
                if (config.Transformation != null)
                {
                    var existingMapping = client.TransformationMappings.FirstOrDefault(tm => tm.IsActive);
                    if (existingMapping != null)
                    {
                        existingMapping.SourceFormat = config.Transformation.SourceFormat;
                        existingMapping.TargetFormat = config.Transformation.TargetFormat;
                        existingMapping.MappingRules = JsonSerializer.Serialize(config.Transformation.FieldMappings);
                    }
                    else
                    {
                        client.TransformationMappings.Add(new TransformationMapping
                        {
                            ClientId = client.ClientId,
                            SourceFormat = config.Transformation.SourceFormat,
                            TargetFormat = config.Transformation.TargetFormat,
                            MappingRules = JsonSerializer.Serialize(config.Transformation.FieldMappings),
                            IsActive = true
                        });
                    }
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client configuration for ClientId: {ClientId}", config.ClientId);
                throw;
            }
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(c => c.ApiKey == apiKey && c.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key: {ApiKey}", apiKey);
                throw;
            }
        }

        public async Task<List<ClientConfiguration>> GetClientSettingsAsync(int clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ClientConfigurations
                    .Where(cc => cc.ClientId == clientId)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client settings for ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<bool> IsClientActiveAsync(int clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(c => c.ClientId == clientId && c.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if client is active for ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<int> GetRateLimitAsync(int clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = await _dbSet.FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);
                return client?.RateLimitPerMinute ?? 1000; // Default rate limit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rate limit for ClientId: {ClientId}", clientId);
                throw;
            }
        }
    }
}