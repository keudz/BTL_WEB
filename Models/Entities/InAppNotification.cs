using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("Notifications")]
    public class InAppNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "SYSTEM"; // FRIDGE_EXPIRY | RED_WARNING | RECIPE_SUGGESTION | SYSTEM

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
