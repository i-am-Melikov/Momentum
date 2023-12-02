using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Enums;
using Momentum.Models;
using Momentum.ViewModels.BasketVMs;
using Momentum.ViewModels.OrderVMs;

namespace Momentum.Controllers
{
    [Authorize(Roles = "Member")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut()
        {
            AppUser appUser = await _userManager.Users
                .Include(u => u.Addresses.Where(a => a.IsDeleted == false && a.IsDefault))
                .Include(u => u.Baskets.Where(b => b.IsDeleted == false)).ThenInclude(b => b.Product)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser.Baskets == null || appUser.Baskets.Count() <= 0)
            {
                TempData["Info"] = "Zehmet Olmas Sebete Mehsul Elave Edin";
                return RedirectToAction("Index", "Product");
            }

            OrderVM orderVM = new OrderVM
            {
                Order = new Order
                {
                    Country = appUser.Addresses.First().Country,
                    Email = appUser.Email,
                    Line1 = appUser.Addresses.First().Line1,
                    Line2 = appUser.Addresses.First().Line2,
                    Name = appUser.Name,
                    PostalCode = appUser.Addresses.First().PostalCode,
                    SurName = appUser.SurName,
                    Town = appUser.Addresses.First().Town,
                    State = appUser.Addresses.First().State
                },
                BasketVMs = appUser.Baskets.Select(x => new BasketVM
                {
                    Id = (int)x.ProductId,
                    Count = x.Count,
                    Image = x.Product.MainImage,
                    Price = x.Product.DiscountedPrice > 0 ? x.Product.DiscountedPrice : x.Product.Price,
                    Title = x.Product.Title,
                }).ToList(),
            };

            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(Order order)
        {
            AppUser appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false && a.IsDefault))
               .Include(u => u.Orders)
               .Include(u => u.Baskets.Where(b => b.IsDeleted == false)).ThenInclude(b => b.Product)
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            OrderVM orderVM = new OrderVM
            {
                Order = order,
                BasketVMs = appUser.Baskets.Select(x => new BasketVM
                {
                    Id = (int)x.ProductId,
                    Count = x.Count,
                    Image = x.Product.MainImage,
                    Price = x.Product.DiscountedPrice > 0 ? x.Product.DiscountedPrice : x.Product.Price,
                    Title = x.Product.Title,
                }).ToList(),
            };

            if (!ModelState.IsValid) return View(orderVM);

            if (appUser.Baskets == null || appUser.Baskets.Count() <= 0)
            {
                TempData["Info"] = "Zehmet Olmas Sebete Mehsul Elave Edin";
                return RedirectToAction("Index", "Product");
            }

            List<OrderProduct> orderProducts = new List<OrderProduct>();

            foreach (Basket basket in appUser.Baskets)
            {
                basket.IsDeleted = true;

                OrderProduct orderProduct = new OrderProduct
                {
                    Count = basket.Count,
                    CreatedAt = DateTime.Now,
                    CreatedBy = appUser.Name + " " + appUser.SurName,
                    Price = basket.Product.DiscountedPrice > 0 ? basket.Product.DiscountedPrice : basket.Product.Price,
                    ProductId = basket.Product.Id,
                    Title = basket.Product.Title
                };

                orderProducts.Add(orderProduct);
            }

            order.OrderProducts = orderProducts;
            order.Status = OrderStatus.Pending;
            order.No = appUser.Orders != null && appUser.Orders.Count() > 0 ? appUser.Orders.OrderByDescending(o => o.Id).FirstOrDefault().No + 1 : 1;
            order.CreatedBy = appUser.Name + " " + appUser.SurName;
            order.UserId = appUser.Id;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("basket", "");

            TempData["Success"] = "Sifarisiniz Ugurla Elave Edildi";
            return RedirectToAction("Index", "Product");

        }
    }
}
