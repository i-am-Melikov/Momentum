using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class AppUser:IdentityUser
    {
        public bool IsActive { get; set; }
        [StringLength(255)]
        public string? Name { get; set; }
        [StringLength(255)]
        public string? SurName { get; set; }
        [NotMapped]
        public IList<string> Roles { get; set; }
        public IEnumerable<Basket>? Baskets { get; set; }

        public IEnumerable<Order>? Orders { get; set; }

        public IEnumerable<Address>? Addresses { get; set; }
    }
}
