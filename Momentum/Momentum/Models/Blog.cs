using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Momentum.Models
{
    public class Blog:BaseEntity
    {
        [StringLength(250)]
        public string Title { get; set; }
        [StringLength(2500)]
        public string Description { get; set; }
    }
}
