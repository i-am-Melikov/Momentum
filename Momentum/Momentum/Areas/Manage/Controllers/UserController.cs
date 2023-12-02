using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Momentum.Areas.Manage.ViewModels.UserVMs;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int currentPage = 1)
        {
            List<AppUser> users = await _userManager.Users.Where(u => u.UserName != User.Identity.Name).ToListAsync();

            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }

            return View(PageNatedList<AppUser>.Create(users.AsQueryable(), currentPage, 5, 5));
        }
        public async Task<IActionResult> SetActive(string? id, int currentPage)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            bool active = appUser.IsActive;
            appUser.IsActive = !active;

            await _userManager.UpdateAsync(appUser);

            List<AppUser> users = await _userManager.Users.Where(u => u.UserName != User.Identity.Name).ToListAsync();

            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }

            return PartialView("_UserSettingsPartial", PageNatedList<AppUser>.Create(users.AsQueryable(), currentPage, 5, 5));
        }
        public async Task<IActionResult> ResetPassword(string? id, int currentPage)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            await _userManager.ResetPasswordAsync(appUser, token, "Welcome123-");

            List<AppUser> users = await _userManager.Users.Where(u => u.UserName != User.Identity.Name).ToListAsync();

            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }

            return PartialView("_UserSettingsPartial", PageNatedList<AppUser>.Create(users.AsQueryable(), currentPage, 5, 5));
        }

        [HttpGet]
        public async Task<IActionResult> ChangeRole(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser? appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();

            var rolesSelectList = new SelectList(roles, "Name", "Name");

            var userRoles = await _userManager.GetRolesAsync(appUser);

            var viewModel = new ChangeRoleVM
            {
                User = appUser,
                SelectedRole = userRoles.FirstOrDefault(), // Set the default selected role
                RolesSelectList = rolesSelectList
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(ChangeRoleVM viewModel)
        {
            //if (!ModelState.IsValid) return BadRequest();
            if (viewModel == null) return BadRequest();
            if (viewModel.User == null) return BadRequest();
            AppUser appUser = await _userManager.FindByIdAsync(viewModel.User.Id);
            if (appUser == null) return BadRequest();
            // Remove the user from the old role (if any)
            var userRoles = await _userManager.GetRolesAsync(appUser);
            await _userManager.RemoveFromRolesAsync(appUser, userRoles);

            // Add the new selected role
            await _userManager.AddToRoleAsync(appUser, viewModel.SelectedRole);

            return RedirectToAction(nameof(Index));
        }

    }
}
