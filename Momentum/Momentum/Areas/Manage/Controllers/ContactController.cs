using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int currentPage = 1)
        {
            DateTime sevenDaysAgo = DateTime.UtcNow.AddHours(4).AddDays(-7);

            IQueryable<ContactMessage>? messages = _context.ContactMessages
                .Where(m => m.CreatedDate > sevenDaysAgo)
                .OrderByDescending(x=>x.CreatedDate);
            if(messages == null)
            {
                return View();
            }
            return View(PageNatedList<ContactMessage>.Create(messages, currentPage, 20, 20));
        }
    }
}
