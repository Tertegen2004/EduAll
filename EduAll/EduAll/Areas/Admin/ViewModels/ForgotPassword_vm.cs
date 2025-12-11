using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class ForgotPassword_vm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
