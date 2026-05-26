using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents transformation mapping rules for client data
    /// </summary>
    [Table("TRANSFORMATION_MAPPING")]
    public class TransformationMapping
    {
        [Key]
        [Column("MappingId")]
        public int MappingId { get; set; }

        [Required]
        [Column("ClientId")]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("SourceFormat")]
        public string SourceFormat { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("TargetFormat")]
        public string TargetFormat { get; set; } = string.Empty;

        [Required]
        [Column("MappingRules")]
        public string MappingRules { get; set; } = string.Empty;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;
    }
}