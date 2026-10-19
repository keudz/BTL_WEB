using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BTL_WEB.Controllers
{
    [Authorize]
    public class FridgeController : Controller
    {
        // GET: /Fridge/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
