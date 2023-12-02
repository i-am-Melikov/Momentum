using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Interfaces;
using Momentum.Models;

namespace Momentum.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Wishlist>> GetUserWishlistsAsync(string userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public void AddToWishlist(string userId, Wishlist wishlistItem)
        {
            var wishlistEntity = new Wishlist
            {
                UserId = userId,
                ProductId = wishlistItem.ProductId,
            };

            _context.Wishlists.Add(wishlistEntity);
            _context.SaveChangesAsync();
        }

        public async Task RemoveFromWishlist(string userId, int productId)
        {
            var wishlistItem = _context.Wishlists
                .FirstOrDefault(item => item.UserId == userId && item.ProductId == productId);
            if (wishlistItem != null)
            {
                // Log or print a message to confirm that the item is found
                Console.WriteLine("Item found in wishlist: " + wishlistItem.Id);

                _context.Wishlists.Remove(wishlistItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
