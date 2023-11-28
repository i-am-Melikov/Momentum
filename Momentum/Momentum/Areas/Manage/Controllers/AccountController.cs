using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Momentum.Areas.Manage.ViewModels.AccountVMs;
using Momentum.Models;

namespace Momentum.Areas.Manage.Controllers
{
    [Area("manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            //IdentityErrorDescriber

            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            AppUser appUser = new AppUser
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
            };

            //if(await _userManager.Users.AnyAsync(u => u.NormalizedUserName == registerVM.UserName.Trim().ToUpperInvariant()))
            //{
            //	ModelState.AddModelError("UserName", $"'{registerVM.UserName}' already exists.");
            //	return View(registerVM);
            //}
            //         if (await _userManager.Users.AnyAsync(u => u.Email == registerVM.UserName.Trim().ToUpperInvariant()))
            //         {
            //             ModelState.AddModelError("Email", $"'{registerVM.Email}' already exists.");
            //             return View(registerVM);
            //         }



            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);
            //menimsetdiyimiz butun deyerleri bura gonderirikki yoxluyaq bizim verdiyimiz shertlerle uyqunlashir yoxsa yox



            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                    //summary error
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, "Admin");

            return RedirectToAction(nameof(Login));
        }

        //CreateSuperAdmin
        //[HttpGet]
        //public async Task<IActionResult> CreateSuperAdmin()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Email = "superadmin@gmail.com",
        //        UserName = "superadmin",
        //    };
        //    await _userManager.CreateAsync(appUser, "SuperAdminP235");
        //    await _userManager.CreateAsync(appUser, "SuperAdmin");

        //    return Ok("Super Admin Yarandi");
        //}

        //[HttpGet]
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));


        //    return Ok("Rollar yarandi");
        //}

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            AppUser appUser = await _userManager.FindByEmailAsync(loginVM.Email);
            if (appUser == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect");
                return View(loginVM);
            }

            //if(await _userManager.CheckPasswordAsync(appUser,loginVM.Password))
            //{
            //             ModelState.AddModelError("", "Email or password is incorrect");
            //             return View(loginVM);
            //         }

            if (appUser.IsActive)
            {
                return Unauthorized();
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RemindMe, true);
            //paswordsigninasync avtomatik olaraq login olandan sonra ozu tokeni refreshliyir ve yeni token verir

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Email or password is incorrect");
                return View(loginVM);
            }
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account was blocked");
                return View(loginVM);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "manage" });
        }
        [HttpGet]
        //[Authorize(Roles = "SuperAdmin,Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (appUser == null) return BadRequest();

            ProfileVM profileVM = new ProfileVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                Email = appUser.Email,
                UserName = appUser.UserName
            };

            return View(profileVM);
        }
        [HttpPost]
        //[Authorize(Roles = "SuperAdmin,Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM profileVM)
        {
            if (!ModelState.IsValid) return View(profileVM);

            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            //if (await _userManager.Users.AnyAsync(u => u.NormalizedUserName == profileVM.UserName.Trim().ToUpperInvariant() && u.Id != appUser.Id))
            //	//invariant utf-8 yeni azrebaycan shriftlerinide goturur
            //{
            //	ModelState.AddModelError("UserName", "Already Exists");
            //}
            //if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == profileVM.Email.Trim().ToUpperInvariant() && u.Id != appUser.Id))
            //         //invariant utf-8 yeni azrebaycan shriftlerinide goturur
            //         {
            //             ModelState.AddModelError("Email", "Already Exists");
            //         }

            if (appUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant())
            //invariant utf-8 yeni azrebaycan shriftlerinide goturur
            {
                appUser.UserName = profileVM.UserName.Trim();
            }
            if (appUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant())
            //invariant utf-8 yeni azrebaycan shriftlerinide goturur
            {
                appUser.Email = profileVM.Email.Trim();
            }

            //appUser.Email = profileVM.Email.Trim();
            //appUser.UserName = profileVM.UserName.Trim();
            appUser.Name = profileVM.Name;
            appUser.SurName = profileVM.SurName;

            IdentityResult identityResult = await _userManager.UpdateAsync(appUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(profileVM);
            }
            if (!string.IsNullOrWhiteSpace(profileVM.CurrentPassword))
            {
                if (!await _userManager.CheckPasswordAsync(appUser, profileVM.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPasword", "Current Password is incorrect");
                }
                string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                identityResult = await _userManager.ResetPasswordAsync(appUser, token, profileVM.NewPassword);

                if (!identityResult.Succeeded)
                {
                    foreach (IdentityError identityError in identityResult.Errors)
                    {
                        ModelState.AddModelError("", identityError.Description);
                    }
                    return View(profileVM);
                }
            }

            await _signInManager.SignInAsync(appUser, true);
            //propertilerin deyerlerini deyishende yeni tokenide deyismesi ucun signinasync yaziriq

            return RedirectToAction("Index", "Dashboard", new { area = "manage" });
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            //sign out async gedir login tokenini silir neticede avtomatik hesabdan cixir
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }
    }
}
