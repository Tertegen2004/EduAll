using EduAll.Areas.Admin.ViewModels;
using EduAll.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduAll.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> usermanager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly SignInManager<AppUser> signInmanager;

        public AccountController(UserManager<AppUser> usermanager,IWebHostEnvironment webHostEnvironment,SignInManager<AppUser> signInmanager)
        {
            this.usermanager = usermanager;
            this.webHostEnvironment = webHostEnvironment;
            this.signInmanager = signInmanager;
        }
        public async Task<IActionResult> Settings()
        {
            var user = await usermanager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new AccountSettings_vm
            {
                Profile = new ProfileSettings_vm
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    JobTitle = user.JobTitle,
                    ImgUrl = user.ImgUrl,
                    Bio = user.Bio 
                }
            };

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> UpdateProfile(AccountSettings_vm model)
        {
            var user = await usermanager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // تحديث الصورة
            if (model.Profile.ImageFile != null)
            {
                string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "images/users");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Profile.ImageFile.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Profile.ImageFile.CopyToAsync(fileStream);
                }

                user.ImgUrl = "/images/users/" + fileName;
            }

            // تحديث البيانات
            user.FirstName = model.Profile.FirstName;
            user.LastName = model.Profile.LastName;
            user.PhoneNumber = model.Profile.PhoneNumber;
            user.JobTitle = model.Profile.JobTitle;
            user.Bio = model.Profile.Bio;

            
            if (user.Email != model.Profile.Email)
            {
                user.Email = model.Profile.Email;
                user.UserName = model.Profile.Email;
            }

            var result = await usermanager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

           
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AccountSettings_vm model)
        {
            var user = await usermanager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (ModelState.IsValid) 
            {
                var result = await usermanager.ChangePasswordAsync(user, model.Password.CurrentPassword, model.Password.NewPassword);

                if (result.Succeeded)
                {
                    await signInmanager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction(nameof(Settings));
                }

                TempData["ErrorMessage"] = "Password Not changed!";
                return RedirectToAction(nameof(Settings), model);
            }

            return RedirectToAction(nameof(Settings));
        }
    }
}
