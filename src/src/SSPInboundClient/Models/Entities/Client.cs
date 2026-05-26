using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents a client that can send requests to the SSP Inbound Client system
    /// </summary>
    [Table("CLIENT")]
    public class Client
    {
        [Key]
        [Column("ClientId")]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ClientName")]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [Column("SecretHash")]
        public string SecretHash { get; set; } = string.Empty;

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("LastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [MaxLength(255)]
        [Column("ContactEmail")]
        public string ContactEmail { get; set; } = string.Empty;

        [Column("RateLimitPerMinute")]
        public int RateLimitPerMinute { get; set; } = 1000;

        // Navigation properties
        public virtual ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();
        public virtual ICollection<ClientConfiguration> Configurations { get; set; } = new List<ClientConfiguration>();
        public virtual ICollection<TransformationMapping> TransformationMappings { get; set; } = new List<TransformationMapping>();
    }
}