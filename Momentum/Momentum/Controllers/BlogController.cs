using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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

            Blog blog = _context.Blogs
                .FirstOrDefault(b => !b.IsDeleted && b.Id == id);

            IEnumerable<Comment> comments = _context.Comments
                .Where(b=>!b.IsDeleted&& b.BlogId == id)
                .OrderByDescending(b=>b.CreatedAt)
                .ToList();

            if (blog == null) return NotFound();

            List<Blog> relatedBlogs = _context.Blogs
                .Where(b => !b.IsDeleted && b.Id != blog.Id).Take(2).ToList();

            BlogVM blogVM = new BlogVM
            {
                Selected = blog,
                Related = relatedBlogs,
                Comments = comments
            };

            return View(blogVM);
        }
        [HttpPost]
        public IActionResult PostComment(int blogId, string name, string email, string content)
        {
            if(name == null || email == null || content ==null || blogId == null || blogId == 0)
            {
                return RedirectToAction("Detail", new { id = blogId });
            }

            var blog = _context.Blogs
                .Include(b => b.Comments)
                .FirstOrDefault(b => b.Id == blogId && !b.IsDeleted);

            if (blog == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Name = name,
                Email = email,
                Content = content,
                CreatedAt = DateTime.Now,
                CreatedBy = name
            };

            blog.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Detail", new { id = blogId });
        }
    }
}
