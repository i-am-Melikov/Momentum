using Momentum.Models;
using System.ComponentModel.DataAnnotations;

namespace Momentum.ViewModels.AccountVMs
{
    public class ProfileVM
    {
        public ProfileAccountVM ProfileAccountVM { get; set; }

        public IEnumerable<Address> Addresses { get; set; }
        public IEnumerable<Order> Orders { get; set; }

        public Address Address { get; set; }
    }
}
