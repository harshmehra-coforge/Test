using Microsoft.EntityFrameworkCore.Storage;
using SSPInboundClient.Data;
using SSPInboundClient.Models.Entities;
using SSPInboundClient.Repositories.Interfaces;

namespace SSPInboundClient.Repositories.Implementations
{
    /// <summary>
    /// Unit of Work implementation for managing transactions and repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private IClientRepository? _clients;
        private IRepository<RequestLog>? _requestLogs;
        private IRepository<ProcessingRule>? _processingRules;
        private IRepository<ClientConfiguration>? _clientConfigurations;
        private IRepository<AuditLog>? _auditLogs;
        private IRepository<ErrorLog>? _errorLogs;
        private IRepository<TransformationMapping>? _transformationMappings;

        public UnitOfWork(
            ApplicationDbContext context, 
            ILogger<UnitOfWork> logger,
            IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        public IClientRepository Clients
        {
            get
            {
                _clients ??= _serviceProvider.GetRequiredService<IClientRepository>();
                return _clients;
            }
        }

        public IRepository<RequestLog> RequestLogs
        {
            get
            {
                _requestLogs ??= new BaseRepository<RequestLog>(_context, 
                    _serviceProvider.GetRequiredService<ILogger<BaseRepository<RequestLog>>>());
                return _requestLogs;
            }
        }

        public IRepository<ProcessingRule> ProcessingRules
        {
            get
            {
                _processingRules ??= new BaseRepository<ProcessingRule>(_context, 
                    _serviceProvider.GetRequiredService<ILogger<BaseRepository<ProcessingRule>>>());
                return _processingRules;
            }
        }

        public IRepository<ClientConfiguration> ClientConfigurations
        {
            get
            {
                _clientConfigurations ??= new BaseRepository<ClientConfiguration>(_context, 
                    _serviceProvider.GetRequiredService<ILogger<BaseRepository<ClientConfiguration>>>());
                return _clientConfigurations;
            }
        }

        public IRepository<AuditLog> AuditLogs
        {
            get
            {
                _auditLogs ??= new BaseRepository<AuditLog>(_context, 
                    _serviceProvider.GetRequiredService<ILogger<BaseRepository<AuditLog>>>());
                return _auditLogs;
            }
        }

        public IRepository<ErrorLog> ErrorLogs
        {
            get
            {
                _errorLogs ??= new BaseRepository<ErrorLog>(_context, 
                    _serviceProvider.GetRequiredService<ILogger<BaseRepository<ErrorLog>>>());
                return _errorLogs;
            }
        }

        public IRepository<TransformationMapping> TransformationMappings
        {
            get
            {
                _transformationMappings ??= new BaseRepository<TransformationMapping>(_context, 
                    _serviceProvider.GetRequiredService<ILogger<BaseRepository<TransformationMapping>>>());
                return _transformationMappings;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Database transaction started");
                return _transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning database transaction");
                throw;
            }
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                    _logger.LogDebug("Database transaction committed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing database transaction");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync(cancellationToken);
                    _logger.LogDebug("Database transaction rolled back");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back database transaction");
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}