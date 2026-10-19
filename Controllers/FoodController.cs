using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BTL_WEB.Data;
using BTL_WEB.Models.Entities;
using BTL_WEB.Services;

namespace BTL_WEB.Controllers
{
    public class FoodController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITrafficLightEngine _trafficLightEngine;

        public FoodController(AppDbContext context, ITrafficLightEngine trafficLightEngine)
        {
            _context = context;
            _trafficLightEngine = trafficLightEngine;
        }

        // GET: /Food/Index (Trang tra cứu công cộng)
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Food/Manage (Chỉ Pro VIP được truy cập)
        [Authorize(Roles = "Pro")]
        public async Task<IActionResult> Manage(string? search = null, string? filter = null, int page = 1)
        {
            int pageSize = 12;
            var query = _context.Foods.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(f => f.Name.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter) && filter != "ALL")
            {
                query = query.Where(f => f.TrafficLightStatic == filter);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var foods = await query
                .OrderByDescending(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.Filter = filter;

            return View(foods);
        }

        // GET: /Food/Create (Pro VIP)
        [Authorize(Roles = "Pro")]
        public IActionResult Create()
        {
            return View(new Food());
        }

        // POST: /Food/Create
        [Authorize(Roles = "Pro")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food food)
        {
            if (!ModelState.IsValid)
            {
                return View(food);
            }

            // Mặc định tự tính Đèn giao thông nếu chưa chọn hoặc tự động
            if (string.IsNullOrEmpty(food.TrafficLightStatic))
            {
                food.TrafficLightStatic = _trafficLightEngine.GetStaticTrafficLight(food.Calories);
            }

            food.CreatedAt = DateTime.UtcNow;
            food.IsCustom = false;
            food.CreatedByUserId = 0;

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm món '{food.Name}' vào hệ thống!";
            return RedirectToAction(nameof(Manage));
        }

        // GET: /Food/Edit/{id}
        [Authorize(Roles = "Pro")]
        public async Task<IActionResult> Edit(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();
            return View(food);
        }

        // POST: /Food/Edit/{id}
        [Authorize(Roles = "Pro")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Food food)
        {
            if (id != food.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(food);
            }

            var existing = await _context.Foods.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = food.Name.Trim();
            existing.ServingSize = food.ServingSize.Trim();
            existing.Calories = food.Calories;
            existing.Protein = food.Protein;
            existing.Carbs = food.Carbs;
            existing.Fat = food.Fat;
            existing.TrafficLightStatic = food.TrafficLightStatic; // Cho phép Pro VIP override thủ công

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật món '{existing.Name}'!";
            return RedirectToAction(nameof(Manage));
        }

        // POST: /Food/Delete/{id}
        [Authorize(Roles = "Pro")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa món ăn khỏi hệ thống!";
            return RedirectToAction(nameof(Manage));
        }
    }
}
