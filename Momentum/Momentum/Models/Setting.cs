using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Setting
    {
        public int Id { get; set; }
        [StringLength(255)]
        public string? Key { get; set; }
        [StringLength(5000)]
        public string? Value { get; set; }
    }
}
