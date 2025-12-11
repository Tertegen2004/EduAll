using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class CreateQuiz_vm
    {
        [Required]
        public string Title { get; set; }
        [Required]

        public int SectionId { get; set; }
        [Required]

        public int CourseId { get; set; }
        [Required()]

        public int PassingScore { get; set; }
    }
}
