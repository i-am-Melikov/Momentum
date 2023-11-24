using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using System.Drawing.Drawing2D;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index(int currentPage = 1)
        {
            IQueryable<Product> products = _context.Products
                .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Category)
                .Include(p=> p.ProductColors.Where(pc=>pc.IsDeleted==false)).ThenInclude(p=>p.Color)
                .Include(p => p.Brand)
                .Where(c => c.IsDeleted == false)
                .OrderByDescending(c => c.Id);

            return View(PageNatedList<Product>.Create(products, currentPage, 6, 10));
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.Colors = await _context.Colors.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.ProductImages = await _context.ProductImages.Where(pi => pi.IsDeleted == false && pi.ProductId == id).ToListAsync();

            Product? product = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false)).ThenInclude(p => p.Color)
                .Include(c => c.ProductCategories.Where(pc=>!pc.IsDeleted)).ThenInclude(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.Colors = await _context.Colors.Where(c => c.IsDeleted == false).ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.Colors = await _context.Colors.Where(c => c.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid) return View(product);

            if (product.DiscountedPrice != null && product.DiscountedPrice > 0 && product.Price <= product.DiscountedPrice)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted price can`t be more than price");
                return View(product);
            }

            if (product.BrandId != null && !await _context.Brands.AnyAsync(c => c.IsDeleted == false && c.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", $"Brand id = {product.Brand} is incorrect");
                return View(product);
            }

            List<ProductColor> productColors = new List<ProductColor>();
            
            if (product.ColorIds != null && product.ColorIds.Count() > 0)
            {
                foreach (int colorId in product.ColorIds)
                {
                    if (!await _context.Colors.AnyAsync(t => t.IsDeleted == false && t.Id == colorId))
                    {
                        ModelState.AddModelError("ColorIds", $"Color id = {colorId} is incorrect");
                        return View(product);
                    }
                    ProductColor productColor = new ProductColor
                    {
                        ColorId = colorId,
                    };
                    productColors.Add(productColor);
                }
            }
            product.ProductColors = productColors;


            List<ProductCategory> productCategories = new List<ProductCategory>();

            if (product.CategoryIds != null && product.CategoryIds.Count() > 0)
            {
                foreach (int categoryId in product.CategoryIds)
                {
                    if (!await _context.Categories.AnyAsync(t => t.IsDeleted == false && t.Id == categoryId))
                    {
                        ModelState.AddModelError("CategoryIds", $"Category id = {categoryId} is incorrect");
                        return View(product);
                    }
                    ProductCategory productCategory = new ProductCategory
                    {
                        CategoryId = categoryId,
                    };
                    productCategories.Add(productCategory);
                }
            }
            product.ProductCategories = productCategories;

            //Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId);
            //Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == product.BrandId);

            //string seria = (category.Name.Substring(0, 2) + brand.Name.Substring(0, 2)).ToLower();

            if (product.Files == null || product.Files.Count() <= 0)
            {
                ModelState.AddModelError("Files", $"Must be minimum 1 file");
                return View(product);
            }

            if (product.Files.Count() > 10)
            {
                ModelState.AddModelError("Files", $"Can be Maximum 10 files");
                return View(product);
            }

            foreach (IFormFile file in product.Files)
            {
                if (!file.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("Files", "File Type must be image");
                    return View(product);
                }
                if ((file.Length / 1024) > 500)
                {
                    ModelState.AddModelError("Files", "File length must be maximum 500kb");
                    return View(product);
                }
            }

            List<ProductImage> productImages = new List<ProductImage>();

            foreach (IFormFile file in product.Files)
            {
                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + file.FileName
                    .Substring(file.FileName.LastIndexOf('.'));

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                ProductImage productImage = new ProductImage
                {
                    Image = fileName
                };
                productImages.Add(productImage);
            }

            product.ProductImages = productImages;

            if (product.MainFile != null)
            {
                if (!product.MainFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainFile", "File Type must be image");
                    return View(product);
                }
                if ((product.MainFile.Length / 1024) > 500)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 500kb");
                    return View(product);
                }

                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + product.MainFile.FileName
                    .Substring(product.MainFile.FileName.LastIndexOf('.'));

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await product.MainFile.CopyToAsync(fileStream);
                }
                product.MainImage = fileName;
            }
            else
            {
                ModelState.AddModelError("MainFile", "Main file required");
                return View(product);
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(p => p.ProductColors.Where(pc => pc.IsDeleted == false))
                .Include(p => p.ProductCategories.Where(pc => pc.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(c => c.IsDeleted == false).ToListAsync();
            ViewBag.Colors = await _context.Colors.Where(c => c.IsDeleted == false).ToListAsync();

            product.ColorIds = product.ProductColors.Select(pc => pc.ColorId).ToList();
            product.CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(c => c.IsDeleted == false).ToListAsync();
            if (product.CategoryIds == null)
            {
                product.CategoryIds = new List<int>();
            }
            if (product.ColorIds == null)
            {
                product.ColorIds = new List<int>();
            }

            if (!ModelState.IsValid) return View(product);

            if (id == null) return BadRequest();

            if (product.Id != id) return BadRequest();
            
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            Product dbProduct = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(c => c.ProductCategories.Where(pc => pc.IsDeleted == false))
                .Include(c => c.ProductColors.Where(pc => pc.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (dbProduct == null) return NotFound();

            if (product.BrandId == null || !await _context.Brands.AnyAsync(c => c.IsDeleted == false && c.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Incorrect");
                return View(dbProduct);
            }

            dbProduct.CategoryIds = product.CategoryIds;
            dbProduct.ColorIds = product.ColorIds;

            int canUpload = 10 - dbProduct.ProductImages.Count;

            if (product.DiscountedPrice > product.Price)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted price cannot be high than price");
                return View(product);
            }
            if (product.Files != null && product.Files.Count() > canUpload)
            {
                ModelState.AddModelError("Files", $"You can add maximum {canUpload} files");
                return View(dbProduct);
            }

            if (dbProduct.ProductColors != null && dbProduct.ProductColors.Count() > 0)
            {
                foreach (ProductColor productColor in dbProduct.ProductColors)
                {
                    productColor.IsDeleted = true;
                    productColor.DeletedAt = DateTime.UtcNow.AddHours(4);
                    productColor.DeletedBy = "Admin";
                }
            }

            List<ProductColor> productColors = new List<ProductColor>();

            if (product.ColorIds != null && product.ColorIds.Count() > 0)
            {
                dbProduct.ColorIds = null;

                foreach (int colorId in product.ColorIds)
                {
                    if (!await _context.Colors.AnyAsync(t => t.IsDeleted == false && t.Id == colorId))
                    {
                        ModelState.AddModelError("ColorIds", $"Color id = {colorId} is incorrect");
                        return View(dbProduct);
                    }
                    ProductColor productColor = new ProductColor
                    {
                        ColorId = colorId,
                    };
                    productColors.Add(productColor);
                }
            }
            dbProduct.ProductColors ??= new List<ProductColor>();
            dbProduct.ProductColors.AddRange(productColors);


            if (dbProduct.ProductCategories != null && dbProduct.ProductCategories.Count() > 0)
            {
                foreach (ProductCategory productCategory in dbProduct.ProductCategories)
                {
                    productCategory.IsDeleted = true;
                    productCategory.DeletedAt = DateTime.UtcNow.AddHours(4);
                    productCategory.DeletedBy = "Admin";
                }
            }

            List<ProductCategory> productCategories = new List<ProductCategory>();

            if (product.CategoryIds != null && product.CategoryIds.Count() > 0)
            {
                dbProduct.CategoryIds = null;

                foreach (int categoryId in product.CategoryIds)
                {
                    if (!await _context.Categories.AnyAsync(t => t.IsDeleted == false && t.Id == categoryId))
                    {
                        ModelState.AddModelError("CategoryIds", $"Category id = {categoryId} is incorrect");
                        return View(dbProduct);
                    }
                    ProductCategory productCategory = new ProductCategory
                    {
                        CategoryId = categoryId,
                    };
                    productCategories.Add(productCategory);
                }
            }
            dbProduct.ProductCategories ??= new List<ProductCategory>();
            dbProduct.ProductCategories.AddRange(productCategories);

            if (product.MainFile != null)
            {
                if (!product.MainFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainFile", "File Type must be image");
                    return View(dbProduct);
                }
                if ((product.MainFile.Length / 1024) > 100)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 100kb");
                    return View(dbProduct);
                }

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", dbProduct.MainImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + product.MainFile.FileName
                    .Substring(product.MainFile.FileName.LastIndexOf('.'));

                filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await product.MainFile.CopyToAsync(fileStream);
                }
                dbProduct.MainImage = fileName;
            }

            if (product.Files != null)
            {
                foreach (IFormFile file in product.Files)
                {
                    if (!file.ContentType.Contains("image/"))
                    {
                        ModelState.AddModelError("Files", "File Type must be image");
                        return View(dbProduct);
                    }
                    if ((file.Length / 1024) > 100)
                    {
                        ModelState.AddModelError("Files", "File length must be maximum 100kb");
                        return View(dbProduct);
                    }
                }

                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.Files)
                {
                    string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + file.FileName
                        .Substring(file.FileName.LastIndexOf('.'));

                    string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", fileName);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = fileName
                    };
                    productImages.Add(productImage);
                }

                dbProduct.ProductImages.AddRange(productImages);
            }

            dbProduct.Title = product.Title.Trim();
            dbProduct.Price = product.Price;
            dbProduct.BrandId = product.BrandId;
            dbProduct.DiscountedPrice = product.DiscountedPrice;
            dbProduct.Description = product.Description;
            dbProduct.Count = product.Count;
            dbProduct.IsTopSeller = product.IsTopSeller;
            dbProduct.IsOurProduct = product.IsOurProduct;
            dbProduct.UpdatedBy = "Admin";
            dbProduct.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> DeleteImage(int? id, int? imageId)
        {
            if (id == null) return BadRequest();
            if (imageId == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            if (!product.ProductImages.Any(pi => pi.Id == imageId)) return NotFound();

            if (product.ProductImages.Count() <= 1) return BadRequest();

            product.ProductImages.FirstOrDefault(pi => pi.Id == imageId).IsDeleted = true;
            product.ProductImages.FirstOrDefault(pi => pi.Id == imageId).DeletedAt = DateTime.UtcNow.AddHours(4);
            product.ProductImages.FirstOrDefault(pi => pi.Id == imageId).DeletedBy = "Admin";

            await _context.SaveChangesAsync();

            string fileName = product.ProductImages.FirstOrDefault(pi => pi.Id == imageId).Image;

            string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return RedirectToAction("Update", new { id = id });
        }
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            ViewBag.ProductImages = await _context.ProductImages.Where(pi => pi.IsDeleted == false && pi.ProductId == id).ToListAsync();

            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(c => c.ProductCategories.Where(pt => pt.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            return View(product);
        }
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(c => c.ProductCategories.Where(pt => pt.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            product.DeletedBy = "Admin";
            product.DeletedAt = DateTime.UtcNow.AddHours(4);

            if (product.MainFile != null)
            {

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", product.MainImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

            }

            if (product.Files != null && product.Id == id)
            {
                foreach (ProductImage productImage in product.Files)
                {

                    string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", productImage.Image);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                }

            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
