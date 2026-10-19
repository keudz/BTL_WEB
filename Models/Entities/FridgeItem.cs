using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_WEB.Models.Entities
{
    [Table("FridgeItems")]
    public class FridgeItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? FoodId { get; set; }

        [Required(ErrorMessage = "Tên thực phẩm không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thực phẩm tối đa 100 ký tự")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = "Other"; // Vegetable | MeatFish | Dairy | Fruit | Other

        [Range(0.01, 1000.0, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public double Quantity { get; set; } = 1.0;

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(20)]
        public string Unit { get; set; } = "kg"; // kg | g | bó | hộp | quả | túi

        public DateTime PurchasedDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Hạn sử dụng không được để trống")]
        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Ước tính giá tiền không hợp lệ")]
        public decimal EstimatedCost { get; set; } = 0;

        public DateTime? LastNotifiedAt { get; set; }

        public bool IsConsumed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("FoodId")]
        public virtual Food? Food { get; set; }
    }
}
