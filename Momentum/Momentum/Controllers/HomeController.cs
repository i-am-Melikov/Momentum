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
                PC = await _context.Products.Where(p => p.IsDeleted == false && p.IsTopSeller == true)
                .Join(
                    _context.ProductCategories.Include(pc => pc.Category).Where(pc => pc.Category.Name == "PC"),
                    product => product.Id,
                    productCategory => productCategory.ProductId,
                    (product, productCategory) => product
                ).ToListAsync(),
                Console = await _context.Products.Where(p => p.IsDeleted == false && p.IsTopSeller == true)
                .Join(
                    _context.ProductCategories.Include(pc => pc.Category).Where(pc => pc.Category.Name == "Console"),
                    product => product.Id,
                    productCategory => productCategory.ProductId,
                    (product, productCategory) => product
                ).ToListAsync(),

                Headphone = await _context.Products
                .Where(p => p.IsDeleted == false && p.IsTopSeller == true)
                .Join(
                    _context.ProductCategories.Include(pc => pc.Category).Where(pc => pc.Category.Name == "Headphone"),
                    product => product.Id,
                    productCategory => productCategory.ProductId,
                    (product, productCategory) => product
                ).ToListAsync(),

                TopSeller = await _context.Products.Where(s => s.IsDeleted == false && s.IsTopSeller == true).ToListAsync(),
                Blog = await _context.Blogs.Where(b=>b.IsDeleted == false).ToListAsync(),
            };

            return View(homeVM);
        }
    }
}
