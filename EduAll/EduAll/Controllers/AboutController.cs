using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class AboutController : Controller
    {
        private readonly UserManager<AppUser> usermanager;
        private readonly IUniteOfWork unite;

        public AboutController(UserManager<AppUser>usermanager,IUniteOfWork unite)
        {
            this.usermanager = usermanager;
            this.unite = unite;
        }


        [HttpGet]
        
        public async Task<IActionResult> Index()
        {
            // 1. جلب المستخدمين اللي عندهم دور "Instructor"
            var instructors = await usermanager.GetUsersInRoleAsync(Roles.Instructor.ToString());

            // 2. تحويل البيانات للـ ViewModel
            var instructorVMs = new List<InstructorCard_vm>();

            foreach (var user in instructors)
            {
                // ممكن تحتاج تجيب تفاصيل إضافية من الداتا بيز (زي الكورسات)
                // لو العلاقة موجودة في AppUser، ممكن تستخدمها مباشرة
                // لكن GetUsersInRoleAsync بترجع User بس، فممكن نحتاج نعمل Query تاني

                var courses = await unite.Course.GettAll()
                    .Where(c => c.InstructorId == user.Id) // تأكد من اسم الـ FK
                    .Include(c => c.Enrollments)
                    .ToListAsync();

                var studentsCount = courses.Sum(c => c.Enrollments.Count);
                var coursesCount = courses.Count;

                instructorVMs.Add(new InstructorCard_vm
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    ImgUrl = string.IsNullOrEmpty(user.ImgUrl) ? "/assets/images/thumbs/instructor-img1.png" : user.ImgUrl,
                    JobTitle = user.JobTitle ?? "Instructor",
                    CoursesCount = coursesCount,
                    StudentsCount = studentsCount,
                    Rating = 4.8m, // أو تحسبها من التقييمات
                    ReviewsCount = 120, // أو تحسبها
                    FacebookUrl = user.FaceBookUrl, // تأكد إن الخصائص دي موجودة في AppUser
                    TwitterUrl = user.TwitterUrl,
                    InstagramUrl = user.InstagramUrl
                });
            }

            return View(instructorVMs);
        }
    }
}
