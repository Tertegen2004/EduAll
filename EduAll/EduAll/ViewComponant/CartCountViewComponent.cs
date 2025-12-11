using EduAll.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduAll.ViewComponant
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly IUniteOfWork unite;

        public CartCountViewComponent(IUniteOfWork unite)
        {
            this.unite = unite;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return View(0);

            // بنشوف الكارت بتاعة اليوزر، وبنعد الآيتمز اللي جواها
            // SelectMany أو Include مش ضروري لو هنستخدم Count وتصفية ذكية

            var count = await unite.CartItem.GettAll()
                .Where(ci => ci.Cart.UserId == userId) // هات العناصر اللي الكارت بتاعتها تخص اليوزر ده
                .CountAsync();

            return View(count);
        }
    }
}
