using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BTL_WEB.Data;
using BTL_WEB.Services;

namespace BTL_WEB.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiVisionApiController : ControllerBase
    {
        private readonly IGeminiVisionService _geminiVisionService;
        private readonly AppDbContext _context;

        public AiVisionApiController(IGeminiVisionService geminiVisionService, AppDbContext context)
        {
            _geminiVisionService = geminiVisionService;
            _context = context;
        }

        // POST: /api/aivisionapi/scan-dish
        // Nhận diện mâm cơm/món ăn đã nấu (Chỉ dành cho User Pro VIP)
        [Authorize(Roles = "Pro")]
        [HttpPost("scan-dish")]
        public async Task<IActionResult> ScanDish(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn hoặc chụp một bức ảnh món ăn." });
            }

            try
            {
                using var stream = imageFile.OpenReadStream();
                var aiResult = await _geminiVisionService.AnalyzeCookedFoodAsync(stream, imageFile.ContentType);

                // Nghiệp vụ: Cố gắng khớp tên món từ AI với Food DB (case-insensitive)
                var dishNameNormalized = aiResult.DishName.Trim().ToLower();
                var matchedFood = await _context.Foods
                    .FirstOrDefaultAsync(f => f.Name.ToLower().Contains(dishNameNormalized) || dishNameNormalized.Contains(f.Name.ToLower()));

                bool isDbMatched = matchedFood != null;

                return Ok(new
                {
                    success = true,
                    dishName = isDbMatched ? matchedFood!.Name : aiResult.DishName,
                    calories = isDbMatched ? matchedFood!.Calories : aiResult.EstimatedCalories,
                    protein = isDbMatched ? matchedFood!.Protein : aiResult.EstimatedProtein,
                    carbs = isDbMatched ? matchedFood!.Carbs : aiResult.EstimatedCarbs,
                    fat = isDbMatched ? matchedFood!.Fat : aiResult.EstimatedFat,
                    servingSize = isDbMatched ? matchedFood!.ServingSize : aiResult.PortionSize,
                    trafficLightStatic = isDbMatched ? matchedFood!.TrafficLightStatic : aiResult.TrafficLightStatic,
                    isDbMatched,
                    isAiEstimated = !isDbMatched,
                    foodId = matchedFood?.Id ?? 0,
                    isMock = aiResult.IsMock,
                    advice = aiResult.Advice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xử lý ảnh qua AI.", error = ex.Message });
            }
        }

        // POST: /api/aivisionapi/scan-ingredients
        // Nhận diện nguyên liệu mua về vào tủ lạnh (Chỉ dành cho User Pro VIP)
        [Authorize(Roles = "Pro")]
        [HttpPost("scan-ingredients")]
        public async Task<IActionResult> ScanIngredients(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn hoặc chụp ảnh nguyên liệu." });
            }

            try
            {
                using var stream = imageFile.OpenReadStream();
                var ingredients = await _geminiVisionService.AnalyzeRawIngredientsAsync(stream, imageFile.ContentType);

                return Ok(new
                {
                    success = true,
                    totalFound = ingredients.Count,
                    ingredients
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi nhận diện nguyên liệu.", error = ex.Message });
            }
        }
    }
}
