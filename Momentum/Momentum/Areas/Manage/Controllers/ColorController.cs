using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ColorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ColorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public IActionResult Index()
        {
            IQueryable<Color> colors = _context.Colors.
                Include(c=>c.ProductColors.Where(pc=>!pc.IsDeleted)).
                ThenInclude(pc=>pc.Product).Where(c => !c.IsDeleted);

            return View(colors);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Color color)
        {
            if (!ModelState.IsValid) return View(color);

            if (await _context.Colors.AnyAsync(c => c.IsDeleted == false && c.Title.ToLower() == color.Title.Trim().ToLower()))
            {
                ModelState.AddModelError("Title", "Already Exists");
                return View(color);
            }
            color.Title = color.Title.Trim();

            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Color color =await _context.Colors.
                Include(c => c.ProductColors.Where(pc => !pc.IsDeleted)).
                ThenInclude(pc => pc.Product).Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (color == null) return NotFound();

            return View(color);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Color color)
        {
            if (!ModelState.IsValid) return View(color);

            if (id == null) return BadRequest();

            if (id != color.Id) return BadRequest();

            Color? dbColor = await _context.Colors.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (dbColor == null) return NotFound();

            if (color.Title == null) return BadRequest();

            if (await _context.Colors.AnyAsync(c => c.IsDeleted == false && c.Id != color.Id && c.Title.ToLower() == color.Title.Trim().ToLower()))
            {
                ModelState.AddModelError("Title", "Already Exists");
                return View(color);
            }

            dbColor.Title = color.Title.Trim();
            dbColor.UpdatedAt = DateTime.Now;
            dbColor.UpdatedBy = "Admin";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Color? color = await _context.Colors.
                Include(c => c.ProductColors.Where(pc => !pc.IsDeleted)).
                ThenInclude(pc => pc.Product).Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (color == null) return NotFound();

            return View(color);
        }
        public async Task<IActionResult> DeleteColor(int? id)
        {
            if (id == null) return BadRequest();

            Color? color = await _context.Colors.
                Include(c => c.ProductColors.Where(pc => !pc.IsDeleted)).
                ThenInclude(pc => pc.Product).Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (color == null) return NotFound();

            if (color.ProductColors != null && color.ProductColors.Count > 0)
            {
                return Content("Cannot delete color with associated product colors.");
            }

            color.IsDeleted = true;
            color.DeletedAt = DateTime.Now;
            color.DeletedBy = "Admin";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
