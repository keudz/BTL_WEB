using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BTL_WEB.Controllers
{
    public class AiScannerController : Controller
    {
        // GET: /AiScanner/Index
        public IActionResult Index()
        {
            // Kiểm tra nếu người dùng là Pro VIP
            bool isPro = User.IsInRole("Pro");
            ViewBag.IsPro = isPro;
            return View();
        }
    }
}
