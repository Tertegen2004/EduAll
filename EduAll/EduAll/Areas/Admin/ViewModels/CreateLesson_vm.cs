using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class CreateLesson_vm
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public IFormFile VideoUrl { get; set; }
        public int SectionId { get; set; }
        public int CourseId { get; set; }
    }
}
