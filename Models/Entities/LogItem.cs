using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("LogItems")]
    public class LogItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DailyLogId { get; set; }

        [Required]
        public int FoodId { get; set; }

        [Range(0.1, 50.0, ErrorMessage = "Số lượng/khẩu phần từ 0.1 đến 50")]
        public double Quantity { get; set; } = 1.0;

        public int CalculatedCalories { get; set; }
        public double CalculatedProtein { get; set; }
        public double CalculatedCarbs { get; set; }
        public double CalculatedFat { get; set; }

        public bool IsAiEstimated { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("DailyLogId")]
        public virtual DailyLog? DailyLog { get; set; }

        [ForeignKey("FoodId")]
        public virtual Food? Food { get; set; }
    }
}
