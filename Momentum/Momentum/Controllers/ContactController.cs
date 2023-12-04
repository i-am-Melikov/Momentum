using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momentum.DataAccess;
using Momentum.Interfaces;
using Momentum.Models;

namespace Momentum.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILayoutService _layoutService;

        public ContactController(ILayoutService layoutService, AppDbContext context)
        {
            _layoutService = layoutService;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> settings = await _layoutService.GetSettingAsync();

            return View(settings);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(string name, string surname, string email, string number, string description)
        {
            if (name == null || surname == null || email == null || number == null || description == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect");
                return RedirectToAction(nameof(Index));
            }
            ContactMessage message = new ContactMessage
            {
                Name = name,
                SurName = surname,
                Email = email,
                Number = number,
                Descpition = description,
                CreatedDate = DateTime.UtcNow
            };
            _context.ContactMessages.Add(message);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
