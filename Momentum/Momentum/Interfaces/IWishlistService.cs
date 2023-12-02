using Momentum.Models;

namespace Momentum.Interfaces
{
    public interface IWishlistService
    {
        Task<List<Wishlist>> GetUserWishlistsAsync(string userId);
        void AddToWishlist(string userId, Wishlist wishlistItem);
        Task RemoveFromWishlist(string userId, int productId);
    }
}
