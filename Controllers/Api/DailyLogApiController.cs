using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_WEB.Data;
using BTL_WEB.Models.Entities;
using BTL_WEB.Services;

namespace BTL_WEB.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DailyLogApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITrafficLightEngine _trafficLightEngine;

        public DailyLogApiController(AppDbContext context, ITrafficLightEngine trafficLightEngine)
        {
            _context = context;
            _trafficLightEngine = trafficLightEngine;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        // GET: /api/dailylogapi/summary?date=2026-10-14
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string? date = null)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            DateTime targetDate = DateTime.UtcNow.Date;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
            {
                targetDate = parsedDate.Date;
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            int targetDailyCalories = userProfile?.TargetCalories ?? 2000;
            int mealsPerDay = userProfile?.MealsPerDay ?? 3;

            // Lấy danh sách 4 bữa trong ngày của user
            var logs = await _context.DailyLogs
                .Include(d => d.LogItems)
                    .ThenInclude(li => li.Food)
                .Where(d => d.UserId == userId && d.LogDate.Date == targetDate)
                .ToListAsync();

            // Định nghĩa các mốc thời gian ăn uống trong ngày theo Schema
            var timelineSlots = new[]
            {
                new { Type = "Breakfast", Time = "07:00", Name = "Bữa sáng sớm" },
                new { Type = "MorningCoffee", Time = "08:30", Name = "Cà phê & ăn nhẹ" },
                new { Type = "MorningSnack", Time = "10:00", Name = "Bữa phụ sáng" },
                new { Type = "Lunch", Time = "12:00", Name = "Bữa trưa" },
                new { Type = "AfternoonSnack", Time = "14:30", Name = "Bữa phụ chiều" },
                new { Type = "PreWorkout", Time = "16:30", Name = "Năng lượng trước tập" },
                new { Type = "Dinner", Time = "18:30", Name = "Bữa tối" },
                new { Type = "LateSnack", Time = "20:30", Name = "Ăn nhẹ tối" }
            };

            var mealsResult = new List<object>();

            int totalConsumedCalories = 0;
            double totalProtein = 0;
            double totalCarbs = 0;
            double totalFat = 0;
            int redMealCalories = 0;

            foreach (var slot in timelineSlots)
            {
                // Cho phép map cả "Snack" vào "AfternoonSnack" để tương thích dữ liệu cũ
                var log = logs.FirstOrDefault(l => l.MealType == slot.Type || (slot.Type == "AfternoonSnack" && l.MealType == "Snack"));
                int mealCalories = log?.TotalMealCalories ?? 0;
                totalConsumedCalories += mealCalories;

                string dynamicLight = _trafficLightEngine.GetDynamicMealTrafficLight(mealCalories, targetDailyCalories, mealsPerDay);

                if (dynamicLight == "RED")
                {
                    redMealCalories += mealCalories;
                }

                double slotProtein = 0;
                double slotCarbs = 0;
                double slotFat = 0;

                var items = log != null 
                    ? log.LogItems.Select(li => (object)new
                    {
                        li.Id,
                        li.FoodId,
                        FoodName = li.Food?.Name ?? "Món không xác định",
                        ServingSize = li.Food?.ServingSize ?? "1 phần",
                        li.Quantity,
                        li.CalculatedCalories,
                        li.CalculatedProtein,
                        li.CalculatedCarbs,
                        li.CalculatedFat,
                        li.IsAiEstimated,
                        FoodStaticLight = li.Food?.TrafficLightStatic ?? "GREEN"
                    }).ToList()
                    : new List<object>();

                if (log != null)
                {
                    slotProtein = Math.Round(log.LogItems.Sum(i => i.CalculatedProtein), 1);
                    slotCarbs = Math.Round(log.LogItems.Sum(i => i.CalculatedCarbs), 1);
                    slotFat = Math.Round(log.LogItems.Sum(i => i.CalculatedFat), 1);

                    totalProtein += slotProtein;
                    totalCarbs += slotCarbs;
                    totalFat += slotFat;
                }

                mealsResult.Add(new
                {
                    mealType = slot.Type,
                    mealTime = slot.Time,
                    mealName = slot.Name,
                    mealCalories,
                    protein = slotProtein,
                    carbs = slotCarbs,
                    fat = slotFat,
                    mealTrafficLightDynamic = dynamicLight,
                    items
                });
            }

            int remainingCalories = targetDailyCalories - totalConsumedCalories;
            double progressPercent = targetDailyCalories > 0 
                ? Math.Min(100.0, Math.Round(((double)totalConsumedCalories / targetDailyCalories) * 100.0, 1)) 
                : 0;

            // Kiểm tra cảnh báo tỷ lệ món/bữa Đỏ trong ngày (> 40%)
            bool isRedWarning = _trafficLightEngine.IsRedRatioExceeded(redMealCalories, totalConsumedCalories, 40.0);

            return Ok(new
            {
                success = true,
                date = targetDate.ToString("yyyy-MM-dd"),
                targetDailyCalories,
                totalConsumedCalories,
                remainingCalories,
                progressPercent,
                totalProtein = Math.Round(totalProtein, 1),
                totalCarbs = Math.Round(totalCarbs, 1),
                totalFat = Math.Round(totalFat, 1),
                isRedWarning,
                meals = mealsResult
            });
        }

        // POST: /api/dailylogapi/add-item
        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromBody] AddLogItemDto dto)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (!DateTime.TryParse(dto.Date, out DateTime logDate))
            {
                logDate = DateTime.UtcNow.Date;
            }
            logDate = logDate.Date;

            var food = await _context.Foods.FindAsync(dto.FoodId);
            if (food == null)
            {
                return BadRequest(new { success = false, message = "Không tìm thấy món ăn" });
            }

            if (dto.Quantity <= 0) dto.Quantity = 1.0;

            // Tìm hoặc tạo mới DailyLog cho bữa ăn
            var dailyLog = await _context.DailyLogs
                .Include(d => d.LogItems)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.LogDate.Date == logDate && d.MealType == dto.MealType);

            if (dailyLog == null)
            {
                dailyLog = new DailyLog
                {
                    UserId = userId,
                    LogDate = logDate,
                    MealType = dto.MealType,
                    TotalMealCalories = 0,
                    MealTrafficLightDynamic = "GREEN"
                };
                _context.DailyLogs.Add(dailyLog);
                await _context.SaveChangesAsync();
            }

            // Tính toán calo theo số lượng khẩu phần
            int calcCalories = (int)Math.Round(food.Calories * dto.Quantity);
            double calcProtein = Math.Round(food.Protein * dto.Quantity, 1);
            double calcCarbs = Math.Round(food.Carbs * dto.Quantity, 1);
            double calcFat = Math.Round(food.Fat * dto.Quantity, 1);

            var logItem = new LogItem
            {
                DailyLogId = dailyLog.Id,
                FoodId = food.Id,
                Quantity = dto.Quantity,
                CalculatedCalories = calcCalories,
                CalculatedProtein = calcProtein,
                CalculatedCarbs = calcCarbs,
                CalculatedFat = calcFat,
                IsAiEstimated = dto.IsAiEstimated,
                CreatedAt = DateTime.UtcNow
            };

            _context.LogItems.Add(logItem);
            await _context.SaveChangesAsync();

            // Tính lại tổng calo bữa ăn & Đèn giao thông động
            await RecalculateDailyLog(dailyLog.Id, userId);

            return await GetSummary(logDate.ToString("yyyy-MM-dd"));
        }

        // DELETE: /api/dailylogapi/remove-item/{id}
        [HttpDelete("remove-item/{id}")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var item = await _context.LogItems
                .Include(li => li.DailyLog)
                .FirstOrDefaultAsync(li => li.Id == id && li.DailyLog!.UserId == userId);

            if (item == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy món ăn cần xóa" });
            }

            int dailyLogId = item.DailyLogId;
            var logDate = item.DailyLog!.LogDate;

            _context.LogItems.Remove(item);
            await _context.SaveChangesAsync();

            // Tính lại tổng calo bữa ăn & Đèn giao thông động
            await RecalculateDailyLog(dailyLogId, userId);

            return await GetSummary(logDate.ToString("yyyy-MM-dd"));
        }

        private async Task RecalculateDailyLog(int dailyLogId, int userId)
        {
            var log = await _context.DailyLogs
                .Include(d => d.LogItems)
                .FirstOrDefaultAsync(d => d.Id == dailyLogId);

            if (log != null)
            {
                log.TotalMealCalories = log.LogItems.Sum(li => li.CalculatedCalories);

                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                int targetDailyCalories = profile?.TargetCalories ?? 2000;
                int mealsPerDay = profile?.MealsPerDay ?? 3;

                log.MealTrafficLightDynamic = _trafficLightEngine.GetDynamicMealTrafficLight(log.TotalMealCalories, targetDailyCalories, mealsPerDay);

                await _context.SaveChangesAsync();
            }
        }

        private static string GetMealVietnameseName(string mealType) => mealType switch
        {
            "Breakfast" => "Bữa Sáng",
            "Lunch" => "Bữa Trưa",
            "Dinner" => "Bữa Tối",
            "Snack" => "Ăn Vặt / Phụ",
            _ => mealType
        };

        public class AddLogItemDto
        {
            public string Date { get; set; } = string.Empty;
            public string MealType { get; set; } = "Breakfast";
            public int FoodId { get; set; }
            public double Quantity { get; set; } = 1.0;
            public bool IsAiEstimated { get; set; } = false;
        }
    }
}
