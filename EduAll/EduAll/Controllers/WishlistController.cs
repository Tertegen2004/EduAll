using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;

        public WishlistController(IUniteOfWork unite,UserManager<AppUser>usermanager)
        {
            this.unite = unite;
            this.usermanager = usermanager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = usermanager.GetUserId(User);

            var wishlistCourses = await unite.Wishlist.GettAll()
                .Include(w => w.Course).ThenInclude(c => c.Category)
                .Include(w => w.Course).ThenInclude(c => c.Instructor)
                .Include(w => w.Course).ThenInclude(c => c.Sections).ThenInclude(s => s.Lessons)
                .Where(w => w.UserId == userId)
                .Select(w => new HomeCourse_vm
                {
                    Id = w.CourseId,
                    Title = w.Course.Title,
                    ImgUrl = w.Course.ImgUrl,
                    CategoryName = w.Course.Category.Name,
                    InstructorName = $"{w.Course.Instructor.FirstName} {w.Course.Instructor.LastName}",
                    InstructorImg = w.Course.Instructor.ImgUrl,
                    Price = w.Course.Price,
                    Rating = 4.8m, // قيمة افتراضية
                    LessonsCount = w.Course.Sections.SelectMany(s => s.Lessons).Count(),
                    DurationHours = w.Course.Duration, // قيمة افتراضية
                    Level = w.Course.Level,
                    IsWishlisted = true // طبعاً لأننا في صفحة الوش ليست
                })
            .ToListAsync();

            return View(new Wishlist_vm { Courses = wishlistCourses });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAll()
        {
            var userId = usermanager.GetUserId(User);
            var items = await unite.Wishlist.GettAll()
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (items.Any())
            {
                await unite.Wishlist.DeleteAll(items); // تأكد إن الريبو فيه RemoveRange
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int courseId)
        {
            var user = await usermanager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            var existingItem = await unite.Wishlist.GettAll()
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.CourseId == courseId);

            bool isAdded;
            if (existingItem != null)
            {
                await unite.Wishlist.Delete(existingItem.Id);
                isAdded = false;
            }
            else
            {
                var newItem = new Wishlist { UserId = user.Id, CourseId = courseId };
                await unite.Wishlist.Create(newItem);
                isAdded = true;
            }

            return Json(new { success = true, isAdded = isAdded, message = isAdded ? "Added to Wishlist" : "Removed from Wishlist" });
        }
    }
}
