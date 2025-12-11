using EduAll.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduAll.ViewComponant
{
    public class WishlistCountViewComponent : ViewComponent
    {
        private readonly IUniteOfWork unite;

        public WishlistCountViewComponent(IUniteOfWork unite)
        {
            this.unite = unite;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return View(0); // لو مش مسجل دخول، العدد صفر
            }

            // عد الكورسات في الوش ليست لهذا المستخدم
            // (استخدمنا LINQ Count مباشرة للأداء)
            var count = unite.Wishlist.GettAll().Count(w => w.UserId == userId);

            // ملحوظة: لو بتستخدم Repository، ممكن تحتاج تضيف ميثود Count فيه
            // var count = await _unitOfWork.Wishlist.CountAsync(w => w.UserId == userId);

            return View(count);
        }
    }
}
