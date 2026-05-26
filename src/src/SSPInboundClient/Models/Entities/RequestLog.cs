using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSPInboundClient.Models.Entities
{
    /// <summary>
    /// Represents a log entry for each request processed by the system
    /// </summary>
    [Table("REQUEST_LOG")]
    public class RequestLog
    {
        [Key]
        [Column("RequestId")]
        public long RequestId { get; set; }

        [Required]
        [Column("ClientId")]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("RequestPath")]
        public string RequestPath { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Column("HttpMethod")]
        public string HttpMethod { get; set; } = string.Empty;

        [Column("RequestBody")]
        public string? RequestBody { get; set; }

        [Column("ResponseBody")]
        public string? ResponseBody { get; set; }

        [Column("StatusCode")]
        public int StatusCode { get; set; }

        [Column("RequestTimestamp")]
        public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;

        [Column("ResponseTimestamp")]
        public DateTime? ResponseTimestamp { get; set; }

        [Column("ProcessingTimeMs")]
        public int? ProcessingTimeMs { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("CorrelationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("UserAgent")]
        public string? UserAgent { get; set; }

        [MaxLength(45)]
        [Column("IpAddress")]
        public string? IpAddress { get; set; }

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;
        public virtual ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();
    }
}