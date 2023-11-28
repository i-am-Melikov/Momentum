using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.ViewModels.HomeVMs;

namespace Momentum.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM
            {
                TopSeller = await _context.Products.Where(s => !s.IsDeleted && s.IsTopSeller == true)
                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductCategories.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Category)
                .ToListAsync(),

                OurProduct = await _context.Products.Where(s => !s.IsDeleted && s.IsOurProduct == true)
                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductCategories.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Category)
                .ToListAsync(),

                PC = await _context.Products
                .Where(p => !p.IsDeleted && p.ProductCategories.Any(pc => !pc.IsDeleted && pc.Category.Name == "Pc"))
                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
                .FirstOrDefaultAsync(),

                Headphone = await _context.Products.Where(s => !s.IsDeleted)
                .Where(p => !p.IsDeleted && p.ProductCategories.Any(pc => !pc.IsDeleted && pc.Category.Name == "Headphone"))
                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
                .FirstOrDefaultAsync(),

                Blog = await _context.Blogs.Where(b => !b.IsDeleted).ToListAsync(),
            };

            return View(homeVM);
        }
    }
}
