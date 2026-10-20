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
    public class WorkoutApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorkoutApiController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        // Định nghĩa 12 môn thể thao phổ biến kèm Icon và hệ số MET theo 3 mức cường độ
        public static readonly List<SportActivityDef> SportDefinitions = new()
        {
            new SportActivityDef { Key = "running", Name = "Chạy bộ", Icon = "🏃", LightMET = 6.0, ModerateMET = 8.5, HighMET = 11.5, UnitDesc = "km/h" },
            new SportActivityDef { Key = "cycling", Name = "Đạp xe", Icon = "🚴", LightMET = 4.0, ModerateMET = 7.0, HighMET = 10.0, UnitDesc = "km/h" },
            new SportActivityDef { Key = "swimming", Name = "Bơi lội", Icon = "🏊", LightMET = 5.0, ModerateMET = 7.5, HighMET = 10.5, UnitDesc = "bơi sải/bướm" },
            new SportActivityDef { Key = "jumping_rope", Name = "Nhảy dây", Icon = "🪢", LightMET = 7.5, ModerateMET = 10.0, HighMET = 12.5, UnitDesc = "nhịp/phút" },
            new SportActivityDef { Key = "gym", Name = "Tập Gym & Tạ", Icon = "🏋️", LightMET = 3.5, ModerateMET = 5.5, HighMET = 8.0, UnitDesc = "kháng lực" },
            new SportActivityDef { Key = "walking", Name = "Đi bộ nhanh", Icon = "🚶", LightMET = 2.8, ModerateMET = 4.0, HighMET = 5.5, UnitDesc = "bước chân" },
            new SportActivityDef { Key = "badminton", Name = "Cầu lông", Icon = "🏸", LightMET = 4.5, ModerateMET = 6.0, HighMET = 8.5, UnitDesc = "giao lưu/đơn" },
            new SportActivityDef { Key = "football", Name = "Đá bóng", Icon = "⚽", LightMET = 5.0, ModerateMET = 7.5, HighMET = 10.0, UnitDesc = "sân 5-7-11" },
            new SportActivityDef { Key = "basketball", Name = "Bóng rổ", Icon = "🏀", LightMET = 4.5, ModerateMET = 6.5, HighMET = 9.0, UnitDesc = "ném rổ/đấu" },
            new SportActivityDef { Key = "yoga", Name = "Yoga & Giãn cơ", Icon = "🧘", LightMET = 2.5, ModerateMET = 3.8, HighMET = 5.0, UnitDesc = "thân tâm" },
            new SportActivityDef { Key = "boxing", Name = "Boxing & Võ thuật", Icon = "🥊", LightMET = 5.5, ModerateMET = 7.8, HighMET = 11.0, UnitDesc = "đối kháng/bao cát" },
            new SportActivityDef { Key = "stair_climbing", Name = "Leo cầu thang", Icon = "🧗", LightMET = 4.5, ModerateMET = 7.5, HighMET = 10.5, UnitDesc = "dốc/bậc" }
        };

        // GET: /api/workoutapi/sports
        [AllowAnonymous]
        [HttpGet("sports")]
        public async Task<IActionResult> GetSports()
        {
            int userId = GetCurrentUserId();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            double userWeight = profile?.WeightKg > 0 ? profile.WeightKg : 65.0;

            return Ok(new
            {
                success = true,
                userWeight,
                sports = SportDefinitions
            });
        }

        // POST: /api/workoutapi/calculate
        [AllowAnonymous]
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculateWorkoutDto dto)
        {
            int userId = GetCurrentUserId();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            double weight = dto.WeightKg.HasValue && dto.WeightKg.Value > 20 
                ? dto.WeightKg.Value 
                : (profile?.WeightKg > 0 ? profile.WeightKg : 65.0);

            var sport = SportDefinitions.FirstOrDefault(s => s.Key.Equals(dto.SportKey, StringComparison.OrdinalIgnoreCase))
                        ?? SportDefinitions.First();

            double met = dto.Intensity?.ToUpper() switch
            {
                "LIGHT" => sport.LightMET,
                "HIGH" => sport.HighMET,
                _ => sport.ModerateMET
            };

            int minutes = Math.Max(1, dto.DurationMinutes);

            // Công thức chuẩn Y học thể thao: Calories = MET * Weight(kg) * (Minutes / 60)
            int burned = (int)Math.Round(met * weight * (minutes / 60.0));

            return Ok(new
            {
                success = true,
                sportName = sport.Name,
                sportIcon = sport.Icon,
                intensity = dto.Intensity ?? "MODERATE",
                durationMinutes = minutes,
                userWeight = weight,
                metValue = met,
                burnedCalories = burned,
                formula = $"{met} (MET) × {weight} kg × ({minutes} / 60 giờ) = {burned} kcal"
            });
        }

        // POST: /api/workoutapi/log
        [HttpPost("log")]
        public async Task<IActionResult> LogWorkout([FromBody] LogWorkoutRequest dto)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            double weight = profile?.WeightKg > 0 ? profile.WeightKg : 65.0;

            var sport = SportDefinitions.FirstOrDefault(s => s.Key.Equals(dto.SportKey, StringComparison.OrdinalIgnoreCase))
                        ?? SportDefinitions.First();

            double met = dto.Intensity?.ToUpper() switch
            {
                "LIGHT" => sport.LightMET,
                "HIGH" => sport.HighMET,
                _ => sport.ModerateMET
            };

            int minutes = Math.Max(1, dto.DurationMinutes);
            int burned = (int)Math.Round(met * weight * (minutes / 60.0));

            var workout = new WorkoutLog
            {
                UserId = userId,
                ActivityName = sport.Name,
                ActivityIcon = sport.Icon,
                Intensity = dto.Intensity?.ToUpper() ?? "MODERATE",
                DurationMinutes = minutes,
                BurnedCalories = burned,
                LoggedAt = DateTime.UtcNow
            };

            _context.WorkoutLogs.Add(workout);
            await _context.SaveChangesAsync();

            // Tính tổng calo đốt hôm nay
            var today = DateTime.UtcNow.Date;
            var totalBurnedToday = await _context.WorkoutLogs
                .Where(w => w.UserId == userId && w.LoggedAt.Date == today)
                .SumAsync(w => w.BurnedCalories);

            return Ok(new
            {
                success = true,
                message = $"Đã ghi nhận bài tập {sport.Icon} {sport.Name} đốt {burned} kcal!",
                workout = new
                {
                    workout.Id,
                    workout.ActivityName,
                    workout.ActivityIcon,
                    workout.Intensity,
                    workout.DurationMinutes,
                    workout.BurnedCalories,
                    LoggedTime = workout.LoggedAt.ToString("HH:mm")
                },
                totalBurnedToday
            });
        }

        // GET: /api/workoutapi/today
        [HttpGet("today")]
        public async Task<IActionResult> GetTodayWorkouts()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var today = DateTime.UtcNow.Date;
            var workouts = await _context.WorkoutLogs
                .Where(w => w.UserId == userId && w.LoggedAt.Date == today)
                .OrderByDescending(w => w.LoggedAt)
                .Select(w => new
                {
                    w.Id,
                    w.ActivityName,
                    w.ActivityIcon,
                    w.Intensity,
                    IntensityDesc = w.Intensity == "LIGHT" ? "Nhẹ" : (w.Intensity == "HIGH" ? "Cường độ cao" : "Vừa phải"),
                    w.DurationMinutes,
                    w.BurnedCalories,
                    LoggedTime = w.LoggedAt.ToString("HH:mm")
                })
                .ToListAsync();

            int totalBurned = workouts.Sum(w => w.BurnedCalories);

            return Ok(new
            {
                success = true,
                totalBurned,
                count = workouts.Count,
                workouts
            });
        }

        // DELETE: /api/workoutapi/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var workout = await _context.WorkoutLogs.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
            if (workout == null) return NotFound(new { success = false, message = "Không tìm thấy bài tập." });

            _context.WorkoutLogs.Remove(workout);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa bài tập vận động." });
        }

        public class SportActivityDef
        {
            public string Key { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Icon { get; set; } = "🏃";
            public double LightMET { get; set; }
            public double ModerateMET { get; set; }
            public double HighMET { get; set; }
            public string UnitDesc { get; set; } = string.Empty;
        }

        public class CalculateWorkoutDto
        {
            public string SportKey { get; set; } = "running";
            public string Intensity { get; set; } = "MODERATE";
            public int DurationMinutes { get; set; } = 30;
            public double? WeightKg { get; set; }
        }

        public class LogWorkoutRequest
        {
            public string SportKey { get; set; } = "running";
            public string Intensity { get; set; } = "MODERATE";
            public int DurationMinutes { get; set; } = 30;
        }
    }
}
