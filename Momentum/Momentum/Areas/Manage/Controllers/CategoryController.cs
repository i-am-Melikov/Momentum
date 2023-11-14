using Microsoft.AspNetCore.Mvc;

namespace Momentum.Areas.Manage.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
