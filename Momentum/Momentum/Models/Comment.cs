using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Comment:BaseEntity
    {
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(3000)]
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
