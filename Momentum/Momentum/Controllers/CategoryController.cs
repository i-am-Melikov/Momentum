using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;

namespace Momentum.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IQueryable<Category> categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.ProductCategories.Where(pc => !pc.IsDeleted && !pc.Product.IsDeleted))
                .ThenInclude(pc => pc.Product);

            if (categories == null) return NotFound();

            return View(categories);
        }
    }
}
