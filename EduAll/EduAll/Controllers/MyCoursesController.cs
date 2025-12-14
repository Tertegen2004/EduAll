using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    [Authorize]
    public class MyCoursesController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;

        public MyCoursesController(IUniteOfWork unite, UserManager<AppUser> usermanager)
        {
            this.unite = unite;
            this.usermanager = usermanager;
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = usermanager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            
            var enrollments = await unite.Enrollment.GettAll()
                .Include(e => e.Course).ThenInclude(c => c.Category)
                .Include(e => e.Course).ThenInclude(c => c.Instructor)
                .Include(e => e.Course).ThenInclude(c => c.Sections).ThenInclude(s => s.Lessons)
                .Include(e => e.Course).ThenInclude(c => c.Reviews)
                .Include(e => e.Course).ThenInclude(c => c.Enrollments) // عشان نعد الطلاب
                .Where(e => e.UserId == userId)
                .ToListAsync();

           
            var model = new MyCourses_vm
            {
                // الكورسات الجارية (أقل من 100%)
                OngoingCourses = enrollments
                    .Where(e => e.Progress < 100)
                    .Select(e => MapToVM(e.Course, e.Progress))
                    .ToList(),

                // الكورسات المكتملة (100%)
                CompletedCourses = enrollments
                    .Where(e => e.Progress >= 100)
                    .Select(e => MapToVM(e.Course, e.Progress))
                    .ToList(),

               
            };

            return View(model);
        }

        // =========================================================
        // دالة التحويل (Mapping)
        // =========================================================
        private Course_vm MapToVM(Course c, int progress)
        {
            if (c == null) return new Course_vm();

            return new Course_vm
            {
                Id = c.Id,
                Title = c.Title,
                ImgUrl = string.IsNullOrEmpty(c.ImgUrl) ? "/assets/images/thumbs/course-img1.png" : c.ImgUrl,

                // البيانات الأساسية
                Category = c.Category?.Name ?? "General",
                InstructorName = c.Instructor != null ? $"{c.Instructor.FirstName} {c.Instructor.LastName}" : "Unknown",
                InstructorImg = c.Instructor?.ImgUrl,
                Level = c.Level.ToString(),

                // الإحصائيات (مؤمنة ضد Null)
                LessonsCount = c.Sections?.Sum(s => s.Lessons?.Count ?? 0) ?? 0,
                StudentsCount = c.Enrollments?.Count ?? 0,
                Duration = c.Duration,
                Rating = (c.Reviews != null && c.Reviews.Any()) ? c.Reviews.Average(r => r.Rating) : 0,

                // بيانات التقدم
                Progress = progress,
                Price = c.Price // مش هتفرق أوي هنا بس خليها لو احتجتها
            };
        }
    }

}



