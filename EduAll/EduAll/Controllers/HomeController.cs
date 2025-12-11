using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUniteOfWork unite;

        public HomeController(IUniteOfWork unite)
        {
            this.unite = unite;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. جلب الأقسام (أول 6 مثلاً)
            var categories = await unite.Category.GettAll()
            .Include(c => c.Courses) // عشان نعرف نعد الكورسات
            .Take(8)
            .ToListAsync();

            var categories_vm = categories.Select(c => new HomeCategory_vm
            {
                Id = c.Id,
                Name = c.Name,
                DataFilter = GetFilterClass(c.Name),
                IconUrl = string.IsNullOrEmpty(c.Img) ? "/assets/images/icons/category-icon1.png" : c.Img,
                CoursesCount = c.Courses.Count(),
                BgColorClass = (c.Id % 3 == 0) ? "bg-main-three-25" : (c.Id % 2 == 0) ? "bg-main-two-25" : "bg-main-25"
            }).ToList();

            // 2. جلب الكورسات الشائعة (الأعلى تقييماً أو الأحدث)
            var popularCourses = await unite.Course.GettAll()
                .Include(c => c.Category)
                .Include(c=>c.Enrollments)
                .Include(c => c.Instructor)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .OrderByDescending(c => c.Enrollments.Count) 
                .Take(6)
                .ToListAsync();

            var course_vm = popularCourses.Select(c => new HomeCourse_vm
            {
                Id = c.Id,
                Title = c.Title,
                ImgUrl = c.ImgUrl,
                CategoryName = c.Category.Name,
                CategoryFilter = GetFilterClass(c.Category.Name),
                InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                InstructorImg = c.Instructor.ImgUrl,
                Level = c.Level, 
                StudentsCount = c.Enrollments.Count(),
                Price = c.Price,
                Rating = 4.8m, // قيمة افتراضية لحد ما تعمل Reviews
                LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count(),
                DurationHours = c.Duration
            }).ToList();

            var model = new HomeIndex_vm
            {
                Categories = categories_vm,
                PopularCourses = course_vm
            };

            return View(model);
        }



        private string GetFilterClass(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return name.ToLower().Trim()
                .Replace(" ", "-")
                .Replace("&", "")
                .Replace(".", "")
                .Replace("/", "");
        }
    }
}
