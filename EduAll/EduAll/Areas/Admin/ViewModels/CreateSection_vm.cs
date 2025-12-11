using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class CreateSection_vm
    {
        [Required]
        public string Title { get; set; }
        public int CourseId { get; set; }
    }
}
