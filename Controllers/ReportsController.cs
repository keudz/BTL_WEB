using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BTL_WEB.Data;

namespace BTL_WEB.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        // GET: /Reports/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Reports/GetChartData?days=7
        [HttpGet]
        public async Task<IActionResult> GetChartData(int days = 7)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (days != 7 && days != 30) days = 7;

            var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
            var endDate = DateTime.UtcNow.Date;

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            int targetCal = profile?.TargetCalories ?? 2000;

            // 1. Dữ liệu Calo trong N ngày gần nhất
            var dailyLogs = await _context.DailyLogs
                .Where(d => d.UserId == userId && d.LogDate >= startDate && d.LogDate <= endDate)
                .GroupBy(d => d.LogDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalCalories = g.Sum(d => d.TotalMealCalories)
                })
                .ToListAsync();

            // 2. Dữ liệu Cân nặng
            var weightLogs = await _context.WeightLogs
                .Where(w => w.UserId == userId && w.RecordedAt >= startDate)
                .OrderBy(w => w.RecordedAt)
                .Select(w => new
                {
                    Date = w.RecordedAt.Date,
                    w.WeightKg
                })
                .ToListAsync();

            var labels = new List<string>();
            var consumedData = new List<int>();
            var targetData = new List<int>();
            var weightData = new List<double?>();

            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                labels.Add(d.ToString("dd/MM"));

                var log = dailyLogs.FirstOrDefault(l => l.Date == d);
                consumedData.Add(log?.TotalCalories ?? 0);
                targetData.Add(targetCal);

                var weight = weightLogs.LastOrDefault(w => w.Date <= d);
                weightData.Add(weight?.WeightKg ?? profile?.WeightKg);
            }

            return Json(new
            {
                success = true,
                labels,
                consumedData,
                targetData,
                weightData
            });
        }
    }
}
