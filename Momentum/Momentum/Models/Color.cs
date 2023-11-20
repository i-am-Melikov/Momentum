namespace Momentum.Models
{
    public class Color:BaseEntity
    {
        public string Title{ get; set; }
        public List<ProductColor>? ProductColors { get; set; }
    }
}
