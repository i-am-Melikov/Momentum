using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Address : BaseEntity
    {
        [StringLength(255)]
        public string Line1 { get; set; }
        [StringLength(255)]
        public string Line2 { get; set; }
        [StringLength(255)]
        public string Country { get; set; }
        [StringLength(255)]
        public string Town { get; set; }
        [StringLength(255)]
        public string State { get; set; }
        [StringLength(255)]
        public string PostalCode { get; set; }
        public bool IsDefault { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
