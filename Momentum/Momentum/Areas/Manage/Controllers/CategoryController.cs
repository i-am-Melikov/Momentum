using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using NuGet.Protocol.Plugins;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public IActionResult Index(int currentPage = 1)
        {
            IQueryable<Category> categories = _context.Categories
            .Include(c => c.ProductCategories)
            .ThenInclude(pc => pc.Product)
            .Where(c => !c.IsDeleted);

            return View(PageNatedList<Category>.Create(categories, currentPage, 6,6));
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
            .Include(c => c.ProductCategories)
            .ThenInclude(pc => pc.Product)
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower() == category.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", "Already Exists");
                return View(category);
            }
            category.Name = category.Name.Trim();

            if (category.MainFile != null)
            {
                if (!category.MainFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainFile", "File type must be image");
                    return View(category);
                }
                if ((category.MainFile.Length / 1024) > 200)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 200kb");
                    return View(category);
                }

                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + category.MainFile.FileName
                    .Substring(category.MainFile.FileName.LastIndexOf('.'));

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "category", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await category.MainFile.CopyToAsync(fileStream);
                }
                category.MainImage = fileName;
            }
            else
            {
                ModelState.AddModelError("MainFile", "Main file required");
                return View(category);
            }
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Category category)
        {

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid) return View(category);
            
            if (id == null) return BadRequest();
            
            if (id != category.Id) return BadRequest();

            Category? dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (dbCategory == null) return NotFound();

            if (category.Name == null) return BadRequest();

            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Id != category.Id && c.Name.ToLower() == category.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", "Already Exists");
                return View(category);
            }

            if (category.MainFile != null)
            {
                if (!category.MainFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainFile", "File Type must be image");
                    return View(dbCategory);
                }
                if ((category.MainFile.Length / 1024) > 200)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 200kb");
                    return View(dbCategory);
                }

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "category", dbCategory.MainImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + category.MainFile.FileName
                    .Substring(category.MainFile.FileName.LastIndexOf('.'));

                filePath = Path.Combine(_env.WebRootPath, "assets", "img", "category", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await category.MainFile.CopyToAsync(fileStream);
                }
                dbCategory.MainImage = fileName;
            }

            dbCategory.Name = category.Name.Trim();
            dbCategory.UpdatedAt = DateTime.Now;
            dbCategory.UpdatedBy = "Admin";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Category? category = await _context.Categories
              .Include(c => c.ProductCategories)
              .ThenInclude(pc => pc.Product)
              .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null) return BadRequest();

            Category? category = await _context.Categories
            .Include(c => c.ProductCategories.Where(pc => !pc.IsDeleted && !pc.Product.IsDeleted))
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (category == null) return NotFound();

            if (category.ProductCategories != null && category.ProductCategories.Count > 0)
            {
                return Content("Cannot delete category with associated product categories.");
            }

            if (category.MainFile != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "category", category.MainImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            category.IsDeleted = true;
            category.DeletedAt = DateTime.Now;
            category.DeletedBy = "Admin";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
