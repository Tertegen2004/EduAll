using EduAll.Areas.Admin.ViewModels;
using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static EduAll.Areas.Admin.ViewModels.InstructorDetails_vm;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class Admin_InstructorController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public Admin_InstructorController(IUniteOfWork unite,UserManager<AppUser> usermanager,IWebHostEnvironment webHostEnvironment)
        {
            this.unite = unite;
            this.usermanager = usermanager;
            this.webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var Instructors = await usermanager.GetUsersInRoleAsync("Instructor");

            var model = new List<Instructor_vm>();

            foreach (var instructor in Instructors)
            {
                var coursesCount = unite.Course.GettAll(e => e.InstructorId == instructor.Id).Count();

                model.Add(new Instructor_vm
                {
                    Id = instructor.Id,
                    FullName = $"{instructor.FirstName} {instructor.LastName}",
                    JobTitle = instructor.JobTitle,
                    Email = instructor.Email,
                    ImgUrl = instructor.ImgUrl,
                    CreatedCoursesCount = coursesCount,
                    IsActive = instructor.IsActive
                });
            }

            return View(model);

        }

        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await usermanager.FindByIdAsync(id);

            user.IsActive = !user.IsActive;

            await usermanager.UpdateAsync(user);

            TempData["SuccessMessage"] = user.IsActive ? "User Activated successfully." : "User Blocked successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInstructor_vm model)
        {
            if (ModelState.IsValid)
            {
                // 1. رفع الصورة (اختياري)
                string? imgPath = null;
                if (model.ImageFile != null)
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "images/users");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    imgPath = "/images/users/" + fileName;
                }

                // 2. إنشاء المستخدم
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    Country = model.Country,
                    ImgUrl = imgPath,
                    EmailConfirmed = true, // مفعل تلقائياً
                    IsActive = true
                };

                var result = await usermanager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 3. إضافة الرول
                    await usermanager.AddToRoleAsync(user, Roles.Instructor.ToString());

                    TempData["SuccessMessage"] = "Instructor added successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // لو فيه أخطاء (زي الباسورد ضعيف)
                foreach (var error in result.Errors)
                {
                    TempData["ErrorMessage"] += error.Description + " ";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid data, please check inputs.";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var instructor = await unite.Instructor.SelectBy(i=>i.Id == id);

            var courses = unite.Course.GettAll(c => c.InstructorId == id)
                 .Select(c => new CourseSummary_vm
                 {
                      Title = c.Title,
                      StudentsCount = c.Enrollments.Count,
                      Rating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0
                 }).ToList();

            var model = new InstructorDetails_vm
            {
                Id = instructor.Id,
                FullName = $"{instructor.FirstName} {instructor.LastName}",
                Email = instructor.Email,
                PhoneNumber = instructor.PhoneNumber ?? "N/A",
                Country = instructor.Country ?? "N/A",
                ImgUrl = instructor.ImgUrl,
                JobTitle = instructor.JobTitle ?? "Instructor",
                IsActive = instructor.IsActive,
                TotalCourses = courses.Count,
                TotalStudents = courses.Sum(c => c.StudentsCount),
                CreatedCourses = courses
            };

            return View(model);
        }
    }
}
