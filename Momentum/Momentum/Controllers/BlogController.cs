using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using Momentum.ViewModels.BlogVMs;

namespace Momentum.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        public BlogController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int currentPage = 1)
        {
            IQueryable<Blog> blogs = _context.Blogs.Where(b=>!b.IsDeleted);

            return View(PageNatedList<Blog>.Create(blogs, currentPage, 4,10));
        }
        public IActionResult Detail(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Blog blog = _context.Blogs.FirstOrDefault(b => !b.IsDeleted && b.Id == id);

            if (blog == null) return NotFound();

            List<Blog> relatedBlogs = _context.Blogs
                .Where(b => !b.IsDeleted && b.Id != blog.Id).Take(2).ToList();

            BlogVM blogVM = new BlogVM
            {
                Selected = blog,
                Related = relatedBlogs
            };

            return View(blogVM);
        }
    }
}
