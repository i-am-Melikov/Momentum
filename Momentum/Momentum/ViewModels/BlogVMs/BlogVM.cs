using Momentum.Models;

namespace Momentum.ViewModels.BlogVMs
{
    public class BlogVM
    {
        public Blog Selected { get; set; }
        public IEnumerable<Blog> Related { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }
}
