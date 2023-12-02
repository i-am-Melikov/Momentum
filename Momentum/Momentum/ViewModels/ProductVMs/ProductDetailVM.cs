using Momentum.Models;

namespace Momentum.ViewModels.ProductVMs
{
    public class ProductDetailVM
    {
        public Product Selected { get; set; }
        public IEnumerable<Product> Relateds { get; set; }
    }
}
