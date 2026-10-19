using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("WeightLogs")]
    public class WeightLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Range(20, 300, ErrorMessage = "Cân nặng từ 20kg đến 300kg")]
        public double WeightKg { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? Note { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
