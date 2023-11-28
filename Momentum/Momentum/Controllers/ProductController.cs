using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using Momentum.ViewModels.BlogVMs;
using Momentum.ViewModels.ProductVMs;
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

        public IActionResult Index(int? id, string sortBy, int currentPage = 1)
        {
            ViewBag.SortBy = sortBy;
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

            // Apply sorting if sortBy is not null
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
                    // Add more cases for other sorting options
                    default:
                        break;
                }
            }

            return View(PageNatedList<Product>.Create(products, currentPage, 6, 10));
        }
        //public async Task<IActionResult> SortBy(IQueryable<Product> products,string sortBy, int currentPage=1)
        //{
        //    ViewBag.SortBy = sortBy;

        //    if(products ==null) return BadRequest();

        //    switch (sortBy)
        //    {
        //        case "title-ascending":
        //            products = products.OrderBy(p => p.Title);
        //            break;
        //        case "title-descending":
        //            products = products.OrderByDescending(p => p.Title);
        //            break;
        //        case "price-ascending":
        //            products = products.OrderBy(p => p.Price);
        //            break;
        //        case "price-descending":
        //            products = products.OrderByDescending(p => p.Price);
        //            break;
        //        case "created-ascending":
        //            products = products.OrderBy(p => p.CreatedAt);
        //            break;
        //        case "created-descending":
        //            products = products.OrderByDescending(p => p.CreatedAt);
        //            break;
        //        default:
        //            products = products.OrderByDescending(p => !p.IsDeleted);
        //            break;
        //    }
        //    return PartialView("_ProductGridPartial", PageNatedList<Product>.Create(products, currentPage, 6, 10));
        //}
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
        //public IActionResult Index(int? id, int currentPage = 1, string sortBy = "best-selling")
        //{
        //    IQueryable<Product> products;

        //    if (id == null)
        //    {
        //        products = _context.Products
        //            .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
        //            .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
        //            .Where(c => c.IsDeleted == false);
        //    }
        //    else
        //    {
        //        products = _context.Products
        //            .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
        //            .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
        //            .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == id && !pc.IsDeleted) && !p.IsDeleted);
        //    }

        //    switch (sortBy)
        //    {
        //        case "title-ascending":
        //            products = products.OrderBy(p => p.Title);
        //            break;
        //        case "title-descending":
        //            products = products.OrderByDescending(p => p.Title);
        //            break;
        //        case "price-ascending":
        //            products = products.OrderBy(p => p.Price);
        //            break;
        //        case "price-descending":
        //            products = products.OrderByDescending(p => p.Price);
        //            break;
        //        case "created-ascending":
        //            products = products.OrderBy(p => p.CreatedAt);
        //            break;
        //        case "created-descending":
        //            products = products.OrderByDescending(p => p.CreatedAt);
        //            break;
        //        default:
        //            products = products.OrderByDescending(p => !p.IsDeleted);
        //            break;
        //    }

        //    return View(PageNatedList<Product>.Create(products, currentPage, 6, 10));
        //}
        //public IActionResult FilterProducts(int? minPrice, int? maxPrice, int currentPage = 1)
        //{

        //    int actualMinPrice = minPrice ?? 0;
        //    int actualMaxPrice = maxPrice ?? int.MaxValue;


        //    //var filteredProducts = _productRepository.GetProducts()
        //    //    .Where(product => product.DiscountedPrice!=null&&product.DiscountedPrice>0?product.DiscountedPrice >= actualMinPrice:product.Price>=actualMinPrice && product.DiscountedPrice != null && product.DiscountedPrice > 0 ? product.DiscountedPrice <= actualMaxPrice : product.Price <= actualMaxPrice);

        //    var filteredProducts = _productRepository.GetProducts()
        //    .Where(product =>
        //        (product.DiscountedPrice != null && product.DiscountedPrice > 0)
        //            ? (product.DiscountedPrice >= actualMinPrice && product.DiscountedPrice <= actualMaxPrice)
        //            : (product.Price >= actualMinPrice && product.Price <= actualMaxPrice)
        //    );

        //    var pagedProducts = PageNatedList<Product>.Create(filteredProducts, currentPage, 12, 5);


        //    ViewBag.MinPrice = actualMinPrice;
        //    ViewBag.MaxPrice = actualMaxPrice;


        //    return PartialView("_ProductPartial", pagedProducts);
        //}

    }
}
