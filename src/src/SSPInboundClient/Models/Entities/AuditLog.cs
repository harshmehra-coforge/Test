using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents audit log entries for tracking system changes
    /// </summary>
    [Table("AUDIT_LOG")]
    public class AuditLog
    {
        [Key]
        [Column("AuditId")]
        public long AuditId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("EntityType")]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("EntityId")]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("Action")]
        public string Action { get; set; } = string.Empty;

        [Column("OldValues")]
        public string? OldValues { get; set; }

        [Column("NewValues")]
        public string? NewValues { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("UserId")]
        public string UserId { get; set; } = string.Empty;

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        [Column("IpAddress")]
        public string? IpAddress { get; set; }
    }
}