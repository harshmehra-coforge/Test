using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents a processing rule used for validation and transformation
    /// </summary>
    [Table("PROCESSING_RULE")]
    public class ProcessingRule
    {
        [Key]
        [Column("RuleId")]
        public int RuleId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("RuleName")]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("RuleType")]
        public string RuleType { get; set; } = string.Empty;

        [Required]
        [Column("Condition")]
        public string Condition { get; set; } = string.Empty;

        [Required]
        [Column("Action")]
        public string Action { get; set; } = string.Empty;

        [Column("Priority")]
        public int Priority { get; set; } = 100;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        [Column("CreatedBy")]
        public string CreatedBy { get; set; } = string.Empty;
    }
}