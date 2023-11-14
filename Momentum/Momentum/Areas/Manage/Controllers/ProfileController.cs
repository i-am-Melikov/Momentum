using Microsoft.AspNetCore.Mvc;

namespace Momentum.Areas.Manage.Controllers
{
    public class ProfileController : Controller
    {
        [Area("manage")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
