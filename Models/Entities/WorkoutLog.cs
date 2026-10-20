using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("WorkoutLogs")]
    public class WorkoutLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        [StringLength(50)]
        public string ActivityIcon { get; set; } = "🏃";

        [StringLength(50)]
        public string Intensity { get; set; } = "MODERATE"; // LIGHT | MODERATE | HIGH

        public int DurationMinutes { get; set; }

        public int BurnedCalories { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
