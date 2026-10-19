using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("DailyLogs")]
    public class DailyLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime LogDate { get; set; } // Ngày ghi nhật ký (chỉ tính ngày)

        [Required]
        [StringLength(20)]
        public string MealType { get; set; } = "Breakfast"; // Breakfast | Lunch | Dinner | Snack

        public int TotalMealCalories { get; set; } = 0;

        [Required]
        [StringLength(10)]
        public string MealTrafficLightDynamic { get; set; } = "GREEN"; // GREEN | YELLOW | RED (Tính theo % target calo & mealsPerDay)

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<LogItem> LogItems { get; set; } = new List<LogItem>();
    }
}
