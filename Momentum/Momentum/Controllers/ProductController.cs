using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using Momentum.ViewModels.BlogVMs;
using Momentum.ViewModels.ProductVMs;
using System.Drawing;
using System.Globalization;
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

        public IActionResult Index(int? id, string sortBy, int currentPage = 1, string availability = null, decimal? minPrice = null, decimal? maxPrice = null, string category = null, string brand = null, string color = null)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Categories = _context.Categories
                .Include(c => c.ProductCategories.Where(pc => !pc.Product.IsDeleted))
                .Where(c => !c.IsDeleted)
                .ToList();
            ViewBag.Brands= _context.Brands.Include(b=>b.Products).Where(c => !c.IsDeleted);
            ViewBag.Colors = _context.Colors.Include(b => b.ProductColors).Where(c => !c.IsDeleted);


            IQueryable<Product> products;

            if (id == null)
            {
                products = _context.Products
                    .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
                    .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
                    .Where(c => c.IsDeleted == false)
                    .OrderByDescending(c => c.Id);
            }
            else
            {
                products = _context.Products
                    .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
                    .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
                    .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == id && !pc.IsDeleted) && !p.IsDeleted)
                    .OrderByDescending(p => p.Id);
            }
            if (!string.IsNullOrEmpty(availability))
            {
                // Filter products based on selected availability
                var selectedAvailability = availability.Split(',');

                if (selectedAvailability.Contains("In stock"))
                {
                    products = products.Where(p => p.Count > 0);
                }

                if (selectedAvailability.Contains("Out of stock"))
                {
                    products = products.Where(p => p.Count <= 0);
                }
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => (p.DiscountedPrice > 0 && p.DiscountedPrice >= minPrice.Value) || (p.DiscountedPrice == 0 && p.Price >= (double)minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.DiscountedPrice > 0 && p.DiscountedPrice <= maxPrice.Value) || (p.DiscountedPrice == 0 && p.Price <= (double)maxPrice.Value));
            }


            if (!string.IsNullOrEmpty(category))
            {
                var selectedCategories = category.Split(',');

                products = products.Where(p => p.ProductCategories.Any(pc => selectedCategories.Contains(pc.Category.Name) && !pc.IsDeleted));
            }
            if (!string.IsNullOrEmpty(brand))
            {
                var selectedBrands = brand.Split(',');

                products = products.Where(p => selectedBrands.Contains(p.Brand.Name));
            }
  
            if (!string.IsNullOrEmpty(color))
            {
                var selectedColors = color.Split(',');

                products = products.Where(p => p.ProductColors.Any(pc => selectedColors.Contains(pc.Color.Title) && !pc.IsDeleted));
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "title-ascending":
                        products = products.OrderBy(p => p.Title);
                        break;
                    case "title-descending":
                        products = products.OrderByDescending(p => p.Title);
                        break;
                    case "price-ascending":
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price-descending":
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    case "created-ascending":
                        products = products.OrderBy(p => p.CreatedAt);
                        break;
                    case "created-descending":
                        products = products.OrderByDescending(p => p.CreatedAt);
                        break;
                    default:
                        break;
                }
            }

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

            IEnumerable<Product> relatedProducts = await _context.Products
               .Include(p => p.ProductCategories.Where(pc => !pc.IsDeleted)).ThenInclude(p => p.Category)
            .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(p => p.Color)
            .Include(p => p.ProductImages.Where(pc => !pc.IsDeleted))
            .Where(c => !c.IsDeleted && c.Id != id)
            .Take(6)
            .ToListAsync();

            if (product == null) return NotFound();

            ProductDetailVM productDetailVM = new ProductDetailVM
            {
                Selected = product,
                Relateds = relatedProducts,
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
        public async Task<IActionResult> Search(string? search)
        {
            List<Product> products = null;
            if (search != null)
            {
                products = await _context.Products.Where(p => p.IsDeleted == false && p.Title.ToLower().Contains(search.ToLower())).ToListAsync();
            }

            return PartialView("_SearchPartial", products);
        }
    }
}
