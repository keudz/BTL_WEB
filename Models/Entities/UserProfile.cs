using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("UserProfiles")]
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = "Male"; // Male | Female

        [Range(10, 120, ErrorMessage = "Tuổi phải từ 10 đến 120")]
        public int Age { get; set; } = 25;

        [Range(50, 250, ErrorMessage = "Chiều cao phải từ 50cm đến 250cm")]
        public double HeightCm { get; set; } = 170;

        [Range(20, 300, ErrorMessage = "Cân nặng phải từ 20kg đến 300kg")]
        public double WeightKg { get; set; } = 65;

        [Range(1.0, 2.5)]
        public double ActivityLevel { get; set; } = 1.375; // 1.2 (Ít), 1.375 (Nhẹ), 1.55 (Vừa), 1.725 (Nhiều), 1.9 (Rất nhiều)

        [Required]
        [StringLength(20)]
        public string GoalType { get; set; } = "Maintain"; // LoseWeight | Maintain | GainWeight

        [Range(1, 8, ErrorMessage = "Số bữa trong ngày từ 1 đến 8")]
        public int MealsPerDay { get; set; } = 3;

        [Range(500, 10000)]
        public int TargetCalories { get; set; } = 2000; // Giá trị dẫn xuất tự động tính toán

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
