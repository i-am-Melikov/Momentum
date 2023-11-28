using Momentum.Models;

namespace Momentum.ViewModels.HomeVMs
{
    public class HomeVM
    {
        public IEnumerable<Product> TopSeller { get; set; }
        public IEnumerable<Product> OurProduct { get; set; }
        public Product PC { get; set; }
        public Product Headphone { get; set; }
        public IEnumerable<Blog> Blog { get; set; }
    }
}
