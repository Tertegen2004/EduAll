using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class InstructorController : Controller
    {
        private readonly UserManager<AppUser> usermanager;
        private readonly IUniteOfWork unite;

        public InstructorController(UserManager<AppUser> usermanager,IUniteOfWork unite)
        {
            this.usermanager = usermanager;
            this.unite = unite;
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // 1. جلب بيانات المدرب
            var instructor = await usermanager.FindByIdAsync(id);
            if (instructor == null) return NotFound();

            // 2. جلب كورسات المدرب
            var courses = await unite.Course.GettAll()
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .Where(c => c.InstructorId == id)
                .ToListAsync();

            // 3. حساب الإحصائيات
            var studentsCount = courses.Sum(c => c.Enrollments.Count);

            // 4. تحويل الكورسات للـ VM (استخدم نفس المابينج القديم أو دالة مساعدة)
            var coursesVM = courses.Select(c => new HomeCourse_vm
            {
                Id = c.Id,
                Title = c.Title,
                ImgUrl = c.ImgUrl,
                CategoryName = c.Category?.Name ?? "General",
                // InstructorName مش محتاجينها هنا لأننا في صفحة المدرب أصلاً
                Price = c.Price,
                LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count(),
                DurationHours = 10, // حساب تقريبي
                StudentsCount = c.Enrollments.Count,
                // تأكد من حساب IsWishlisted لو اليوزر الحالي مسجل دخول (زي ما عملنا في الهوم)
            }).ToList();

            // 5. بناء الموديل النهائي
            var model = new InstructorDetails_vm
            {
                Id = instructor.Id,
                FullName = $"{instructor.FirstName} {instructor.LastName}",
                ImgUrl = string.IsNullOrEmpty(instructor.ImgUrl) ? "/assets/images/thumbs/instructor-details-thumb.png" : instructor.ImgUrl,
                JobTitle = instructor.JobTitle ?? "Instructor",
                Bio = instructor.Bio ?? "No bio available.",
                Email = instructor.Email,
                Phone = instructor.PhoneNumber ?? "Not Available",
                Address = instructor.Country ?? "Not Available", // تأكد إن Address موجودة في AppUser

                FacebookUrl = instructor.FaceBookUrl,
                TwitterUrl = instructor.TwitterUrl,
                InstagramUrl = instructor.InstagramUrl, // تأكد إنها موجودة في AppUser

                CoursesCount = courses.Count,
                StudentsCount = studentsCount,
                Rating = 4.8m, // قيمة افتراضية
                ReviewsCount = 120, // قيمة افتراضية

                Courses = coursesVM
            };

            return View(model);
        }
    }
}
