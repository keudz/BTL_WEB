using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_WEB.Data;
using BTL_WEB.Models.Entities;
using BTL_WEB.Services;

namespace BTL_WEB.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITrafficLightEngine _trafficLightEngine;

        public FoodApiController(AppDbContext context, ITrafficLightEngine trafficLightEngine)
        {
            _context = context;
            _trafficLightEngine = trafficLightEngine;
        }

        // GET: /api/foodapi/search?query=pho&trafficLight=GREEN&page=1&pageSize=10
        // Thỏa mãn tiêu chí: Case-insensitive, Paging, Filtering của giảng viên
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? query = null,
            [FromQuery] string? trafficLight = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var foodsQuery = _context.Foods.AsQueryable();

            // 1. Tìm kiếm không phân biệt hoa/thường (case-insensitive)
            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                foodsQuery = foodsQuery.Where(f => f.Name.ToLower().Contains(q));
            }

            // 2. Lọc theo Đèn Giao Thông tĩnh (Filtering)
            if (!string.IsNullOrWhiteSpace(trafficLight) && trafficLight.ToUpper() != "ALL")
            {
                var tl = trafficLight.Trim().ToUpper();
                foodsQuery = foodsQuery.Where(f => f.TrafficLightStatic == tl);
            }

            var totalItems = await foodsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // 3. Phân trang (Paging)
            var items = await foodsQuery
                .OrderBy(f => f.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.ServingSize,
                    f.Calories,
                    f.Protein,
                    f.Carbs,
                    f.Fat,
                    f.TrafficLightStatic,
                    f.IsCustom
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                totalItems,
                totalPages,
                currentPage = page,
                pageSize,
                items
            });
        }

        // POST: /api/foodapi/custom
        // Cho phép người dùng tự tạo món mới nếu trong DB chưa có
        [HttpPost("custom")]
        public async Task<IActionResult> CreateCustomFood([FromBody] CustomFoodDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = 0;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int uid))
            {
                userId = uid;
            }

            // Tự động gán Đèn giao thông tĩnh dựa trên số calo
            string staticLight = _trafficLightEngine.GetStaticTrafficLight(dto.Calories);

            var food = new Food
            {
                Name = dto.Name.Trim(),
                ServingSize = string.IsNullOrWhiteSpace(dto.ServingSize) ? "1 phần" : dto.ServingSize.Trim(),
                Calories = dto.Calories,
                Protein = dto.Protein,
                Carbs = dto.Carbs,
                Fat = dto.Fat,
                TrafficLightStatic = staticLight,
                IsCustom = true,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Đã tạo món ăn thành công",
                food = new
                {
                    food.Id,
                    food.Name,
                    food.ServingSize,
                    food.Calories,
                    food.Protein,
                    food.Carbs,
                    food.Fat,
                    food.TrafficLightStatic
                }
            });
        }

        public class CustomFoodDto
        {
            public string Name { get; set; } = string.Empty;
            public string ServingSize { get; set; } = "1 phần";
            public int Calories { get; set; }
            public double Protein { get; set; }
            public double Carbs { get; set; }
            public double Fat { get; set; }
        }
    }
}
