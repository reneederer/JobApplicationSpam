using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using JobApplicationSpam.Models;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using System;

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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(loginModel.Email);
                if(user != null)
                {
                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await signInManager.PasswordSignInAsync(user, loginModel.Password, false, false);
                    if(result.Succeeded)
                    {
                        return Redirect("/");
                    }
                }
                ModelState.AddModelError(nameof(LoginModel.Email), "Invalid email or password");
                ViewBag.Message = "Invalid email or password";
            }
            return View("Login");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountModel details, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByEmailAsync(details.Email);
                if (existingUser == null)
                {
                    var appUser = new AppUser { UserName = details.Email, Email = details.Email };
                    appUser.PasswordHash = userManager.PasswordHasher.HashPassword(appUser, details.Password);
                    var createResult = await userManager.CreateAsync(appUser);
                    if (createResult.Succeeded)
                    {
                        await signInManager.SignOutAsync();
                        Microsoft.AspNetCore.Identity.SignInResult signInResult =
                            await signInManager.PasswordSignInAsync(appUser, details.Password, false, false);
                        if (signInResult.Succeeded)
                        {
                            return Redirect(returnUrl ?? "/");
                        }
                        else
                        {
                            ModelState.AddModelError(nameof(AccountModel.Email), "Login failed.");
                            ViewBag.Message = "Login failed.";
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(AccountModel.Email), "Failed to create account.");
                        ViewBag.Message = "Failed to create account";
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(AccountModel.Email), "Email already registered.");
                    ViewBag.Message = "Email already registered.";
                }
            }
            else
            {
                ViewBag.Message = "Sorry, the data you entered seems to be invalid.";
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string returnUrl)
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model, string returnUrl)
        {
            var appUser = await userManager.FindByEmailAsync(model.Email);
            if(appUser != null)
            {
                appUser.ChangePasswordGuid = Guid.NewGuid().ToString();
                appUser.ChangePasswordExpiresOn = DateTime.Now + TimeSpan.FromDays(1);
                var updateResult = await userManager.UpdateAsync(appUser);
                if(updateResult.Succeeded)
                {
                    var emailUserName = Startup.Configuration["Data:JobApplicationSpam:Email:UserName"];
                    var emailPassword = Startup.Configuration["Data:JobApplicationSpam:Email:Password"];
                    var emailServer = Startup.Configuration["Data:JobApplicationSpam:Email:Server"];
                    var emailPort = Int32.Parse(Startup.Configuration["Data:JobApplicationSpam:Email:Port"]);
                    var fromName = "Bewerbungsspam";
                    var fromAddress = new MailAddress("info@bewerbungsspam.de", fromName, System.Text.Encoding.UTF8);
                    var subject = "Password recovery";
                    var body = $"Dear user,\n\nyou can set a new password for your account by visiting this link:\nhttps://www.bewerbungsspam.de/Account/ChangePassword?email={model.Email}&changePasswordGuid={appUser.ChangePasswordGuid}\n\nYours dearly,\n\nwww.bewerbungsspam.de";

                    using (var smtpClient = new SmtpClient(emailServer, emailPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new System.Net.NetworkCredential(emailUserName, emailPassword);
                        var toAddress = new MailAddress(model.Email);
                        var message = new MailMessage(fromAddress, toAddress) { SubjectEncoding = System.Text.Encoding.UTF8, Subject = subject, Body = body, BodyEncoding = System.Text.Encoding.UTF8 };
                        /*
                        let attachments =
                            attachmentPathsAndNames
                            |> List.map(fun(filePath, fileName)->
                                let attachment = new Attachment(filePath)
                                attachment.Name < -fileName
                                message.Attachments.Add(attachment)
                                attachment)
                                */
                        smtpClient.Send(message);

                    }
                }
                else
                {
                    ViewBag.Message = "Sorry, an error occurred.";
                }
            }
            else
            {
                ViewBag.Message = "I could not find your email.";
            }
            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string returnUrl)
        {
            if(!Request.Query.ContainsKey("email") || !Request.Query.ContainsKey("changePasswordGuid"))
            {
                return View();
            }
            else
            {
                var email = Request.Query["email"];
                var guid = Request.Query["changePasswordGuid"];
                var appUser = await userManager.FindByEmailAsync(email);
                if (guid == appUser.ChangePasswordGuid)
                {
                    await signInManager.SignOutAsync();
                    await signInManager.SignInAsync(appUser, false);
                    return RedirectToAction("ChangePassword", "Account");
                }
                else
                {
                    return View();
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel changePasswordModel, string returnUrl)
        {
            var appUser = await userManager.GetUserAsync(HttpContext.User);
            if(appUser != null)
            {
                appUser.PasswordHash = userManager.PasswordHasher.HashPassword(appUser, changePasswordModel.Password);
                var result1 = await userManager.UpdateAsync(appUser);
                if(result1.Succeeded)
                {
                    return Redirect("/");
                }
            }
            return View("ChangePassword");
        }


        [AllowAnonymous]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            await signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}
