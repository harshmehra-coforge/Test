using Microsoft.EntityFrameworkCore.Storage;
using SSPInboundClient.Models.Entities;

namespace SSPInboundClient.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work interface for managing transactions and repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository Clients { get; }
        IRepository<RequestLog> RequestLogs { get; }
        IRepository<ProcessingRule> ProcessingRules { get; }
        IRepository<ClientConfiguration> ClientConfigurations { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<ErrorLog> ErrorLogs { get; }
        IRepository<TransformationMapping> TransformationMappings { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}