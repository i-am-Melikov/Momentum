using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.Interfaces;

namespace Momentum.Controllers
{
    public class ContactController : Controller
    {
        private readonly ILayoutService _layoutService;

        public ContactController(ILayoutService layoutService)
        {
            _layoutService = layoutService;
        }
        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> settings = await _layoutService.GetSettingAsync();

            return View(settings);
        }
    }
}
