using EduAll.Areas.Admin.ViewModels;
using EduAll.Constant;
using EduAll.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class AdminSuggestionController : Controller
    {
        private readonly IUniteOfWork unite;

        public AdminSuggestionController(IUniteOfWork unite)
        {
            this.unite = unite;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suggestions = await unite.CourseSuggestion.GettAll()
                .Include(s => s.User)
            .Include(s => s.Course)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new Suggestion_vm
                {
                    Id = s.Id,
                    StudentName = $"{s.User.FirstName} {s.User.LastName}",
                    StudentImage = s.User.ImgUrl,
                    CourseName = s.Course.Title,
                    Content = s.Content,
                    CreatedAt = s.CreatedAt,
                    Status = s.Status.ToString(),
                    AdminResponse = s.AdminResponseNote
                }).ToListAsync();

            return View(suggestions);
        }

        // 2. الرد على الاقتراح (Approve/Reject)
        [HttpPost]
        [ValidateAntiForgeryToken] // أمان إضافي
        public async Task<IActionResult> Respond(int id, string response, string status)
        {
            // 1. جلب الاقتراح من الداتا بيز
            var suggestion = await unite.CourseSuggestion.FindById(id);

            if (suggestion == null)
            {
                TempData["ErrorMessage"] = "Suggestion not found.";
                return RedirectToAction(nameof(Index));
            }

            // 2. تحديث الرد
            suggestion.AdminResponseNote = response;

            // 3. تحديث الحالة بناءً على الزر المضغوط
            // لاحظ: الأسماء هنا (Approve, Reject) لازم تطابق الـ value في الـ HTML بالظبط
            if (status == "Approve")
            {
                suggestion.Status = SuggestionState.Approved;
            }
            else if (status == "Reject")
            {
                suggestion.Status = SuggestionState.Rejected;
            }
            else
            {
                // لو القيمة جت غلط لأي سبب، ممكن نطلع خطأ أو نسيبها Pending
                TempData["ErrorMessage"] = "Invalid status selected.";
                return RedirectToAction(nameof(Index));
            }

            // 4. حفظ التغييرات في الداتا بيز (أهم خطوة!)
            try
            {
                await unite.CourseSuggestion.Update(suggestion.Id);
                // await unite.CompleteAsync(); // لو الريبو بتاعك بيحتاج Complete

                TempData["SuccessMessage"] = "Response sent successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating suggestion.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
