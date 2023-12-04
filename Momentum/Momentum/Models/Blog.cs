using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Momentum.Models
{
    public class Blog:BaseEntity
    {
        [StringLength(250)]
        public string Title { get; set; }
        [StringLength(10000)]
        public string Description { get; set; }
        [StringLength(255)]
        public string? MainImage { get; set; }
        [NotMapped]
        public IFormFile? MainFile { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
