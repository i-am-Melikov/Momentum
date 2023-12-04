using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels.BasketVMs;
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
        public IActionResult DeleteItem(int itemId)
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

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}