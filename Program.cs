using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using BTL_WEB.Data;

namespace BTL_WEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Cấu hình Database EF Core với SQL Server LocalDB
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Cấu hình Cookie Authentication (Thỏa mãn tiêu chí Cookies của giảng viên)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "KIT_Auth_Cookie";
                    options.Cookie.HttpOnly = true;
                });

            // 3. Cấu hình Session (Thỏa mãn tiêu chí Session của giảng viên)
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.Name = "KIT_Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 4. Đăng ký các Domain Services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<BTL_WEB.Services.ITdeeCalculatorService, BTL_WEB.Services.TdeeCalculatorService>();
            builder.Services.AddScoped<BTL_WEB.Services.ITrafficLightEngine, BTL_WEB.Services.TrafficLightEngine>();
            builder.Services.AddScoped<BTL_WEB.Services.IGeminiVisionService, BTL_WEB.Services.GeminiVisionService>();
            builder.Services.AddHostedService<BTL_WEB.Services.FridgeReminderService>();

            // 5. Cấu hình Quốc tế hóa (i18n) & MVC Controllers / Views
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllersWithViews()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // 5. Tự động khởi tạo Database và nạp sẵn dữ liệu mẫu (Seed Data)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Lỗi khi tự động khởi tạo cơ sở dữ liệu.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Hỗ trợ đa ngôn ngữ Tiếng Việt và Tiếng Anh
            var supportedCultures = new[] { "vi-VN", "en-US" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("vi-VN")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            app.UseRequestLocalization(localizationOptions);

            app.UseRouting();

            // Thứ tự Middleware quan trọng: Session -> Authentication -> Authorization
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
