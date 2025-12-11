using EduAll.Constant;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace EduAll.Controllers
{
    public class ContactController : Controller
    {

        private readonly EmailSender _emailSender;

        public ContactController(EmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new Contact_vm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(Contact_vm model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // 1. تجهيز عنوان الرسالة
                string subject = $"New Contact Message from: {model.Name}";

                // 2. تجهيز محتوى الرسالة (HTML)
                string body = $@"
                    <h3>You have received a new message from EduAll Website</h3>
                    <p><strong>Name:</strong> {model.Name}</p>
                    <p><strong>Email:</strong> {model.Email}</p>
                    <p><strong>Phone:</strong> {model.Phone}</p>
                    <hr/>
                    <p><strong>Message:</strong></p>
                    <p>{model.Message}</p>
                ";

                // 3. إرسال الإيميل
                await _emailSender.SendEmailAsync(subject, body);

                TempData["SuccessMessage"] = "Your message has been sent successfully! We will contact you soon.";
            }
            catch (Exception ex)
            {
                // لو حصل خطأ في الإرسال (نت فاصل مثلاً أو إعدادات غلط)
                TempData["ErrorMessage"] = "Failed to send message. Please try again later.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
