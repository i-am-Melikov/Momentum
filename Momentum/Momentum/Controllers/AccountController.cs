using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework.Profiler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Momentum.DataAccess;
using Momentum.Models;
using Momentum.ViewModels;
using Momentum.ViewModels.AccountVMs;

namespace Momentum.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly SmtpSetting _smtpSetting;
        private readonly IWebHostEnvironment _env;
        public AccountController(UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            IConfiguration config,
            IOptions<SmtpSetting> options,
            IWebHostEnvironment env,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
            _smtpSetting = options.Value;
            _env = env;
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(ViewModels.AccountVMs.RegisterVM registerVM)
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

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                    //summary error
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, "Member");
            string templateFullPath = Path.Combine(_env.WebRootPath, "templates", "EmailConfirm.html");
            string templateContent = await System.IO.File.ReadAllTextAsync(templateFullPath);

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            string url = Url.Action("EmailConfirm", "Account", new { id = appUser.Id, token = token }, Request.Scheme, Request.Host.ToString());

            templateContent = templateContent.Replace("{{url}}", url);

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Email Confirm";
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = templateContent
            };

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }

            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(ViewModels.AccountVMs.LoginVM loginVM)
        {
            //if (!ModelState.IsValid) return View(loginVM);

            AppUser appUser = await _userManager.FindByEmailAsync(loginVM.Email);
            if (appUser == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect");
                return View(loginVM);
            }
            if (loginVM.Password == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect");
                return View(loginVM);
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RemindMe, true);

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

            return RedirectToAction("Index", "Home");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> EmailConfirm(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, token);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(nameof(Login));
            }

            await _signInManager.SignInAsync(appUser, true);

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
               .Include(u => u.Orders.Where(o => o.IsDeleted == false)).ThenInclude(o => o.OrderProducts.Where(op => op.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser == null) return RedirectToAction(nameof(Login));
            ProfileVM profileVM = new ProfileVM();
            
            if (appUser.Addresses != null)
            {
                profileVM.Addresses = appUser.Addresses;
            }
            profileVM.ProfileAccountVM = new ProfileAccountVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                Email = appUser.Email,
                UserName = appUser.UserName
            };
            profileVM.Orders = appUser.Orders;

            return View(profileVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileAccount(ProfileAccountVM profileAccountVM)
        {
            TempData["Tab"] = "Account";
            AppUser appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser == null) return RedirectToAction(nameof(Login));

            ProfileVM profileVM = new ProfileVM();
            if(appUser.Addresses != null)
            {
                profileVM.Addresses = appUser.Addresses;
            }
            profileVM.ProfileAccountVM = profileAccountVM;

            if (!ModelState.IsValid)
            {
                return View("Profile", profileVM);
            }


            if (appUser.NormalizedUserName != profileAccountVM.UserName.Trim().ToUpperInvariant())
            {
                appUser.UserName = profileAccountVM.UserName;
            }
            if (appUser.NormalizedEmail != profileAccountVM.Email.Trim().ToUpperInvariant())
            {
                appUser.Email = profileAccountVM.Email;
            }

            appUser.Name = profileAccountVM.Name;
            appUser.SurName = profileAccountVM.SurName;

            IdentityResult identityResult = await _userManager.UpdateAsync(appUser);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View("Profile", profileVM);
            }
            if (profileAccountVM.CurrentPassword != null && profileAccountVM.NewPassword!= null && profileAccountVM.ConfirmPassword != null && profileAccountVM.NewPassword==profileAccountVM.ConfirmPassword)
            {
                if(await _userManager.CheckPasswordAsync(appUser, profileAccountVM.CurrentPassword) && profileAccountVM.CurrentPassword != null)
                {
                    var result = await _userManager.ChangePasswordAsync(appUser, profileAccountVM.CurrentPassword, profileAccountVM.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please check the passwords inputs");
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            await _signInManager.SignInAsync(appUser, true);

            return RedirectToAction(nameof(Profile));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Member")]
        public async Task<IActionResult> AddAddress(Address address)
        {
            TempData["Tab"] = "Address";

            AppUser? appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser == null) RedirectToAction(nameof(Login));

            ProfileVM profileVM = new ProfileVM();
            profileVM.ProfileAccountVM = new ProfileAccountVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                Email = appUser.Email,
                UserName = appUser.UserName
            };
            if(appUser.Addresses != null)
            {
                profileVM.Addresses = appUser.Addresses;
            }

            if (!ModelState.IsValid)
            {
                profileVM.Address = address;
                TempData["addreess"] = "true";
                return View("Profile", profileVM);
            }

            if (address.IsDefault == true)
            {
                if (appUser.Addresses != null && appUser.Addresses.Count() > 0)
                {
                    foreach (Address address1 in appUser.Addresses)
                    {
                        address1.IsDefault = false;
                    }
                }
            }
            else
            {
                if (appUser.Addresses == null || appUser.Addresses.Count() <= 0)
                {
                    address.IsDefault = true;
                }
            }

            address.UserId = appUser.Id;
            address.CreatedBy = appUser.Name + " " + appUser.SurName;
            address.CreatedAt = DateTime.Now;

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

    }
}
