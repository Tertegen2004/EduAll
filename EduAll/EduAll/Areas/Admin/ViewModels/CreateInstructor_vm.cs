using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class CreateInstructor_vm
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]

        public string JobTitle { get; set; }
        [Required]

        public string Country { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
