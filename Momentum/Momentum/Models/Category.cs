using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public List<ProductCategory>? ProductCategories { get; set; }
        [StringLength(255)]
        public string? MainImage { get; set; }
        [NotMapped]
        public IFormFile? MainFile { get; set; }
    }
}
