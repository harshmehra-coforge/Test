using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents client-specific configuration settings
    /// </summary>
    [Table("CLIENT_CONFIGURATION")]
    public class ClientConfiguration
    {
        [Key]
        [Column("ConfigId")]
        public int ConfigId { get; set; }

        [Required]
        [Column("ClientId")]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ConfigKey")]
        public string ConfigKey { get; set; } = string.Empty;

        [Required]
        [Column("ConfigValue")]
        public string ConfigValue { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("DataType")]
        public string DataType { get; set; } = "String";

        [Column("IsEncrypted")]
        public bool IsEncrypted { get; set; } = false;

        [Column("LastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;
    }
}