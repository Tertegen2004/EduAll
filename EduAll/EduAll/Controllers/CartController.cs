using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<AppUser> usermanager;
        private readonly IUniteOfWork unite;

        public CartController(UserManager<AppUser>usermanager,IUniteOfWork unite)
        {
            this.usermanager = usermanager;
            this.unite = unite;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = usermanager.GetUserId(User);

            // 1. جلب الكارت الخاصة باليوزر
            var cart = await unite.Cart.GettAll() // أو Use _context directly if Include is easier
                .Include(c => c.CartItems).ThenInclude(ci => ci.Course).ThenInclude(co => co.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return View(new Cart_vm()); // سلة فارغة
            }

            // 2. تحويل البيانات للـ ViewModel
            var model = new Cart_vm
            {
                Items = cart.CartItems.Select(ci => new CartItem_vm
                {
                    CartItemId = ci.Id,
                    CourseId = ci.CourseId,
                    CourseTitle = ci.Course.Title,
                    CourseImg = string.IsNullOrEmpty(ci.Course.ImgUrl) ? "/assets/images/thumbs/course-img1.png" : ci.Course.ImgUrl,
                    CategoryName = ci.Course.Category?.Name ?? "General",
                    Price = ci.Course.Price
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            // هنا id هو CartItemId
            var item = await unite.CartItem.FindById(id);
            if (item != null)
            {
                await unite.CartItem.Delete(item.Id);
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> Add(int courseId)
        {
            var userId = usermanager.GetUserId(User);
            if (userId == null) return Json(new { success = false, message = "Login required" });

            var isEnrolled = await unite.Enrollment.GettAll()
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (isEnrolled)
            {
                return Json(new { success = false, message = "You already own this course! Check 'My Courses'." });
            }

            // 1. البحث عن الكارت الخاصة بالمستخدم
            var cart = await unite.Cart.GettAll()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // 2. لو مفيش كارت، نكريت واحدة جديدة
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    UpdatedAt = DateTime.Now,
                    CartItems = new List<CartItem>() // تهيئة الليست
                };
                await unite.Cart.Create(cart);
            }

            // 3. التأكد إن الكورس مش موجود في الكارت دي
            if (cart.CartItems.Any(i => i.CourseId == courseId))
            {
                return Json(new { success = false, message = "Course already in cart!" });
            }

            // 4. إضافة الكورس (CartItem)
            var cartItem = new CartItem
            {
                CartId = cart.Id, // ربطناه بالكارت اللي جبناها
                CourseId = courseId
            };

            await unite.CartItem.Create(cartItem);

            // تحديث وقت الكارت
            cart.UpdatedAt = DateTime.Now;

            // 5. حساب العدد الجديد للكارت
            var newCount = await unite.CartItem.GettAll()
                .Where(ci => ci.Cart.UserId == userId)
                .CountAsync(); ; // العدد القديم + العنصر الجديد

            return Json(new { success = true, message = "Added successfully", cartCount = newCount });
        }
    }
}
