using SSPInboundClient.Models.Entities;
using SSPInboundClient.Models.DTOs;

namespace SSPInboundClient.Repositories.Interfaces
{
    /// <summary>
    /// Client repository interface with specific client operations
    /// </summary>
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
        Task<ClientConfigurationDto?> GetConfigurationAsync(int clientId, CancellationToken cancellationToken = default);
        Task<ClientConfigurationDto> UpdateConfigurationAsync(ClientConfigurationDto config, CancellationToken cancellationToken = default);
        Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
        Task<List<ClientConfiguration>> GetClientSettingsAsync(int clientId, CancellationToken cancellationToken = default);
        Task<bool> IsClientActiveAsync(int clientId, CancellationToken cancellationToken = default);
        Task<int> GetRateLimitAsync(int clientId, CancellationToken cancellationToken = default);
    }
}