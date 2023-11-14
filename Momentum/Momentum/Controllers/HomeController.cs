using Microsoft.AspNetCore.Mvc;

namespace Momentum.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
