﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;

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
                .Where(c => c.IsDeleted == false)
                .OrderByDescending(c => c.Id);

            return View(PageNatedList<Product>.Create(products, currentPage, 6, 10));
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            ViewBag.ProductImages = await _context.ProductImages.Where(pi => pi.IsDeleted == false && pi.ProductId == id).ToListAsync();

            Product? product = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(c => c.ProductCategories).ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid) return View(product);

            if (product.DiscountedPrice != null && product.DiscountedPrice > 0 && product.Price <= product.DiscountedPrice)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted price can`t be more than price");
                return View(product);
            }

            if (product.DiscountedPrice != null && product.DiscountedPrice > 0 && (product.EcoTax >= product.DiscountedPrice || product.EcoTax >= product.Price))
            {
                ModelState.AddModelError("EcoTax", "Tax can`t be more than price or discounted price");
                return View(product);
            }


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
                if ((file.Length / 1024) > 200)
                {
                    ModelState.AddModelError("Files", "File length must be maximum 200kb");
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
                if ((product.MainFile.Length / 1024) > 200)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 200kb");
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
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            if (product.CategoryIds == null)
            {
                product.CategoryIds = new List<int>();
            }

            if (!ModelState.IsValid) return View(product);

            if (id == null) return BadRequest();

            if (product.Id != id) return BadRequest();
            
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync();

            Product dbProduct = await _context.Products
                .Include(p => p.ProductImages.Where(pi => pi.IsDeleted == false))
                .Include(c => c.ProductCategories.Where(pc => pc.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (dbProduct == null) return NotFound();

            dbProduct.CategoryIds = product.CategoryIds;

            int canUpload = 10 - dbProduct.ProductImages.Count;

            if (product.DiscountedPrice > product.Price)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted price cannot be high than price");
                return View(product);
            }
            if (product.EcoTax > product.Price || product.EcoTax > product.DiscountedPrice)
            {
                ModelState.AddModelError("EcoTax", "EcoTax cannot be high than price or discounted price");
                return View(product);
            }
            if (product.Files != null && product.Files.Count() > canUpload)
            {
                ModelState.AddModelError("Files", $"You can add maximum {canUpload} files");
                return View(dbProduct);
            }

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
                        ModelState.AddModelError("TagIds", $"Tag id = {categoryId} is incorrect");
                        return View(dbProduct);
                    }
                    ProductCategory productCategory = new ProductCategory
                    {
                        CategoryId = categoryId,
                    };
                    productCategories.Add(productCategory);
                }
            }

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
            dbProduct.DiscountedPrice = product.DiscountedPrice;
            dbProduct.Description = product.Description;
            dbProduct.EcoTax = product.EcoTax;
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