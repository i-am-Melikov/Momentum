using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [StringLength(255)]
        public string? CreatedBy { get; set; } = "System";
        public DateTime? DeletedAt { get; set; }
        [StringLength(255)]
        public string? DeletedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(255)]
        public string? UpdatedBy { get; set; }
    }
}
