﻿using Microsoft.AspNetCore.Mvc;
using JobApplicationSpam.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace JobApplicationSpam.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<AppUser> userManager;
        public AdminController(UserManager<AppUser> usrMgr)
        {
            userManager = usrMgr;
        }
        public ViewResult Index() =>
            View(userManager.Users);

        public ViewResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser { UserName = model.Name, Email = model.Email };
                IdentityResult result =
                    await userManager.CreateAsync(user, model.Password);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found.");
            }
            return View("Index", userManager.Users);
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}