using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using JobApplicationSpam.Models;
using System.Threading.Tasks;

namespace JobApplicationSpam.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> userMgr, SignInManager<AppUser> signInMgr)
        {
            userManager = userMgr;
            signInManager = signInMgr;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountModel details, string returnUrl)
        {
            /*
            var appUser = new AppUser { UserName = details.Email, Email = details.Email };
            appUser.PasswordHash = userManager.PasswordHasher.HashPassword(appUser, details.Password);
            var result1 = await userManager.CreateAsync(appUser);
            */
            if(ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(details.Email);
                if(user != null)
                {
                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await signInManager.PasswordSignInAsync(user, details.Password, false, false);
                    if(result.Succeeded)
                    {
                        return View(details);
                        //return Redirect(returnUrl ?? "/");
                    }
                }
                ModelState.AddModelError(nameof(AccountModel.Email), "Invalid email or password");
            }
            return View(details);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountModel details, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                var existingUser = await userManager.FindByEmailAsync(details.Email);
                if (existingUser == null)
                {
                    var appUser = new AppUser { UserName = details.Email, Email = details.Email };
                    appUser.PasswordHash = userManager.PasswordHasher.HashPassword(appUser, details.Password);
                    var result1 = await userManager.CreateAsync(appUser);
                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await signInManager.PasswordSignInAsync(appUser, details.Password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(returnUrl ?? "/");
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(AccountModel.Email), "Invalid email or password.");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(AccountModel.Email), "Email already registered.");
                }
            }
            return View(details);
        }


    }
}
