using EduAll.Constant;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduAll.ViewComponant
{
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly IUniteOfWork unite;

        public NotificationsViewComponent(IUniteOfWork unite)
        {
            this.unite = unite;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Content("");

            List<Notification_vm> notifications = new List<Notification_vm>();

            // =========================================================
            // 1. لو المستخدم "أدمن": هات الاقتراحات الجديدة (Pending)
            // =========================================================
            if (UserClaimsPrincipal.IsInRole("Admin"))
            {
                var pendingSuggestions = await unite.CourseSuggestion.GettAll()
                    .Include(s => s.Course)
                    .Include(s => s.User) // عشان نعرف مين الطالب
                    .Where(s => s.Status == SuggestionState.Pending)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                notifications = pendingSuggestions.Select(s => new Notification_vm
                {
                    Id = s.Id,
                    Message = "New Suggestion Submitted",
                    Detail = $"{s.User.FirstName} suggested edit for {s.Course.Title}",
                    CourseName = s.Course.Title,
                    Time = s.CreatedAt,
                    IsApproved = false, // نستخدم دي عشان نغير لون الأيقونة (أصفر مثلاً للانتظار)
                    // رابط يودي لصفحة مراجعة الاقتراحات (هتحتاج تعمل الكنترولر ده)
                    Link = Url.Action("Index", "AdminSuggestion", new { area = "Admin" })
                }).ToList();
            }
            // =========================================================
            // 2. لو المستخدم "طالب": هات الردود (Approved/Rejected)
            // =========================================================
            else
            {
                var mySuggestions = await unite.CourseSuggestion.GettAll()
                    .Include(s => s.Course)
                    .Where(s => s.UserId == userId && s.Status != SuggestionState.Pending)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                notifications = mySuggestions.Select(s => new Notification_vm
                {
                    Id = s.Id,
                    Message = s.Status == SuggestionState.Approved ? "Suggestion Approved! 🎉" : "Suggestion Rejected 😔",
                    Detail = s.AdminResponseNote ?? "Click to see details",
                    CourseName = s.Course.Title,
                    Time = s.CreatedAt,
                    IsApproved = s.Status == SuggestionState.Approved,
                    Link = Url.Action("Details", "Course", new { id = s.CourseId, area = "" })
                }).ToList();
            }

            return View(notifications);
        }
    }
}
