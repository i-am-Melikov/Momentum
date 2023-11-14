using Microsoft.AspNetCore.Mvc;

namespace Momentum.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
