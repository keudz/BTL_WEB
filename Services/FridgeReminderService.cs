using Microsoft.EntityFrameworkCore;
using BTL_WEB.Data;
using BTL_WEB.Models.Entities;

namespace BTL_WEB.Services
{
    public class FridgeReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FridgeReminderService> _logger;

        public FridgeReminderService(IServiceProvider serviceProvider, ILogger<FridgeReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FridgeReminderService (Cron Task) đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiringFridgeItems();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình quét tủ lạnh ngầm.");
                }

                // Chạy định kỳ mỗi 6 tiếng một lần (hoặc 1 ngày)
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        public async Task CheckExpiringFridgeItems()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            var warningThreshold = now.AddHours(48); // Điều kiện nghiệp vụ: Hạn dùng <= 48 giờ

            // Quét các nguyên liệu chưa tiêu thụ và sắp hết hạn trong 48 giờ tới
            var expiringItems = await context.FridgeItems
                .Where(fi => !fi.IsConsumed && fi.ExpiryDate <= warningThreshold)
                .ToListAsync();

            foreach (var item in expiringItems)
            {
                // Tránh gửi trùng lặp nếu hôm nay đã tạo thông báo rồi (trong vòng 24h)
                if (item.LastNotifiedAt.HasValue && (now - item.LastNotifiedAt.Value).TotalHours < 24)
                {
                    continue;
                }

                var hoursLeft = (int)Math.Max(0, (item.ExpiryDate - now).TotalHours);
                string urgency = hoursLeft == 0 ? "đã hết hạn" : $"chỉ còn {hoursLeft} giờ";

                string recipeSuggestion = GenerateQuickRecipe(item.ItemName, item.Category);

                var notification = new InAppNotification
                {
                    UserId = item.UserId,
                    Title = $"🚨 Cảnh báo hạn dùng: {item.ItemName} ({urgency})",
                    Message = $"Nguyên liệu '{item.ItemName}' ({item.Quantity} {item.Unit}) sắp hết hạn sử dụng. Gợi ý chế biến nhanh: {recipeSuggestion}",
                    Type = "FRIDGE_EXPIRY",
                    IsRead = false,
                    CreatedAt = now
                };

                context.Notifications.Add(notification);
                item.LastNotifiedAt = now;
            }

            await context.SaveChangesAsync();
        }

        private static string GenerateQuickRecipe(string itemName, string category)
        {
            var lower = itemName.ToLower();
            if (lower.Contains("cá") || lower.Contains("hồi"))
            {
                return "Cá áp chảo sốt bơ tỏi ăn kèm salad hoặc nấu canh chua thanh mát.";
            }
            if (lower.Contains("thịt") || lower.Contains("gà"))
            {
                return "Ức gà xào ớt chuông hoặc luộc xé phay chấm muối tiêu chanh.";
            }
            if (lower.Contains("rau") || lower.Contains("cải"))
            {
                return "Luộc chấm kho quẹt hoặc xào tỏi nhanh trong 5 phút.";
            }
            if (lower.Contains("trứng"))
            {
                return "Trứng cuộn rau củ hoặc ốp la lòng đào kèm bánh mì ngũ cốc.";
            }
            return "Nấu súp thập cẩm hoặc làm món xào nhanh để tối ưu dinh dưỡng và chống lãng phí!";
        }
    }
}
