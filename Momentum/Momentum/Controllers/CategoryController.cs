using Microsoft.AspNetCore.Mvc;

namespace Momentum.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
