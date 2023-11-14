using Microsoft.AspNetCore.Mvc;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
