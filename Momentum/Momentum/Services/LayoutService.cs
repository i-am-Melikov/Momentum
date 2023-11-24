using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Interfaces;
using Momentum.Models;
using Momentum.ViewModels.BasketVMs;
using Newtonsoft.Json;

namespace Momentum.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        public LayoutService(AppDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }
        public async Task<Dictionary<string, string>> GetSettingAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }
        public async Task<List<BasketVM>> GetBasketsAsync()
        {
            string cookie = _contextAccessor.HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (!string.IsNullOrEmpty(cookie))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }
            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
                if(product != null)
                {
                    basketVM.Title = product.Title;
                    basketVM.Image = product.MainImage;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                }
            }
            return basketVMs;
        }
    }
}
