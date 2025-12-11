using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class EditCategory_vm
    {
        public int Id { get; set; }
        [Required] 
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? Img { get; set; } 
    }
}
