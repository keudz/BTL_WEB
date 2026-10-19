using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("Foods")]
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống")]
        [StringLength(150, ErrorMessage = "Tên món ăn tối đa 150 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khẩu phần không được để trống")]
        [StringLength(50)]
        public string ServingSize { get; set; } = "1 phần"; // VD: 1 tô, 1 đĩa, 100g

        [Range(0, 5000, ErrorMessage = "Calo phải từ 0 đến 5000 kcal")]
        public int Calories { get; set; }

        [Range(0, 500, ErrorMessage = "Chất đạm (Protein) từ 0 đến 500g")]
        public double Protein { get; set; }

        [Range(0, 500, ErrorMessage = "Tinh bột (Carbs) từ 0 đến 500g")]
        public double Carbs { get; set; }

        [Range(0, 500, ErrorMessage = "Chất béo (Fat) từ 0 đến 500g")]
        public double Fat { get; set; }

        [Required]
        [StringLength(10)]
        public string TrafficLightStatic { get; set; } = "GREEN"; // GREEN | YELLOW | RED

        public bool IsCustom { get; set; } = false;

        public int CreatedByUserId { get; set; } = 0; // 0 = Hệ thống / Thư viện chuẩn

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<LogItem> LogItems { get; set; } = new List<LogItem>();
    }
}
