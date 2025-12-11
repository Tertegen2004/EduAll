using EduAll.Areas.Admin.ViewModels;
using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> usermanager;
        private readonly SignInManager<AppUser> signInmanager;
        private readonly IUniteOfWork unite;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AccountController(UserManager<AppUser> usermanager,SignInManager<AppUser>signInmanager,IUniteOfWork unite,IWebHostEnvironment webHostEnvironment)
        {
            this.usermanager = usermanager;
            this.signInmanager = signInmanager;
            this.unite = unite;
            this.webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignIn_vm model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await usermanager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // 2. التحقق من كلمة المرور وتسجيل الدخول
                var result = await signInmanager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    if (await usermanager.IsInRoleAsync(user, Roles.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(Register_vm model)
        {
            if (ModelState.IsValid)
            {
                // تجهيز المستخدم
                var user = new AppUser
                {
                    UserName = model.Email, // في Identity الاسم المستعار هو الإيميل عادة
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Country = model.Country,
                    IsActive = true, // تفعيل الحساب مباشرة
                    ImgUrl = "/Admin/assets/images/thumbs/user-img.png" // صورة افتراضية
                };

                // إنشاء المستخدم
                var result = await usermanager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {

                    // إعطاء المستخدم صلاحية "طالب"
                    await usermanager.AddToRoleAsync(user, Roles.Student.ToString());

                    // تسجيل الدخول فوراً
                    await signInmanager.SignInAsync(user, isPersistent: false);

                    // توجيه للصفحة الرئيسية
                    return RedirectToAction("Index", "Home",new {area=""});
                }

                // لو فيه أخطاء (زي الباسورد ضعيف أو الإيميل مكرر)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            await signInmanager.SignOutAsync();
            return View(nameof(SignIn));
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPassword_vm model)
        {
            if (ModelState.IsValid)
            {
                var user = await usermanager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("Email", "Password is wrong");

                    return View(model);
                }
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string Email)
        {
            return View(new ResetPassword_vm { Email = Email});
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword_vm model)
        {
            // 1. التحقق من صحة البيانات (Validation)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. البحث عن المستخدم بواسطة الإيميل
            var user = await usermanager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // لتجنب كشف الحسابات (Enumeration Attacks)، نعيد المستخدم لصفحة التأكيد
                // وكأن العملية تمت بنجاح، حتى لو الإيميل غير موجود
                TempData["ErrorMessage"] = "Email Not Found!";

                return RedirectToAction(nameof(ResetPassword));
            }
            var removeold = await usermanager.RemovePasswordAsync(user);
            // 3. تنفيذ عملية تغيير كلمة المرور
            var result = await usermanager.AddPasswordAsync(user, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password Changed Successfully";

                return RedirectToAction(nameof(SignIn));
            }
            return View(model);
        }

        [HttpGet]
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
                    Bio = user.Bio // تأكد إن الخاصية دي موجودة في AppUser
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

            // تحديث الإيميل (يحتاج توكن وتأكيد في الوضع الطبيعي، لكن هنا هنغيره مباشرة للتسهيل)
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

            // إعادة تحميل الصفحة لتجنب مشاكل الفيو
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AccountSettings_vm model)
        {
            var user = await usermanager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (ModelState.IsValid) // أو افحص model.Password بس
            {
                var result = await usermanager.ChangePasswordAsync(user, model.Password.CurrentPassword, model.Password.NewPassword);

                if (result.Succeeded)
                {
                    await signInmanager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction(nameof(Settings));
                }

                TempData["ErrorMessage"] = "Password Not changed!";
                return RedirectToAction(nameof(Settings),model);
            }

            return RedirectToAction(nameof(Settings));
        }
    }
}
