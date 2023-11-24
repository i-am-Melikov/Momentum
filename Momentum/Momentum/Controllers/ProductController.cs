using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using Momentum.ViewModels.BlogVMs;
using Momentum.ViewModels.ProductVMs;
using System.Reflection.Metadata;

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
        public async Task<IActionResult> Detail(int? id)
        {
            if(id == null) return BadRequest();

            Product? product = await _context.Products
            .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
            .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
            .Include(p => p.ProductImages.Where(pc => pc.IsDeleted == false))
            .Where(c => c.IsDeleted == false)
            .FirstOrDefaultAsync(c => c.Id == id);

            if(product == null) return NotFound();

            ProductDetailVM productDetailVM = new ProductDetailVM
            {
                Selected = product,
            };

            return View(productDetailVM);
        }
        public async Task<IActionResult> Modal(int? id)
        {
            if (id == null) return BadRequest("Id is not be null");
            Product? product = await _context.Products
                .Include(p => p.ProductCategories.Where(pi => pi.IsDeleted == false)).ThenInclude(p => p.Category)
                .Include(p => p.ProductColors.Where(pi => pi.IsDeleted == false)).ThenInclude(p => p.Color)
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

            if (product == null) return BadRequest("Id is Incorrect");

            return PartialView("_ModalPartial", product);
        }
    }
}
