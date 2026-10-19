using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(255)]
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Free"; // Pro | Free

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual UserProfile? Profile { get; set; }
        public virtual ICollection<DailyLog> DailyLogs { get; set; } = new List<DailyLog>();
        public virtual ICollection<FridgeItem> FridgeItems { get; set; } = new List<FridgeItem>();
        public virtual ICollection<WeightLog> WeightLogs { get; set; } = new List<WeightLog>();
        public virtual ICollection<InAppNotification> Notifications { get; set; } = new List<InAppNotification>();
    }
}
