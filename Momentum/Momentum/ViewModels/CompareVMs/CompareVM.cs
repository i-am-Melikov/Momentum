using Momentum.Models;

namespace Momentum.ViewModels.CompareVMs
{
    public class CompareVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
    }
}
