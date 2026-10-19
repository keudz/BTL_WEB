namespace BTL_WEB.Services
{
    public interface ITdeeCalculatorService
    {
        double CalculateBmr(string gender, double weightKg, double heightCm, int age);
        double CalculateTdee(double bmr, double activityLevel);
        int CalculateTargetCalories(double tdee, string goalType);
    }

    public class TdeeCalculatorService : ITdeeCalculatorService
    {
        public double CalculateBmr(string gender, double weightKg, double heightCm, int age)
        {
            // Công thức Mifflin-St Jeor
            if (string.Equals(gender, "Female", StringComparison.OrdinalIgnoreCase))
            {
                return (10 * weightKg) + (6.25 * heightCm) - (5 * age) - 161;
            }
            // Mặc định là Male
            return (10 * weightKg) + (6.25 * heightCm) - (5 * age) + 5;
        }

        public double CalculateTdee(double bmr, double activityLevel)
        {
            if (activityLevel < 1.0) activityLevel = 1.2;
            return bmr * activityLevel;
        }

        public int CalculateTargetCalories(double tdee, string goalType)
        {
            double target = tdee;

            switch (goalType?.ToLowerInvariant())
            {
                case "loseweight":
                case "giamcan":
                    target = tdee - 500; // Thâm hụt 500 calo/ngày để giảm ~0.5kg/tuần
                    if (target < 1200) target = 1200; // Ngưỡng an toàn tối thiểu
                    break;

                case "gainweight":
                case "tangcan":
                    target = tdee + 300; // Thặng dư 300 calo/ngày
                    break;

                case "maintain":
                case "duytri":
                default:
                    target = tdee;
                    break;
            }

            return (int)Math.Round(target);
        }
    }
}
