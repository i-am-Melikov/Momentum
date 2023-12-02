using Momentum.Enums;
using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Order : BaseEntity
    {
        public int No { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public OrderStatus Status { get; set; }
        [StringLength(255)]
        public string? Comment { get; set; }
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(255)]
        public string SurName { get; set; }
        [StringLength(255), EmailAddress]
        public string Email { get; set; }
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

        public IEnumerable<OrderProduct>? OrderProducts { get; set; }
    }
}
