using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Interfaces;
using Momentum.Models;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Momentum.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService wishlistService;
        private readonly AppDbContext _context;
        public WishlistController(IWishlistService wishlistService, AppDbContext context)
        {
            this.wishlistService = wishlistService;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var wishlistItems = await wishlistService.GetUserWishlistsAsync(userId);

            return View(wishlistItems);
        }

        public async Task<IActionResult> AddToWishlist(int? id)
        {
            if (id == null) return BadRequest();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isInWishlist = await IsProductInWishlist(userId, id.Value);

            if (isInWishlist)
            {
                await wishlistService.RemoveFromWishlist(userId, id.Value);
                return Content($"Product removed from wishlist successfully.");
            }

            Product? product = await _context.Products
                .Include(p => p.ProductColors.Where(pc => !pc.IsDeleted)).ThenInclude(p => p.Color)
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

            if (product == null) return NotFound();

            wishlistService.AddToWishlist(userId, new Wishlist { ProductId = product.Id });

            return PartialView("_WishlistNotificationPartial", product);
        }
        public async Task<IActionResult> RemoveFromWishlist(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the product is in the wishlist
            bool isInWishlist = await IsProductInWishlist(userId, id.Value);

            if (isInWishlist)
            {
                // Remove the product from the wishlist
                await wishlistService.RemoveFromWishlist(userId, id.Value);
                return RedirectToAction(nameof(Index));
            }

            // If the product is not in the wishlist, you might want to handle this case differently
            return Content($"Product is not in the wishlist.");
        }
        private async Task<bool> IsProductInWishlist(string userId, int productId)
        {
            var userWishlists = await wishlistService.GetUserWishlistsAsync(userId);
            var isInWishlist = userWishlists.Any(item => item.ProductId == productId);
            return isInWishlist;
        }
    }
}
