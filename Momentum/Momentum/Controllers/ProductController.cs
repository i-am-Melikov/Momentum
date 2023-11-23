using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;

namespace Momentum.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int currentPage = 1)
        {
            IQueryable<Product> products = _context.Products
            .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
            .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
            .Where(c => c.IsDeleted == false)
            .OrderByDescending(c => c.Id);

            return View(PageNatedList<Product>.Create(products, currentPage, 6, 10));
        }
        public IActionResult Detail()
        {
            return View();
        }
    }
}
