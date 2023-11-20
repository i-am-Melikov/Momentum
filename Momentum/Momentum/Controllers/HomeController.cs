using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.ViewModels.HomeVMs;

namespace Momentum.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM
            {
                TopSeller = await _context.Products.Where(s => s.IsDeleted == false && s.IsTopSeller == true).ToListAsync(),
                OurProduct = await _context.Products.Where(s => s.IsDeleted == false && s.IsOurProduct == true).ToListAsync(),
                Blog = await _context.Blogs.Where(b=>b.IsDeleted == false).ToListAsync(),
            };

            return View(homeVM);
        }
    }
}
