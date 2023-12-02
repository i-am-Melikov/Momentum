using Momentum.Models;
using Momentum.ViewModels.BasketVMs;

namespace Momentum.ViewModels.OrderVMs
{
    public class OrderVM
    {
        public Order Order { get; set; }
        public IEnumerable<BasketVM> BasketVMs { get; set; }
    }
}
