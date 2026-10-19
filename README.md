# 🥗 KIT - HỆ THỐNG THEO DÕI & QUẢN LÝ CALORIES THÔNG MINH
**Bài tập lớn môn học: Lập trình Web**  
**Công nghệ:** ASP.NET Core MVC (.NET 8/10), Entity Framework Core, SQL Server LocalDB, AJAX RESTful Web API, Google Gemini AI Vision.

---

## 🚀 1. HƯỚNG DẪN CÀI ĐẶT & CHẠY ỨNG DỤNG (CHỈ CẦN 1 BƯỚC)

1. Mở Solution bằng **Visual Studio 2022** hoặc **VS Code**.
2. Nhấn **F5** (hoặc chạy lệnh terminal: `dotnet run`).
3. **Cơ sở dữ liệu tự động sinh:** Ứng dụng đã cấu hình cơ chế `DbInitializer`, khi chạy lần đầu sẽ **tự động tạo Database `KitCalorieDb` trên `(localdb)\mssqllocaldb`** và tự động nạp sẵn 3 tài khoản mẫu kèm hơn 35 món ăn Việt Nam chuẩn. Không cần gõ lệnh SQL thủ công!
4. *(Tùy chọn nộp bài cho Giảng viên)*: File Script CSDL thuần đã được chuẩn bị sẵn tại: [`KIT_Database_Full_Script.sql`](./KIT_Database_Full_Script.sql).

---

## 🎯 2. TÀI KHOẢN MẪU ĐỂ CHẤM THI / DEMO NHANH

Trên trang Đăng nhập (`/Account/Login`), hệ thống đã tích hợp sẵn **2 nút bấm điền nhanh tài khoản**:

| Loại tài khoản | Tên đăng nhập | Mật khẩu | Quyền hạn & Tính năng trải nghiệm |
| :--- | :--- | :--- | :--- |
| 👑 **User PRO VIP** | `prouser` | `Pro@123` | Toàn bộ quyền: Ghi nhật ký calo, Tủ lạnh ảo, **Quét ảnh AI Gemini Vision (mâm cơm & nguyên liệu)**, Quản lý & thêm món vào Thư viện |
| 🌱 **User Miễn phí (Free)** | `freeuser` | `User@123` | Tra cứu dinh dưỡng, ghi nhật ký, tủ lạnh. Khi bấm vào tính năng AI sẽ hiển thị thông báo giới hạn yêu cầu nâng cấp Pro VIP |

---

## 📋 3. BẢNG ĐỐI CHIẾU TIÊU CHÍ CHẤM ĐIỂM CỦA GIẢNG VIÊN

| Tiêu chí của Giảng viên | Hiện thực hóa trong Project KIT | Vị trí file trong Source Code |
| :--- | :--- | :--- |
| **1. Phân quyền người dùng (Multi-role)** | 2 Roles: `Pro` (VIP mở khóa AI) và `Free` (Cơ bản) | `Controllers/AccountController.cs`, `Models/Entities/User.cs` |
| **2. Template & Responsive** | Bootstrap 5 + Bộ CSS tùy biến dinh dưỡng `kit-theme.css`, chuẩn Mobile-First | `Views/Shared/_Layout.cshtml`, `wwwroot/css/kit-theme.css` |
| **3. AJAX, Paging, Filtering** | - Tìm kiếm món ăn realtime (case-insensitive) + lọc màu đèn + phân trang<br>- Thêm/xóa món ăn trong ngày cập nhật tức thì không reload trang | `Controllers/Api/FoodApiController.cs`, `Controllers/Api/DailyLogApiController.cs`, `Views/DailyLog/Index.cshtml` |
| **4. CRUD trên các bảng dữ liệu** | - CRUD Food (Admin)<br>- CRUD DailyLog & LogItem (User)<br>- CRUD FridgeItem (Tủ lạnh)<br>- CRUD Profile thể chất | `Controllers/FoodController.cs`, `Controllers/FridgeController.cs`, `Controllers/AccountController.cs` |
| **5. Session & Cookies** | - **Cookie:** Cookie Authentication (`KIT_Auth_Cookie`) ghi nhớ đăng nhập `Remember Me`<br>- **Session:** `HttpContext.Session` lưu UserId, Role, TargetCalories | `Program.cs`, `Controllers/AccountController.cs` |
| **6. Data Annotations** | Validate form chặt chẽ: `[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`, `[Compare]`, `[Display]` | `Models/ViewModels/*.cs`, `Models/Entities/*.cs` |
| **7. RESTful Web API** | Thư mục Controllers/Api cung cấp các endpoint JSON phục vụ AJAX | `Controllers/Api/FoodApiController.cs`, `DailyLogApiController.cs`, `FridgeApiController.cs`, `AiVisionApiController.cs` |
| **8. Bảo mật (Security)** | - Mã hóa mật khẩu chuẩn BCrypt<br>- Chống CSRF (`[ValidateAntiForgeryToken]`)<br>- Parameterized Queries chống SQL Injection qua EF Core<br>- Escape XSS trong Razor | `Program.cs`, `Controllers/*.cs` |
| **9. Quốc tế hóa (i18n)** | Chuyển đổi ngôn ngữ Tiếng Việt & Tiếng Anh qua Cookie Provider trên thanh navbar | `Program.cs`, `Controllers/LanguageController.cs`, `Views/Shared/_Layout.cshtml` |
| **10. Tác vụ ngầm (Background Task)** | `FridgeReminderService : BackgroundService` quét nguyên liệu có hạn dùng ≤ 48 giờ để chống lãng phí | `Services/FridgeReminderService.cs`, `Program.cs` |
| **11. Trợ lý AI Vision** | Nhận diện mâm cơm & nguyên liệu bằng Google Gemini API (kèm cơ chế Mock offline an toàn) | `Services/GeminiVisionService.cs`, `Controllers/Api/AiVisionApiController.cs` |
