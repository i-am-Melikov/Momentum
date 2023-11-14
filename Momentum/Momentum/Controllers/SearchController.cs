using Microsoft.AspNetCore.Mvc;

namespace Momentum.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
