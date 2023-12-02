using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using System.Drawing.Drawing2D;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;
        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int currentPage = 1)
        {
            IQueryable<Brand> queries = _context.Brands
                .Include(b => b.Products.Where(p => p.IsDeleted == false))
                .Where(b => b.IsDeleted == false)
                .OrderByDescending(b => b.Id);

            return View(PageNatedList<Brand>.Create(queries, currentPage, 5, 8));
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (!ModelState.IsValid) return View(brand);

            if (await _context.Brands.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower() == brand.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", $"{brand.Name} Already Exist");
                return View(brand);
            }
            brand.Name = brand.Name.Trim();

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, Brand brand)
        {
            if (id == null) return BadRequest();

            if (id != brand.Id) return BadRequest();

            if (!ModelState.IsValid) return View(brand);

            Brand dbBrand = await _context.Brands
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);
            if (dbBrand == null) return NotFound();

            if (await _context.Brands.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower() == brand.Name.Trim().ToLower() && b.Id != dbBrand.Id))
            {
                ModelState.AddModelError("Name", $"{brand.Name} Already Exist");
                return View(brand);
            }

            dbBrand.Name = brand.Name.Trim();
            dbBrand.UpdatedBy = "Admin";
            dbBrand.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands
                .Include(b => b.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();

            return View(brand);
        }
        public async Task<IActionResult> DeleteBrand(int? id)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands
                .Include(b => b.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) return NotFound();

            brand.IsDeleted = true;
            brand.DeletedBy = "Admin";
            brand.DeletedAt = DateTime.Now;

            if (brand.Products != null && brand.Products.Count() > 0)
            {
                foreach (Product product in brand.Products)
                {
                    product.BrandId = null;
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
