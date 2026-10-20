namespace BTL_WEB.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public bool IsAuthenticated { get; set; } = false;
        public string FullName { get; set; } = "Khách";
        public string AvatarLetter { get; set; } = "K";
        public string GoalDisplay { get; set; } = "Giảm mỡ tự nhiên";
        public int StreakDays { get; set; } = 1;
        public int UnreadNotificationCount { get; set; } = 0;

        // Calo
        public int TargetCalories { get; set; } = 1850;
        public int ConsumedCalories { get; set; } = 0;
        public int BurnedCalories { get; set; } = 0;
        public int RemainingCalories => Math.Max(0, (TargetCalories - ConsumedCalories) + BurnedCalories);
        public double ProgressPercent => TargetCalories > 0 ? Math.Min(100.0, Math.Round(((double)ConsumedCalories / TargetCalories) * 100.0, 1)) : 0;

        // Macros
        public double TotalProtein { get; set; } = 0;
        public double TargetProtein { get; set; } = 108;
        public double TotalCarbs { get; set; } = 0;
        public double TargetCarbs { get; set; } = 163;
        public double TotalFat { get; set; } = 0;
        public double TargetFat { get; set; } = 40;

        // Quick cards meta
        public int FridgeItemCount { get; set; } = 0;
        public int WaterMl { get; set; } = 0;

        // Danh sách món đã ăn hôm nay
        public List<TodayMealItemDto> TodayLoggedItems { get; set; } = new List<TodayMealItemDto>();

        // Danh sách bài tập đã ghi nhận hôm nay
        public List<TodayWorkoutDto> TodayWorkouts { get; set; } = new List<TodayWorkoutDto>();
    }

    public class TodayMealItemDto
    {
        public int Id { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public string ServingSize { get; set; } = "1 phần";
        public double Quantity { get; set; } = 1.0;
        public int Calories { get; set; }
        public string MealType { get; set; } = "Breakfast";
    }

    public class TodayWorkoutDto
    {
        public int Id { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string ActivityIcon { get; set; } = "🏃";
        public string Intensity { get; set; } = "MODERATE";
        public int DurationMinutes { get; set; }
        public int BurnedCalories { get; set; }
        public string LoggedTime { get; set; } = string.Empty;
    }
}
