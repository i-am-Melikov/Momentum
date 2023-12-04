using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels.BasketVMs;
using Momentum.ViewModels.CompareVMs;
using Newtonsoft.Json;

namespace Momentum.Controllers
{
    public class CookieController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CookieController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Cart()
        {
            string basket = Request.Cookies["basket"];

            IEnumerable<BasketVM> basketVMs = new List<BasketVM>();

            if (!string.IsNullOrWhiteSpace(basket))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                AppUser? appUser = await _userManager.Users
                    .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (appUser != null)
                {
                    foreach (BasketVM basketVM in basketVMs)
                    {
                        Product product = await _context.Products
                            .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
                            .FirstOrDefaultAsync(p => p.Id == basketVM.Id);

                        basketVM.Title = product.Title;
                        basketVM.Image = product.MainImage;
                        basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                        basketVM.Color = string.Join(", ", product.ProductColors.Select(pc => pc.Color.Title));
                        basketVM.Count = basketVMs.FirstOrDefault(b => b.Id == product.Id).Count;
                    }
                }
            }

            return View(basketVMs);
        }
        //public async Task<IActionResult> Cart()
        //{
        //    AppUser appUser = await _userManager.Users
        //        .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
        //        .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        //    if (appUser != null)
        //    {
        //        // Create a list to store new baskets
        //        List<Basket> newBaskets = new List<Basket>();

        //        foreach (var basketItem in appUser.Baskets)
        //        {
        //            Product product = await _context.Products
        //                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
        //                .FirstOrDefaultAsync(p => p.Id == basketItem.ProductId);

        //            if (product != null)
        //            {
        //                // Map BasketVM to Basket model
        //                Basket basket = new Basket
        //                {
        //                    Count = basketItem.Count,
        //                    ProductId = basketItem.ProductId,
        //                    Title = product.Title,
        //                    Color = string.Join(", ", product.ProductColors.Select(pc => pc.Color.Title)),
        //                    Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price,
        //                    Image = product.MainImage,
        //                    // map other properties as needed
        //                };

        //                // Assuming you have a UserId property in your AppUser class
        //                if (User.Identity.IsAuthenticated)
        //                {
        //                    basket.UserId = appUser.Id;
        //                }

        //                newBaskets.Add(basket);
        //            }
        //        }

        //        // Add new baskets to the database
        //        await _context.Baskets.AddRangeAsync(newBaskets);
        //        await _context.SaveChangesAsync();
        //    }

        //    // Retrieve the updated baskets from the database
        //    List<Basket> basketVMs = await _context.Baskets
        //        .Where(b => b.UserId == appUser.Id && b.IsDeleted == false)
        //        .ToListAsync();

        //    return View(basketVMs);
        //}
        //[HttpPost]
        //public async Task<IActionResult> UpdateQuantity(int productId, int changeAmount)
        //{
        //    // Retrieve the user
        //    AppUser appUser = await _userManager.Users
        //        .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
        //        .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        //    string basket = Request.Cookies["basket"];

        //    IEnumerable<BasketVM> basketVMs = new List<BasketVM>();

        //    basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
        //    // Find the corresponding basket item
        //    Basket basketItem = appUser.Baskets.FirstOrDefault(b => b.Id == productId);

        //    if (basketItem != null)
        //    {
        //        // Update the quantity
        //        int newQuantity = basketItem.Count + changeAmount;

        //        // Ensure the quantity is at least 1
        //        basketItem.Count = Math.Max(1, newQuantity);
        //    }

        //    await _context.SaveChangesAsync();

        //    // Return a success status
        //    return Ok();
        //}
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null) return BadRequest("Id Is Not Be Null");

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound("Id Is InCorrect");

            string? basket = Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (!string.IsNullOrWhiteSpace(basket))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                if (basketVMs.Exists(b => b.Id == id))
                {
                    basketVMs.Find(b => b.Id == id).Count += 1;
                }
                else
                {
                    basketVMs.Add(new BasketVM
                    {
                        Id = (int)id,
                        Count = 1
                    });
                }
            }
            else
            {
                basketVMs = new List<BasketVM> { new BasketVM
                    {
                        Id = (int)id,
                        Count = 1
                    }
                };
            }

            basket = JsonConvert.SerializeObject(basketVMs);

            Response.Cookies.Append("basket", basket);

            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                AppUser appUser = await _userManager.Users
                    .Include(b => b.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (appUser != null && appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    Basket userBasket = appUser.Baskets.FirstOrDefault(b => b.ProductId == id);

                    if (userBasket != null)
                    {
                        userBasket.Count = basketVMs.FirstOrDefault(b => b.Id == id).Count;
                    }
                    else
                    {
                        Basket userNewBasket = new Basket
                        {
                            UserId = appUser.Id,
                            ProductId = id,
                            Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                        };

                        await _context.Baskets.AddAsync(userNewBasket);
                    }
                }
                else
                {
                    Basket userNewBasket = new Basket
                    {
                        UserId = appUser.Id,
                        ProductId = id,
                        Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                    };

                    await _context.Baskets.AddAsync(userNewBasket);
                }

                await _context.SaveChangesAsync();

            }

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);

                basketVM.Title = product.Title;
                basketVM.Image = product.MainImage;
                basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
            }

            return PartialView("_BasketPartial", basketVMs);
        }
        public IActionResult CompareIndex()
        {
            var compareCookie = Request.Cookies["compare"];
            List<CompareVM> compareVMs = null;

            if (!string.IsNullOrWhiteSpace(compareCookie))
            {
                compareVMs = JsonConvert.DeserializeObject<List<CompareVM>>(compareCookie);
            }

            return View(compareVMs);
        }
        public async Task<IActionResult> AddToCompare(int? id)
        {
            if (id == null)
            {
                return BadRequest("Id Is Not Be Null");
            }

            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound("Product not found or deleted.");
            }

            var compareCookie = Request.Cookies["compare"];
            List<CompareVM> compareVMs;

            if (!string.IsNullOrWhiteSpace(compareCookie))
            {
                compareVMs = JsonConvert.DeserializeObject<List<CompareVM>>(compareCookie);

                if (compareVMs.Any(c => c.Id == id))
                {
                    return Content("Product is already in the comparison list.");
                }

                compareVMs.Add(new CompareVM
                {
                    Id = product.Id,
                    Title = product.Title,
                    Price= (int)product.Price,
                    Category = product.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault(),
                    Image = product.MainImage,
                    DiscountedPrice = product.DiscountedPrice,
                });
            }
            else
            {
                compareVMs = new List<CompareVM> {
                    new CompareVM
                    {
                        Id = product.Id,
                        Title = product.Title,
                        Price= (int)product.Price,
                        Category = product.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault(),
                        Image = product.MainImage,
                        DiscountedPrice = product.DiscountedPrice,
                    }
                };
            }

            var updatedCompareCookie = JsonConvert.SerializeObject(compareVMs);
            Response.Cookies.Append("compare", updatedCompareCookie);

            var refererUrl = Request.Headers["Referer"].ToString();

            return Redirect(refererUrl);
        }
        public IActionResult RemoveFromCompare(int id)
        {
            var compareCookie = Request.Cookies["compare"];

            if (!string.IsNullOrWhiteSpace(compareCookie))
            {
                List<CompareVM> compareVMs = JsonConvert.DeserializeObject<List<CompareVM>>(compareCookie);

                compareVMs.RemoveAll(c => c.Id == id);

                var updatedCompareCookie = JsonConvert.SerializeObject(compareVMs);
                Response.Cookies.Append("compare", updatedCompareCookie);
            }

            return RedirectToAction(nameof(CompareIndex));
        }
        public async Task<IActionResult> BasketNotification(int? id)
        {
            if (id == null) return BadRequest("Id is not be null");

            var product = await _context.Products
                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(pc => pc.Color)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                var cartNotification = new CartNotificationVM
                {
                    Id = product.Id,
                    Title = product.Title,
                    Color = string.Join(", ", product.ProductColors.Select(pc => pc.Color.Title)),
                    Image = product.MainImage
                };

                return PartialView("_BasketNotificationPartial", cartNotification);
            }

            return NotFound();
        }
        [HttpDelete]
        [Route("/Cookie/DeleteItem/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var basketCookie = Request.Cookies["basket"];

            if (basketCookie == null)
            {
                return NotFound();
            }

            var basketItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BasketVM>>(basketCookie);

            var itemToDelete = basketItems.FirstOrDefault(item => item.Id == itemId);

            if (itemToDelete != null)
            {
                basketItems.Remove(itemToDelete);

                var updatedBasketCookie = Newtonsoft.Json.JsonConvert.SerializeObject(basketItems);

                Response.Cookies.Append("basket", updatedBasketCookie);

                AppUser? appUser = await _userManager.Users
                    .Include(u => u.Baskets.Where(b => b.IsDeleted == false && b.ProductId == itemId))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (appUser != null)
                {
                    var basketItemToDelete = appUser.Baskets.FirstOrDefault(b => b.ProductId == itemId);

                    if (basketItemToDelete != null)
                    {
                        basketItemToDelete.IsDeleted = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

    }
}