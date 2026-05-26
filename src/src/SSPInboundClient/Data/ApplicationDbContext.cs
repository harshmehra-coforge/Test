using Microsoft.EntityFrameworkCore;
using SSPInboundClient.Models.Entities;

namespace SSPInboundClient.Data
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Client> Clients { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<ProcessingRule> ProcessingRules { get; set; }
        public DbSet<ClientConfiguration> ClientConfigurations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<TransformationMapping> TransformationMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Client entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.HasIndex(e => e.ApiKey).IsUnique().HasDatabaseName("IX_CLIENT_ApiKey");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_CLIENT_IsActive");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastModified).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure RequestLog entity
            modelBuilder.Entity<RequestLog>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.HasIndex(e => e.ClientId).HasDatabaseName("IX_REQUEST_LOG_ClientId");
                entity.HasIndex(e => e.RequestTimestamp).HasDatabaseName("IX_REQUEST_LOG_Timestamp");
                entity.HasIndex(e => e.CorrelationId).HasDatabaseName("IX_REQUEST_LOG_CorrelationId");
                entity.Property(e => e.RequestTimestamp).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(d => d.Client)
                    .WithMany(p => p.RequestLogs)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProcessingRule entity
            modelBuilder.Entity<ProcessingRule>(entity =>
            {
                entity.HasKey(e => e.RuleId);
                entity.HasIndex(e => new { e.RuleType, e.IsActive }).HasDatabaseName("IX_PROCESSING_RULE_Type_Active");
                entity.HasIndex(e => e.Priority).HasDatabaseName("IX_PROCESSING_RULE_Priority");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure ClientConfiguration entity
            modelBuilder.Entity<ClientConfiguration>(entity =>
            {
                entity.HasKey(e => e.ConfigId);
                entity.HasIndex(e => new { e.ClientId, e.ConfigKey }).IsUnique().HasDatabaseName("IX_CLIENT_CONFIGURATION_ClientId_Key");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Configurations)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditId);
                entity.HasIndex(e => e.EntityType).HasDatabaseName("IX_AUDIT_LOG_EntityType");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AUDIT_LOG_Timestamp");
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure ErrorLog entity
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.ErrorId);
                entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_ERROR_LOG_RequestId");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_ERROR_LOG_Timestamp");
                entity.HasIndex(e => e.Severity).HasDatabaseName("IX_ERROR_LOG_Severity");
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(d => d.RequestLog)
                    .WithMany(p => p.ErrorLogs)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure TransformationMapping entity
            modelBuilder.Entity<TransformationMapping>(entity =>
            {
                entity.HasKey(e => e.MappingId);
                entity.HasIndex(e => new { e.ClientId, e.SourceFormat, e.TargetFormat }).HasDatabaseName("IX_TRANSFORMATION_MAPPING_Client_Formats");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TransformationMappings)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default processing rules
            modelBuilder.Entity<ProcessingRule>().HasData(
                new ProcessingRule
                {
                    RuleId = 1,
                    RuleName = "Required Fields Validation",
                    RuleType = "Validation",
                    Condition = "request.CorrelationId != null && request.ClientId != null",
                    Action = "ValidateRequiredFields",
                    Priority = 1,
                    IsActive = true,
                    CreatedBy = "System"
                },
                new ProcessingRule
                {
                    RuleId = 2,
                    RuleName = "Client Authentication",
                    RuleType = "Authentication",
                    Condition = "request.ClientId != null",
                    Action = "ValidateClientCredentials",
                    Priority = 2,
                    IsActive = true,
                    CreatedBy = "System"
                },
                new ProcessingRule
                {
                    RuleId = 3,
                    RuleName = "Rate Limiting Check",
                    RuleType = "RateLimit",
                    Condition = "true",
                    Action = "CheckRateLimit",
                    Priority = 3,
                    IsActive = true,
                    CreatedBy = "System"
                }
            );

            // Seed default client
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    ClientId = 1,
                    ClientName = "Default Test Client",
                    ApiKey = "test-api-key-12345",
                    SecretHash = "hashed-secret-value",
                    ContactEmail = "test@example.com",
                    RateLimitPerMinute = 1000,
                    IsActive = true
                }
            );
        }
    }
}