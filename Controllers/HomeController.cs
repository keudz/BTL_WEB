using BTL_WEB.Data;
using BTL_WEB.Models;
using BTL_WEB.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BTL_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeDashboardViewModel();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                model.IsAuthenticated = true;
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId) && userId > 0)
                {
                    var user = await _context.Users
                        .Include(u => u.Profile)
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        model.FullName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Username;
                        model.AvatarLetter = !string.IsNullOrWhiteSpace(model.FullName) ? model.FullName.Substring(0, 1).ToUpper() : "U";

                        if (user.Profile != null)
                        {
                            model.TargetCalories = user.Profile.TargetCalories > 0 ? user.Profile.TargetCalories : 1850;
                            model.GoalDisplay = user.Profile.GoalType switch
                            {
                                "LoseWeight" => "Giảm mỡ tự nhiên",
                                "Maintain" => "Duy trì vóc dáng",
                                "GainWeight" or "MuscleGain" => "Tăng cơ săn chắc",
                                _ => "Giảm mỡ tự nhiên"
                            };

                            // Tính mục tiêu macro dựa trên TDEE
                            model.TargetProtein = Math.Round((model.TargetCalories * 0.25) / 4.0, 0);
                            model.TargetCarbs = Math.Round((model.TargetCalories * 0.45) / 4.0, 0);
                            model.TargetFat = Math.Round((model.TargetCalories * 0.30) / 9.0, 0);
                        }

                        // Lấy các bữa ăn hôm nay
                        var today = DateTime.UtcNow.Date;
                        var dailyLogs = await _context.DailyLogs
                            .Include(d => d.LogItems)
                                .ThenInclude(li => li.Food)
                            .Where(d => d.UserId == userId && d.LogDate.Date == today)
                            .ToListAsync();

                        var allItems = dailyLogs.SelectMany(d => d.LogItems.Select(li => new TodayMealItemDto
                        {
                            Id = li.Id,
                            FoodName = li.Food?.Name ?? "Món đã ghi",
                            ServingSize = li.Food?.ServingSize ?? "1 phần",
                            Quantity = li.Quantity,
                            Calories = li.CalculatedCalories,
                            MealType = d.MealType
                        })).ToList();

                        model.TodayLoggedItems = allItems;
                        model.ConsumedCalories = dailyLogs.Sum(d => d.TotalMealCalories);
                        model.TotalProtein = Math.Round(dailyLogs.SelectMany(d => d.LogItems).Sum(li => li.CalculatedProtein), 1);
                        model.TotalCarbs = Math.Round(dailyLogs.SelectMany(d => d.LogItems).Sum(li => li.CalculatedCarbs), 1);
                        model.TotalFat = Math.Round(dailyLogs.SelectMany(d => d.LogItems).Sum(li => li.CalculatedFat), 1);

                        // Lấy danh sách bài tập vận động đã ghi hôm nay
                        var workoutsToday = await _context.WorkoutLogs
                            .Where(w => w.UserId == userId && w.LoggedAt.Date == today)
                            .OrderByDescending(w => w.LoggedAt)
                            .ToListAsync();

                        model.TodayWorkouts = workoutsToday.Select(w => new TodayWorkoutDto
                        {
                            Id = w.Id,
                            ActivityName = w.ActivityName,
                            ActivityIcon = w.ActivityIcon,
                            Intensity = w.Intensity,
                            DurationMinutes = w.DurationMinutes,
                            BurnedCalories = w.BurnedCalories,
                            LoggedTime = w.LoggedAt.ToString("HH:mm")
                        }).ToList();

                        model.BurnedCalories = workoutsToday.Sum(w => w.BurnedCalories);

                        // Đếm số lượng thực phẩm trong tủ lạnh
                        model.FridgeItemCount = await _context.FridgeItems
                            .CountAsync(fi => fi.UserId == userId && !fi.IsConsumed);

                        // Thông báo chưa đọc
                        model.UnreadNotificationCount = await _context.Notifications
                            .CountAsync(n => n.UserId == userId && !n.IsRead);

                        // Chuỗi ngày ghi nhận
                        model.StreakDays = 1;
                    }
                }
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
