using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Color:BaseEntity
    {
        [StringLength(20)]
        public string Title{ get; set; }
        public List<ProductColor>? ProductColors { get; set; }
    }
}
