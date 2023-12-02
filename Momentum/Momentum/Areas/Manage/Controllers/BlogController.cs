using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index(int currentPage = 1)
        {
            IQueryable<Blog> blogs = _context.Blogs.Where(c => !c.IsDeleted).OrderByDescending(c=>c.Id);

            return View(PageNatedList<Blog>.Create(blogs, currentPage, 6,10));
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Blog? blog = await _context.Blogs.Where(c => !c.IsDeleted).FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (blog == null) return NotFound();

            return View(blog);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (!ModelState.IsValid) return View(blog);

            if (blog.MainFile != null)
            {
                if (!blog.MainFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainFile", "File Type must be image");
                    return View(blog);
                }
                if ((blog.MainFile.Length / 1024) > 200)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 200kb");
                    return View(blog);
                }

                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + blog.MainFile.FileName
                    .Substring(blog.MainFile.FileName.LastIndexOf('.'));

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "blog", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await blog.MainFile.CopyToAsync(fileStream);
                }
                blog.MainImage = fileName;
            }
            else
            {
                ModelState.AddModelError("MainFile", "Main file required");
                return View(blog);
            }

            await _context.Blogs.AddAsync(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Blog? blog = await _context.Blogs.Where(c => !c.IsDeleted).FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);


            if (blog == null) return NotFound();

            return View(blog);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Blog blog)
        {
            if (id == null) return BadRequest();

            if (blog.Id != id) return BadRequest();

            Blog dbBlog = await _context.Blogs.Where(c => !c.IsDeleted).FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (dbBlog == null) return NotFound();

            if (!ModelState.IsValid) return View(dbBlog);

            if (blog.MainFile != null)
            {
                if (!blog.MainFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainFile", "File Type must be image");
                    return View(dbBlog);
                }
                if ((blog.MainFile.Length / 1024) > 200)
                {
                    ModelState.AddModelError("MainFile", "File length must be maximum 200kb");
                    return View(dbBlog);
                }

                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", dbBlog.MainImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                string fileName = DateTime.UtcNow.AddHours(4).ToString("yyyyMMddHHmmssfff") + blog.MainFile.FileName
                    .Substring(blog.MainFile.FileName.LastIndexOf('.'));

                filePath = Path.Combine(_env.WebRootPath, "assets", "img", "blog", fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await blog.MainFile.CopyToAsync(fileStream);
                }
                dbBlog.MainImage = fileName;
            }

            dbBlog.Title = blog.Title.Trim();
            dbBlog.Description = blog.Description;
            dbBlog.UpdatedBy = "Admin";
            dbBlog.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Blog blog = await _context.Blogs.Where(c => !c.IsDeleted).FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (blog == null) return NotFound();

            return View(blog);
        }
        public async Task<IActionResult> DeleteBlog(int? id)
        {
            if (id == null) return BadRequest();

            Blog blog = await _context.Blogs.Where(c => !c.IsDeleted).FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (blog == null) return NotFound();

            blog.IsDeleted = true;
            blog.DeletedBy = "Admin";
            blog.DeletedAt = DateTime.UtcNow.AddHours(4);

            if (blog.MainFile != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "blog", blog.MainImage);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
