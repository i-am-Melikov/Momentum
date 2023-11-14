using Microsoft.AspNetCore.Mvc;

namespace Momentum.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
