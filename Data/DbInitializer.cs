using BTL_WEB.Models.Entities;
using BCrypt.Net;

namespace BTL_WEB.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Tự động tạo cơ sở dữ liệu nếu chưa tồn tại
            context.Database.EnsureCreated();

            // Nếu đã có User thì coi như đã seed
            if (context.Users.Any())
            {
                return;
            }

            // 1. SEED USERS & PROFILES (Pro User & Free User)
            var proUser = new User
            {
                Username = "prouser",
                Email = "pro@kit.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pro@123"),
                FullName = "Nguyễn Hoàng Nam (Pro VIP)",
                Role = "Pro",
                CreatedAt = DateTime.UtcNow
            };

            var freeUser = new User
            {
                Username = "freeuser",
                Email = "user@kit.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Trần Thị Mai (Free User)",
                Role = "Free",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(proUser, freeUser);
            context.SaveChanges();

            // Seed profiles
            var proProfile = new UserProfile
            {
                UserId = proUser.Id,
                Gender = "Male",
                Age = 24,
                HeightCm = 172,
                WeightKg = 68,
                ActivityLevel = 1.55,
                GoalType = "LoseWeight",
                MealsPerDay = 3,
                TargetCalories = 1850 // TDEE - 500
            };

            var freeProfile = new UserProfile
            {
                UserId = freeUser.Id,
                Gender = "Female",
                Age = 21,
                HeightCm = 160,
                WeightKg = 52,
                ActivityLevel = 1.375,
                GoalType = "Maintain",
                MealsPerDay = 3,
                TargetCalories = 1650
            };

            context.UserProfiles.AddRange(proProfile, freeProfile);

            // 2. SEED 35+ MÓN ĂN VIỆT NAM CHUẨN ĐẦY ĐỦ CALO, MACRO & ĐÈN GIAO THÔNG TĨNH
            // TrafficLightStatic: <=200: GREEN, 200-500: YELLOW, >500: RED
            var foods = new List<Food>
            {
                // === NHÓM 🟢 XANH (Thanh đạm, ít calo, ăn thoải mái) ===
                new Food { Name = "Salad ức gà sốt mè rang", ServingSize = "1 đĩa (200g)", Calories = 185, Protein = 24.5, Carbs = 8.2, Fat = 5.8, TrafficLightStatic = "GREEN" },
                new Food { Name = "Trứng luộc", ServingSize = "1 quả (55g)", Calories = 78, Protein = 6.3, Carbs = 0.6, Fat = 5.3, TrafficLightStatic = "GREEN" },
                new Food { Name = "Canh cải thịt băm", ServingSize = "1 bát tô (250ml)", Calories = 95, Protein = 8.5, Carbs = 4.0, Fat = 4.5, TrafficLightStatic = "GREEN" },
                new Food { Name = "Canh chua cá lóc (nước trong)", ServingSize = "1 bát tô (300ml)", Calories = 145, Protein = 16.0, Carbs = 9.0, Fat = 3.5, TrafficLightStatic = "GREEN" },
                new Food { Name = "Rau muống luộc chấm tương", ServingSize = "1 đĩa (150g)", Calories = 45, Protein = 3.2, Carbs = 6.5, Fat = 0.5, TrafficLightStatic = "GREEN" },
                new Food { Name = "Gỏi cuốn tôm thịt", ServingSize = "1 cuốn (80g)", Calories = 85, Protein = 5.5, Carbs = 11.0, Fat = 1.8, TrafficLightStatic = "GREEN" },
                new Food { Name = "Ức gà áp chảo tiêu xanh", ServingSize = "100g", Calories = 165, Protein = 31.0, Carbs = 0.5, Fat = 3.6, TrafficLightStatic = "GREEN" },
                new Food { Name = "Khoai lang luộc", ServingSize = "1 củ vừa (120g)", Calories = 105, Protein = 2.0, Carbs = 24.0, Fat = 0.2, TrafficLightStatic = "GREEN" },
                new Food { Name = "Súp gà ngô non", ServingSize = "1 bát con (200ml)", Calories = 135, Protein = 11.2, Carbs = 14.0, Fat = 3.2, TrafficLightStatic = "GREEN" },
                new Food { Name = "Canh rong biển đậu hũ non", ServingSize = "1 bát tô (250ml)", Calories = 80, Protein = 6.5, Carbs = 5.0, Fat = 3.0, TrafficLightStatic = "GREEN" },
                new Food { Name = "Dưa hấu tráng miệng", ServingSize = "1 đĩa (200g)", Calories = 60, Protein = 1.2, Carbs = 15.0, Fat = 0.3, TrafficLightStatic = "GREEN" },

                // === NHÓM 🟡 VÀNG (Calo trung bình, bữa ăn chính tiêu chuẩn 200 - 500 kcal) ===
                new Food { Name = "Cơm trắng", ServingSize = "1 bát con (150g)", Calories = 200, Protein = 4.2, Carbs = 44.5, Fat = 0.4, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Cơm gạo lứt", ServingSize = "1 bát con (150g)", Calories = 170, Protein = 4.5, Carbs = 36.0, Fat = 1.5, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Bánh mì ốp la (1 trứng + dưa chuột)", ServingSize = "1 ổ (140g)", Calories = 310, Protein = 11.0, Carbs = 42.0, Fat = 10.5, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Cháo gà hạt sen", ServingSize = "1 tô vừa (350ml)", Calories = 320, Protein = 18.5, Carbs = 48.0, Fat = 5.2, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Thịt lợn nạc kho tiêu", ServingSize = "1 đĩa nhỏ (120g)", Calories = 240, Protein = 26.0, Carbs = 3.5, Fat = 12.0, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Đậu sốt cà chua", ServingSize = "1 đĩa (180g)", Calories = 210, Protein = 13.0, Carbs = 9.0, Fat = 13.5, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Bún mọc sườn non", ServingSize = "1 tô (450ml)", Calories = 420, Protein = 22.0, Carbs = 56.0, Fat = 12.0, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Phở gà ta (nước trong, không mỡ gà)", ServingSize = "1 tô vừa (450ml)", Calories = 430, Protein = 26.0, Carbs = 58.0, Fat = 8.5, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Cá basa kho tộ", ServingSize = "1 khúc (130g)", Calories = 260, Protein = 21.0, Carbs = 6.0, Fat = 16.0, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Trứng chiên hành tây cà chua", ServingSize = "2 quả (140g)", Calories = 220, Protein = 13.5, Carbs = 4.5, Fat = 16.0, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Bò xào bông cải xanh", ServingSize = "1 đĩa (200g)", Calories = 285, Protein = 25.0, Carbs = 8.0, Fat = 15.0, TrafficLightStatic = "YELLOW" },
                new Food { Name = "Bún chả cá Đà Nẵng", ServingSize = "1 tô (450ml)", Calories = 410, Protein = 20.0, Carbs = 55.0, Fat = 11.0, TrafficLightStatic = "YELLOW" },

                // === NHÓM 🔴 ĐỎ (Calo cao > 500 kcal, dầu mỡ/đường cần kiểm soát) ===
                new Food { Name = "Phở bò tái nạm gầu", ServingSize = "1 tô đặc biệt (550ml)", Calories = 580, Protein = 32.0, Carbs = 68.0, Fat = 20.0, TrafficLightStatic = "RED" },
                new Food { Name = "Cơm tấm sườn bì chả trứng ốp la", ServingSize = "1 đĩa đầy đủ (400g)", Calories = 780, Protein = 38.0, Carbs = 85.0, Fat = 32.0, TrafficLightStatic = "RED" },
                new Food { Name = "Bún đậu mắm tôm thập cẩm", ServingSize = "1 mẹt đầy đủ (450g)", Calories = 720, Protein = 34.0, Carbs = 70.0, Fat = 33.0, TrafficLightStatic = "RED" },
                new Food { Name = "Bún bò Huế giò heo", ServingSize = "1 tô lớn (550ml)", Calories = 650, Protein = 35.0, Carbs = 62.0, Fat = 27.0, TrafficLightStatic = "RED" },
                new Food { Name = "Bún chả Hà Nội (thịt nướng than)", ServingSize = "1 suất đầy đủ (400g)", Calories = 620, Protein = 29.0, Carbs = 65.0, Fat = 26.0, TrafficLightStatic = "RED" },
                new Food { Name = "Cơm rang dưa bò", ServingSize = "1 đĩa (350g)", Calories = 690, Protein = 24.0, Carbs = 82.0, Fat = 28.0, TrafficLightStatic = "RED" },
                new Food { Name = "Bánh mì pate xúc xích chả lụa", ServingSize = "1 ổ thập cẩm (200g)", Calories = 520, Protein = 19.0, Carbs = 54.0, Fat = 24.0, TrafficLightStatic = "RED" },
                new Food { Name = "Mì xào giòn hải sản thịt heo", ServingSize = "1 đĩa lớn (400g)", Calories = 740, Protein = 28.0, Carbs = 78.0, Fat = 34.0, TrafficLightStatic = "RED" },
                new Food { Name = "Trà sữa trân châu đường đen (500ml)", ServingSize = "1 ly lớn (Size L)", Calories = 510, Protein = 3.5, Carbs = 88.0, Fat = 16.0, TrafficLightStatic = "RED" },
                new Food { Name = "Gà rán giòn cay (2 miếng)", ServingSize = "2 miếng (250g)", Calories = 640, Protein = 36.0, Carbs = 32.0, Fat = 38.0, TrafficLightStatic = "RED" },
                new Food { Name = "Pizza phô mai xúc xích (2 lát)", ServingSize = "2 lát (200g)", Calories = 560, Protein = 22.0, Carbs = 58.0, Fat = 25.0, TrafficLightStatic = "RED" },
                new Food { Name = "Lẩu thái chua cay hải sản (chia suất)", ServingSize = "1 phần ăn (450g)", Calories = 590, Protein = 36.0, Carbs = 42.0, Fat = 28.0, TrafficLightStatic = "RED" }
            };

            context.Foods.AddRange(foods);
            context.SaveChanges();

            // 3. SEED DỮ LIỆU TỦ LẠNH MẪU CHO USER (Có món sắp hết hạn <= 48h để demo chức năng cảnh báo)
            var now = DateTime.UtcNow;
            var fridgeItems = new List<FridgeItem>
            {
                new FridgeItem
                {
                    UserId = proUser.Id,
                    ItemName = "Cá hồi phi lê tươi",
                    Category = "MeatFish",
                    Quantity = 0.5,
                    Unit = "kg",
                    PurchasedDate = now.AddDays(-2),
                    ExpiryDate = now.AddHours(28), // Hết hạn trong vòng 28h (Kích hoạt cảnh báo <= 48h!)
                    EstimatedCost = 180000,
                    IsConsumed = false
                },
                new FridgeItem
                {
                    UserId = proUser.Id,
                    ItemName = "Rau cải thìa Đà Lạt",
                    Category = "Vegetable",
                    Quantity = 1.0,
                    Unit = "bó",
                    PurchasedDate = now.AddDays(-1),
                    ExpiryDate = now.AddHours(36), // Hết hạn trong vòng 36h (Kích hoạt cảnh báo!)
                    EstimatedCost = 25000,
                    IsConsumed = false
                },
                new FridgeItem
                {
                    UserId = proUser.Id,
                    ItemName = "Trứng gà Ba Huân",
                    Category = "Dairy",
                    Quantity = 10,
                    Unit = "quả",
                    PurchasedDate = now,
                    ExpiryDate = now.AddDays(14), // Còn 14 ngày
                    EstimatedCost = 35000,
                    IsConsumed = false
                },
                new FridgeItem
                {
                    UserId = proUser.Id,
                    ItemName = "Sữa tươi không đường Vinamilk",
                    Category = "Dairy",
                    Quantity = 2,
                    Unit = "hộp",
                    PurchasedDate = now,
                    ExpiryDate = now.AddDays(7),
                    EstimatedCost = 65000,
                    IsConsumed = false
                }
            };

            context.FridgeItems.AddRange(fridgeItems);

            // 4. SEED LỊCH SỬ CÂN NẶNG ĐỂ VẼ BIỂU ĐỒ (7 ngày gần nhất)
            var weightLogs = new List<WeightLog>
            {
                new WeightLog { UserId = proUser.Id, WeightKg = 69.5, RecordedAt = now.AddDays(-6), Note = "Bắt đầu lộ trình" },
                new WeightLog { UserId = proUser.Id, WeightKg = 69.2, RecordedAt = now.AddDays(-5) },
                new WeightLog { UserId = proUser.Id, WeightKg = 68.9, RecordedAt = now.AddDays(-4) },
                new WeightLog { UserId = proUser.Id, WeightKg = 68.8, RecordedAt = now.AddDays(-3) },
                new WeightLog { UserId = proUser.Id, WeightKg = 68.4, RecordedAt = now.AddDays(-2) },
                new WeightLog { UserId = proUser.Id, WeightKg = 68.2, RecordedAt = now.AddDays(-1) },
                new WeightLog { UserId = proUser.Id, WeightKg = 68.0, RecordedAt = now, Note = "Mục tiêu đạt tốt" }
            };

            context.WeightLogs.AddRange(weightLogs);
            context.SaveChanges();
        }
    }
}
