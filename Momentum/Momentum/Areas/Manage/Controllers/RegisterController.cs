using Microsoft.AspNetCore.Mvc;

namespace Momentum.Areas.Manage.Controllers
{
    public class RegisterController : Controller
    {
        [Area("manage")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
