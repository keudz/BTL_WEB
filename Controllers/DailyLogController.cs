using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BTL_WEB.Controllers
{
    [Authorize]
    public class DailyLogController : Controller
    {
        // GET: /DailyLog/Index?date=yyyy-MM-dd
        public IActionResult Index(string? date = null)
        {
            ViewBag.SelectedDate = string.IsNullOrEmpty(date) 
                ? DateTime.UtcNow.ToString("yyyy-MM-dd") 
                : date;
            return View();
        }
    }
}
