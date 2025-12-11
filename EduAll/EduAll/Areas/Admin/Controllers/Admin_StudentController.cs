using EduAll.Areas.Admin.ViewModels;
using EduAll.Domain;
using EduAll.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class Admin_StudentController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;

        public Admin_StudentController(IUniteOfWork unite,UserManager<AppUser> usermanager)
        {
            this.unite = unite;
            this.usermanager = usermanager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var students = await usermanager.GetUsersInRoleAsync("Student");

            var model = new List<Students_vm>();

            foreach (var student in students)
            {
                var coursesCount = unite.Enrollment.GettAll(e => e.UserId == student.Id).Count();

                model.Add(new Students_vm
                {
                    Id = student.Id,
                    FullName = $"{student.FirstName} {student.LastName}",
                    Email = student.Email,
                    ImgUrl = student.ImgUrl, 
                    EnrolledCoursesCount = coursesCount,
                    IsActive = student.IsActive 
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {

            var student = await usermanager.FindByIdAsync(id);

            var enrollments = unite.Enrollment.GettAll(e => e.UserId == id, e => e.Course).ToList();

            var model = new StudentDetails_vm
            {
                Id = student.Id,
                FullName = $"{student.FirstName} {student.LastName}",
                Email = student.Email,
                PhoneNumber = student.PhoneNumber ?? "N/A",
                Country = student.Country ?? "N/A",
                ImgUrl = student.ImgUrl,
                IsActive = student.IsActive,
                EnrolledCourses = enrollments.Select(e => new StudentCourseInfo
                {
                    CourseName = e.Course.Title,
                    Progress = e.Progress, // المفروض الحقل ده موجود في جدول Enrollment
                    Status = e.Progress >= 100 ? "Completed" : "In Progress"
                }).ToList()

            };


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await usermanager.FindByIdAsync(id);

            user.IsActive = !user.IsActive;

            await usermanager.UpdateAsync(user);

            TempData["SuccessMessage"] = user.IsActive ? "User Activated successfully." : "User Blocked successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
