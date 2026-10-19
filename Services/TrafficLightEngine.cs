namespace BTL_WEB.Services
{
    public interface ITrafficLightEngine
    {
        string GetStaticTrafficLight(int calories);
        string GetDynamicMealTrafficLight(int mealCalories, int targetDailyCalories, int mealsPerDay);
        bool IsRedRatioExceeded(int redCaloriesOrCount, int totalCaloriesOrCount, double thresholdPercent = 40.0);
    }

    public class TrafficLightEngine : ITrafficLightEngine
    {
        // 1. Phân loại Đèn TĨNH gắn với từng món ăn
        public string GetStaticTrafficLight(int calories)
        {
            if (calories <= 200) return "GREEN";
            if (calories <= 500) return "YELLOW";
            return "RED";
        }

        // 2. Phân loại Đèn ĐỘNG theo từng bữa ăn dựa trên % Target Calories và Số bữa/ngày
        public string GetDynamicMealTrafficLight(int mealCalories, int targetDailyCalories, int mealsPerDay)
        {
            if (targetDailyCalories <= 0) targetDailyCalories = 2000;
            if (mealsPerDay <= 0) mealsPerDay = 3;

            double mealPercentage = ((double)mealCalories / targetDailyCalories) * 100.0;

            switch (mealsPerDay)
            {
                case 1: // OMAD (1 bữa/ngày)
                    if (mealPercentage <= 60.0) return "GREEN";
                    if (mealPercentage <= 90.0) return "YELLOW";
                    return "RED";

                case 2: // 2 bữa/ngày
                    if (mealPercentage <= 40.0) return "GREEN";
                    if (mealPercentage <= 65.0) return "YELLOW";
                    return "RED";

                case 3: // 3 bữa/ngày (phổ biến)
                    if (mealPercentage <= 25.0) return "GREEN";
                    if (mealPercentage <= 40.0) return "YELLOW";
                    return "RED";

                case 4: // 4 bữa/ngày
                    if (mealPercentage <= 20.0) return "GREEN";
                    if (mealPercentage <= 32.0) return "YELLOW";
                    return "RED";

                case 5: // 5 bữa/ngày
                    if (mealPercentage <= 16.0) return "GREEN";
                    if (mealPercentage <= 26.0) return "YELLOW";
                    return "RED";

                default: // 6 bữa trở lên
                    if (mealPercentage <= 13.0) return "GREEN";
                    if (mealPercentage <= 20.0) return "YELLOW";
                    return "RED";
            }
        }

        // 3. Kiểm tra xem tỷ lệ món Đỏ trong ngày có vượt ngưỡng an toàn không
        public bool IsRedRatioExceeded(int redCaloriesOrCount, int totalCaloriesOrCount, double thresholdPercent = 40.0)
        {
            if (totalCaloriesOrCount <= 0) return false;
            double ratio = ((double)redCaloriesOrCount / totalCaloriesOrCount) * 100.0;
            return ratio > thresholdPercent;
        }
    }
}
