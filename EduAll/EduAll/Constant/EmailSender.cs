using System.Net.Mail;
using System.Net;

namespace EduAll.Constant
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string subject, string messageBody)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"]);
            var fromEmail = emailSettings["FromEmail"];
            var password = emailSettings["Password"];

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "EduAll Contact Form"),
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = true,
            };

            // هنا بنحدد الإيميل اللي هيستلم الرسالة (اللي هو إيميلك برضه)
            mailMessage.To.Add(fromEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}

