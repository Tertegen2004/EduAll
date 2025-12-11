using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly UserManager<AppUser> usermanager;
        private readonly IUniteOfWork unite;

        public CheckOutController(UserManager<AppUser>usermanager,IUniteOfWork unite)
        {
            this.usermanager = usermanager;
            this.unite = unite;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await usermanager.GetUserAsync(User);

            // جلب الكورسات من الكارت لحساب السعر
            var cartItems = await unite.CartItem.GettAll()
                .Include(c => c.Course)
                .Where(c => c.Cart.UserId == user.Id)
            .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart"); // لو السلة فاضية ارجع

            var model = new Checkout_vm
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Phone = user.PhoneNumber,
                SubTotal = cartItems.Sum(c => c.Course.Price),
                Tax = 5 // ضريبة ثابتة كمثال
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(Checkout_vm model)
        {
            var user = await usermanager.GetUserAsync(User);

            // 1. جلب عناصر الكارت
            var cartItems = await unite.CartItem.GettAll()
                .Include(c => c.Course)
                .Where(c => c.Cart.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // 2. إنشاء الاشتراكات (Enrollments)
            var enrollments = cartItems.Select(item => new Enrollment
            {
                UserId = user.Id,
                CourseId = item.CourseId,
                EnrolledAt = DateTime.Now,
                Progress = 0
            }).ToList();

            await unite.Enrollment.CreateRange(enrollments);

            // 3. تفريغ الكارت
            await unite.CartItem.DeleteAll(cartItems);


            // 5. توجيه لصفحة نجاح أو كورساتي
            TempData["SuccessMessage"] = "Payment Successful! Courses added to your library.";
            return RedirectToAction("Index", "MyCourses");
        }
    }
}
