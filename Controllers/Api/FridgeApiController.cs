using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_WEB.Data;
using BTL_WEB.Models.Entities;

namespace BTL_WEB.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FridgeApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FridgeApiController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        // GET: /api/fridgeapi/items?category=Vegetable&compartment=FREEZER
        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] string? category = null, [FromQuery] string? compartment = null)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var query = _context.FridgeItems
                .Where(fi => fi.UserId == userId && !fi.IsConsumed);

            if (!string.IsNullOrWhiteSpace(category) && category.ToUpper() != "ALL")
            {
                query = query.Where(fi => fi.Category == category);
            }

            var items = await query
                .OrderBy(fi => fi.ExpiryDate)
                .ToListAsync();

            var now = DateTime.UtcNow;

            var allMapped = items.Select(fi =>
            {
                var diff = fi.ExpiryDate - now;
                var totalHours = diff.TotalHours;
                var totalDays = (int)Math.Ceiling(diff.TotalDays);

                string status = "FRESH";
                string statusText = $"Còn {totalDays} ngày";
                string badgeClass = "badge bg-success-subtle text-success";

                if (totalHours < 0)
                {
                    status = "EXPIRED";
                    statusText = "Đã quá hạn!";
                    badgeClass = "badge bg-danger text-white";
                }
                else if (totalHours <= 48)
                {
                    status = "EXPIRING_SOON";
                    statusText = totalHours <= 24 ? $"Chỉ còn {(int)totalHours} giờ!" : "Hạn còn < 48 giờ!";
                    badgeClass = "badge bg-warning text-dark fw-bold border border-warning";
                }

                // Nhận diện ngăn mát vs ngăn đá
                bool isFreezer = fi.Category == "FREEZER" || fi.Category == "MeatFish";
                string compType = isFreezer ? "FREEZER" : "COOLER";
                string compName = isFreezer ? "Ngăn Đá" : "Ngăn Mát";
                string compBadge = isFreezer ? "❄️ Đá" : "🥬 Mát";
                string compBadgeClass = isFreezer ? "badge bg-info-subtle text-primary border" : "badge bg-success-subtle text-success border";

                return new
                {
                    fi.Id,
                    fi.ItemName,
                    fi.Category,
                    fi.Quantity,
                    fi.Unit,
                    PurchasedDate = fi.PurchasedDate.ToString("yyyy-MM-dd"),
                    ExpiryDate = fi.ExpiryDate.ToString("yyyy-MM-dd"),
                    fi.EstimatedCost,
                    totalHours = Math.Round(totalHours, 1),
                    totalDays,
                    status,
                    statusText,
                    badgeClass,
                    compartment = compType,
                    compartmentName = compName,
                    compartmentBadge = compBadge,
                    compartmentBadgeClass = compBadgeClass
                };
            }).ToList();

            var coolerCount = allMapped.Count(i => i.compartment == "COOLER");
            var freezerCount = allMapped.Count(i => i.compartment == "FREEZER");
            var expiringSoonCount = allMapped.Count(r => r.status == "EXPIRING_SOON");
            var expiredCount = allMapped.Count(r => r.status == "EXPIRED");

            // Lọc theo ngăn nếu có
            var filteredItems = allMapped;
            if (!string.IsNullOrWhiteSpace(compartment) && compartment.ToUpper() != "ALL")
            {
                filteredItems = allMapped.Where(i => i.compartment.Equals(compartment, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(new
            {
                success = true,
                totalItems = allMapped.Count,
                coolerCount,
                freezerCount,
                expiringSoonCount,
                expiredCount,
                items = filteredItems
            });
        }

        // POST: /api/fridgeapi/add
        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromBody] AddFridgeItemDto dto)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.ItemName))
            {
                return BadRequest(new { success = false, message = "Tên nguyên liệu không được để trống" });
            }

            DateTime expiry;
            if (dto.ExpiryDays.HasValue && dto.ExpiryDays.Value > 0)
            {
                expiry = DateTime.UtcNow.AddDays(dto.ExpiryDays.Value);
            }
            else if (DateTime.TryParse(dto.ExpiryDateString, out DateTime parsedDate))
            {
                expiry = parsedDate;
            }
            else
            {
                expiry = DateTime.UtcNow.AddDays(3); // Mặc định 3 ngày
            }

            var item = new FridgeItem
            {
                UserId = userId,
                ItemName = dto.ItemName.Trim(),
                Category = string.IsNullOrWhiteSpace(dto.Category) ? "Other" : dto.Category,
                Quantity = dto.Quantity > 0 ? dto.Quantity : 1.0,
                Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "phần" : dto.Unit.Trim(),
                PurchasedDate = DateTime.UtcNow,
                ExpiryDate = expiry,
                EstimatedCost = dto.EstimatedCost >= 0 ? dto.EstimatedCost : 0,
                IsConsumed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.FridgeItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã thêm '{item.ItemName}' vào tủ lạnh ảo!" });
        }

        // PUT: /api/fridgeapi/swap-compartment/{id}
        [HttpPut("swap-compartment/{id}")]
        public async Task<IActionResult> SwapCompartment(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var item = await _context.FridgeItems.FirstOrDefaultAsync(fi => fi.Id == id && fi.UserId == userId);
            if (item == null) return NotFound(new { success = false, message = "Không tìm thấy nguyên liệu" });

            bool currentlyFreezer = item.Category == "FREEZER" || item.Category == "MeatFish";
            item.Category = currentlyFreezer ? "COOLER" : "FREEZER";
            await _context.SaveChangesAsync();

            string newName = currentlyFreezer ? "Ngăn Mát" : "Ngăn Đá";
            return Ok(new { success = true, newCompartment = item.Category, message = $"Đã chuyển '{item.ItemName}' sang {newName}!" });
        }

        // GET: /api/fridgeapi/suggest-recipes
        [HttpGet("suggest-recipes")]
        public async Task<IActionResult> SuggestRecipes()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var fridgeItems = await _context.FridgeItems
                .Where(fi => fi.UserId == userId && !fi.IsConsumed)
                .OrderBy(fi => fi.ExpiryDate)
                .ToListAsync();

            if (!fridgeItems.Any())
            {
                return Ok(new
                {
                    success = false,
                    message = "Tủ lạnh hiện chưa có nguyên liệu nào. Hãy thêm thực phẩm hoặc quét hóa đơn để AI gợi ý thực đơn!"
                });
            }

            var itemNames = fridgeItems.Select(f => f.ItemName).ToList();

            // Sinh công thức chế biến dựa trên danh sách nguyên liệu THẬT của người dùng
            var recipes = new List<object>();

            var primaryItem = fridgeItems.First();
            var secondaryItem = fridgeItems.Count > 1 ? fridgeItems[1] : null;

            recipes.Add(new
            {
                recipeTitle = $"{primaryItem.ItemName} Áp Chảo Thanh Đạm",
                calories = 320,
                protein = 28.5,
                carbs = 8.0,
                fat = 12.0,
                cookingTimeMinutes = 15,
                difficulty = "Dễ",
                trafficLight = "GREEN",
                usedIngredients = secondaryItem != null 
                    ? $"{primaryItem.ItemName} ({primaryItem.Quantity} {primaryItem.Unit}), {secondaryItem.ItemName}"
                    : $"{primaryItem.ItemName} ({primaryItem.Quantity} {primaryItem.Unit})",
                instructions = $"1. Rửa sạch và thái miếng vừa ăn {primaryItem.ItemName}.\n2. Ướp cùng 1 chút tiêu, muối hạt và dầu olive trong 5 phút.\n3. Áp chảo trên lửa vừa 4 phút mỗi mặt cho chín vàng tới, giữ trọn độ ẩm và dinh dưỡng.",
                reason = $"Nguyên liệu '{primaryItem.ItemName}' đang được ưu tiên dùng sớm nhất để đảm bảo độ tươi ngon và chống lãng phí."
            });

            if (secondaryItem != null)
            {
                recipes.Add(new
                {
                    recipeTitle = $"Canh Dinh Dưỡng {secondaryItem.ItemName} & {primaryItem.ItemName}",
                    calories = 210,
                    protein = 18.0,
                    carbs = 14.5,
                    fat = 4.0,
                    cookingTimeMinutes = 20,
                    difficulty = "Rất dễ",
                    trafficLight = "GREEN",
                    usedIngredients = $"{secondaryItem.ItemName}, {primaryItem.ItemName}",
                    instructions = $"1. Đun sôi 600ml nước dùng thanh đạm.\n2. Cho {primaryItem.ItemName} vào ninh nhẹ 10 phút.\n3. Thêm {secondaryItem.ItemName}, nêm gia vị nhẹ tay và rắc hành ngò thơm trước khi thưởng thức.",
                    reason = "Bữa ăn ít dầu mỡ, hỗ trợ kiểm soát calo và giải phóng 2 nguyên liệu trong tủ lạnh."
                });
            }

            return Ok(new
            {
                success = true,
                totalRecipes = recipes.Count,
                fridgeIngredients = itemNames,
                recipes
            });
        }

        // GET: /api/fridgeapi/notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    CreatedAt = n.CreatedAt.ToString("HH:mm dd/MM/yyyy")
                })
                .ToListAsync();

            return Ok(new { success = true, notifications });
        }

        public class AddFridgeItemDto
        {
            public string ItemName { get; set; } = string.Empty;
            public string Category { get; set; } = "Other";
            public double Quantity { get; set; } = 1.0;
            public string Unit { get; set; } = "kg";
            public int? ExpiryDays { get; set; }
            public string? ExpiryDateString { get; set; }
            public decimal EstimatedCost { get; set; } = 0;
        }
    }
}
