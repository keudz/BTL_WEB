using System.ComponentModel.DataAnnotations;

namespace BTL_WEB.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Gói tài khoản")]
        public string Role { get; set; } = "Free";

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = "Male";

        [Range(10, 120, ErrorMessage = "Tuổi phải từ 10 đến 120")]
        [Display(Name = "Tuổi")]
        public int Age { get; set; } = 25;

        [Range(50, 250, ErrorMessage = "Chiều cao từ 50cm đến 250cm")]
        [Display(Name = "Chiều cao (cm)")]
        public double HeightCm { get; set; } = 170;

        [Range(20, 300, ErrorMessage = "Cân nặng từ 20kg đến 300kg")]
        [Display(Name = "Cân nặng (kg)")]
        public double WeightKg { get; set; } = 65;

        [Range(1.0, 2.5)]
        [Display(Name = "Mức độ vận động")]
        public double ActivityLevel { get; set; } = 1.375;

        [Required]
        [Display(Name = "Mục tiêu thể chất")]
        public string GoalType { get; set; } = "Maintain";

        [Range(1, 8, ErrorMessage = "Số bữa trong ngày từ 1 đến 8 bữa")]
        [Display(Name = "Số bữa ăn/ngày")]
        public int MealsPerDay { get; set; } = 3;

        [Display(Name = "Calo mục tiêu mỗi ngày (kcal)")]
        public int TargetCalories { get; set; }

        [Display(Name = "Chỉ số BMR (Năng lượng nghỉ ngơi)")]
        public int Bmr { get; set; }

        [Display(Name = "Chỉ số TDEE (Năng lượng tiêu hao hàng ngày)")]
        public int Tdee { get; set; }

        [Display(Name = "Chỉ số BMI")]
        public double Bmi { get; set; }

        [Display(Name = "Phân loại thể trạng")]
        public string BmiCategory { get; set; } = "Bình thường";

        [Display(Name = "Màu sắc thể trạng")]
        public string BmiColorClass { get; set; } = "success";

        [Display(Name = "Tỷ lệ mỡ cơ thể ước tính (%)")]
        public double EstimatedBodyFat { get; set; }
    }
}
