using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents error log entries for tracking system errors
    /// </summary>
    [Table("ERROR_LOG")]
    public class ErrorLog
    {
        [Key]
        [Column("ErrorId")]
        public long ErrorId { get; set; }

        [Column("RequestId")]
        public long? RequestId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ErrorType")]
        public string ErrorType { get; set; } = string.Empty;

        [Required]
        [Column("ErrorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;

        [Column("StackTrace")]
        public string? StackTrace { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Source")]
        public string Source { get; set; } = string.Empty;

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        [Column("Severity")]
        public string Severity { get; set; } = "Error";

        // Navigation properties
        [ForeignKey("RequestId")]
        public virtual RequestLog? RequestLog { get; set; }
    }
}