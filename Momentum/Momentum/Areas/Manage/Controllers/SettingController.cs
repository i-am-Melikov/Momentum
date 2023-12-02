using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.Areas.Manage.ViewModels.SettingVM;
using Momentum.DataAccess;
using Momentum.Models;
using NuGet.Configuration;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;

        public SettingController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Setting> settings = _context.Settings.ToList();
            return View(settings);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();

            Setting setting = await _context.Settings.FirstOrDefaultAsync(x => x.Id == id);
            if (setting == null) return NotFound();
            ViewBag.key = setting.Key;
            var model = new SettingVM()
            {
                Value = setting.Value,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, SettingVM setting)
        {
            if (id == null) return BadRequest();
            var dbSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (dbSetting == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.key = dbSetting.Key;
                return View(setting);
            }

            dbSetting.Value = setting.Value;
            _context.Settings.Update(dbSetting);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
