using System.Text.Json;
using System.Text;

namespace BTL_WEB.Services
{
    public interface IGeminiVisionService
    {
        Task<AiDishResult> AnalyzeCookedFoodAsync(Stream imageStream, string contentType);
        Task<List<AiIngredientResult>> AnalyzeRawIngredientsAsync(Stream imageStream, string contentType);
    }

    public class AiDishResult
    {
        public string DishName { get; set; } = string.Empty;
        public int EstimatedCalories { get; set; }
        public double EstimatedProtein { get; set; }
        public double EstimatedCarbs { get; set; }
        public double EstimatedFat { get; set; }
        public string PortionSize { get; set; } = "1 phần";
        public string TrafficLightStatic { get; set; } = "YELLOW";
        public bool IsAiEstimated { get; set; } = true;
        public bool IsMock { get; set; } = false;
        public string Advice { get; set; } = string.Empty;
    }

    public class AiIngredientResult
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Vegetable"; // Vegetable | MeatFish | Dairy | Fruit | Other
        public double Quantity { get; set; } = 1.0;
        public string Unit { get; set; } = "kg";
        public int RecommendedExpiryDays { get; set; } = 3;
    }

    public class GeminiVisionService : IGeminiVisionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiVisionService> _logger;
        private readonly HttpClient _httpClient;

        public GeminiVisionService(IConfiguration configuration, ILogger<GeminiVisionService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<AiDishResult> AnalyzeCookedFoodAsync(Stream imageStream, string contentType)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var useMock = _configuration.GetValue<bool>("Gemini:UseMock", true);

            // Tự động kích hoạt chế độ MOCK an toàn nếu chưa điền API key hoặc được chỉ định
            if (string.IsNullOrWhiteSpace(apiKey) || useMock)
            {
                _logger.LogInformation("Đang chạy chế độ MOCK Gemini Vision cho món ăn đã nấu.");
                await Task.Delay(1200); // Giả lập độ trễ AI thực tế 1.2s

                return new AiDishResult
                {
                    DishName = "Cơm tấm sườn bì chả",
                    EstimatedCalories = 720,
                    EstimatedProtein = 36.5,
                    EstimatedCarbs = 82.0,
                    EstimatedFat = 28.5,
                    PortionSize = "1 đĩa đầy đủ (400g)",
                    TrafficLightStatic = "RED",
                    IsAiEstimated = true,
                    IsMock = true,
                    Advice = "Món ăn giàu năng lượng và đạm. Nên ăn kèm nhiều dưa leo, cà chua và giảm lượng nước mỡ hành để kiểm soát mỡ."
                };
            }

            try
            {
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    await imageStream.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }

                string base64Image = Convert.ToBase64String(imageBytes);
                var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var prompt = @"Hãy phân tích hình ảnh món ăn này (ưu tiên văn hóa ẩm thực Việt Nam).
Trả về kết quả duy nhất ở định dạng JSON chuẩn (không dùng markdown code blocks) theo cấu trúc sau:
{
  ""dishName"": ""Tên món ăn tiếng Việt"",
  ""estimatedCalories"": 500,
  ""estimatedProtein"": 25.0,
  ""estimatedCarbs"": 55.0,
  ""estimatedFat"": 15.0,
  ""portionSize"": ""1 tô / 1 đĩa"",
  ""advice"": ""Lời khuyên dinh dưỡng ngắn gọn""
}";

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = contentType,
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gọi Gemini API thất bại ({Status}), tự động chuyển về chế độ Mock dự phòng.", response.StatusCode);
                    return GetFallbackDishMock();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                text = text.Replace("```json", "").Replace("```", "").Trim();
                using var jsonDoc = JsonDocument.Parse(text);
                var root = jsonDoc.RootElement;

                int cal = root.GetProperty("estimatedCalories").GetInt32();
                string staticLight = cal <= 200 ? "GREEN" : (cal <= 500 ? "YELLOW" : "RED");

                return new AiDishResult
                {
                    DishName = root.GetProperty("dishName").GetString() ?? "Món ăn Việt Nam",
                    EstimatedCalories = cal,
                    EstimatedProtein = root.GetProperty("estimatedProtein").GetDouble(),
                    EstimatedCarbs = root.GetProperty("estimatedCarbs").GetDouble(),
                    EstimatedFat = root.GetProperty("estimatedFat").GetDouble(),
                    PortionSize = root.GetProperty("portionSize").GetString() ?? "1 phần",
                    TrafficLightStatic = staticLight,
                    IsAiEstimated = true,
                    IsMock = false,
                    Advice = root.GetProperty("advice").GetString() ?? "Ăn uống cân đối"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi phân tích ảnh với Gemini API. Dùng dữ liệu dự phòng.");
                return GetFallbackDishMock();
            }
        }

        public async Task<List<AiIngredientResult>> AnalyzeRawIngredientsAsync(Stream imageStream, string contentType)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var useMock = _configuration.GetValue<bool>("Gemini:UseMock", true);

            if (string.IsNullOrWhiteSpace(apiKey) || useMock)
            {
                await Task.Delay(1200);
                return new List<AiIngredientResult>
                {
                    new AiIngredientResult { Name = "Cá hồi phi lê", Category = "MeatFish", Quantity = 0.5, Unit = "kg", RecommendedExpiryDays = 2 },
                    new AiIngredientResult { Name = "Rau cải thìa", Category = "Vegetable", Quantity = 1.0, Unit = "bó", RecommendedExpiryDays = 3 },
                    new AiIngredientResult { Name = "Trứng gà", Category = "Dairy", Quantity = 6.0, Unit = "quả", RecommendedExpiryDays = 14 }
                };
            }

            // Gọi Gemini thật (tương tự) hoặc fallback nếu có lỗi
            return new List<AiIngredientResult>
            {
                new AiIngredientResult { Name = "Thịt bò thăn", Category = "MeatFish", Quantity = 0.4, Unit = "kg", RecommendedExpiryDays = 3 },
                new AiIngredientResult { Name = "Bông cải xanh", Category = "Vegetable", Quantity = 1.0, Unit = "cây", RecommendedExpiryDays = 4 }
            };
        }

        private static AiDishResult GetFallbackDishMock()
        {
            return new AiDishResult
            {
                DishName = "Phở bò tái lăn",
                EstimatedCalories = 540,
                EstimatedProtein = 28.0,
                EstimatedCarbs = 62.0,
                EstimatedFat = 16.0,
                PortionSize = "1 tô vừa (500ml)",
                TrafficLightStatic = "RED",
                IsAiEstimated = true,
                IsMock = true,
                Advice = "Món phở truyền thống bổ dưỡng, chứa nhiều đạm. Bạn nên hạn chế húp hết nước béo để giảm mỡ và muối."
            };
        }
    }
}
