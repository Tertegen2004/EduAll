using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class ProfileSettings_vm
    {
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string JobTitle { get; set; } // Role or Job Title

        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }
        public string? ImgUrl { get; set; }

        public string Bio { get; set; }
    }
}
