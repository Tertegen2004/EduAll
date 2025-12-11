using EduAll.Areas.Admin.ViewModels;
using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class HomeController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;

        public HomeController(IUniteOfWork unite,UserManager<AppUser> usermanager)
        {
            this.unite = unite;
            this.usermanager = usermanager;
        }
        public async Task<IActionResult> Index()
        {
            var students = await usermanager.GetUsersInRoleAsync(Roles.Student.ToString());
            var instructors = await usermanager.GetUsersInRoleAsync(Roles.Instructor.ToString());
            var courses = unite.Course.GettAll(null,c=>c.Instructor).ToList();
             var orders = unite.Order.GettAll().ToList(); // لو عندك جدول Order

            // 2. تجهيز الموديل
            var model = new Dashboard_vm
            {
                TotalStudents = students.Count(s => s.IsActive == true),
                TotalInstructors = instructors.Count(i=>i.IsActive == true),
                TotalCourses = courses.Count,
                 TotalRevenue = orders.Sum(o => o.TotalPrice), // مثال للأرباح

                // آخر 5 كورسات
                LatestCourses = courses.OrderByDescending(c => c.Id).Take(5)
                    .Select(c => new DashboardCourse_vm
                    {
                        Title = c.Title,
                        Price = c.Price,
                        ImgUrl = c.ImgUrl,
                        Instructor = $"{c.Instructor.FirstName} {c.Instructor.LastName}"
                    }).ToList(),

                // آخر 5 طلاب
                NewStudents = students.OrderByDescending(s => s.Id).Take(5)
                    .Select(s => new DashboardStudent_vm
                    {
                        Name = $"{s.FirstName} {s.LastName}",
                        ImgUrl = s.ImgUrl,
                        JoinDate = s.CreatedAt
                    }).ToList()
            };

            return View(model);
        }
    }
    
}
