using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class SignIn_vm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]

        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
