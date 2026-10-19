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

        // GET: /api/fridgeapi/items?category=Vegetable
        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] string? category = null)
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

            var result = items.Select(fi =>
            {
                var diff = fi.ExpiryDate - now;
                var totalHours = diff.TotalHours;
                var totalDays = (int)Math.Ceiling(diff.TotalDays);

                string status = "FRESH"; // Còn tươi
                string statusText = $"Còn {totalDays} ngày";
                string badgeClass = "badge bg-success-subtle text-success";

                if (totalHours < 0)
                {
                    status = "EXPIRED";
                    statusText = "Đã quá hạn!";
                    badgeClass = "badge bg-danger text-white";
                }
                else if (totalHours <= 48) // <= 48h kích hoạt cảnh báo
                {
                    status = "EXPIRING_SOON";
                    statusText = totalHours <= 24 ? $"Chỉ còn {(int)totalHours} giờ!" : "Hạn còn < 48 giờ!";
                    badgeClass = "badge bg-warning text-dark fw-bold border border-warning";
                }

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
                    badgeClass
                };
            }).ToList();

            var expiringSoonCount = result.Count(r => r.status == "EXPIRING_SOON" || r.status == "EXPIRED");

            return Ok(new
            {
                success = true,
                totalItems = result.Count,
                expiringSoonCount,
                items = result
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

        // PUT: /api/fridgeapi/consume/{id}
        [HttpPut("consume/{id}")]
        public async Task<IActionResult> ConsumeItem(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var item = await _context.FridgeItems.FirstOrDefaultAsync(fi => fi.Id == id && fi.UserId == userId);
            if (item == null) return NotFound(new { success = false, message = "Không tìm thấy nguyên liệu" });

            item.IsConsumed = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã đánh dấu đã dùng xong '{item.ItemName}'!" });
        }

        // DELETE: /api/fridgeapi/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var item = await _context.FridgeItems.FirstOrDefaultAsync(fi => fi.Id == id && fi.UserId == userId);
            if (item == null) return NotFound(new { success = false, message = "Không tìm thấy nguyên liệu" });

            _context.FridgeItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa nguyên liệu khỏi tủ lạnh." });
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
