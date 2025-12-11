using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class CreateQuestion_vm
    {
        public int CourseId { get; set; } 

        [Required]
        public int QuizId { get; set; } 

        [Required(ErrorMessage = "Question text is required.")]
        public string Text { get; set; }

        [Range(1, 100, ErrorMessage = "Score must be at least 1.")]
        public int Score { get; set; } = 2;

        public List<string> Options { get; set; } = new List<string>();
        public int CorrectOption { get; set; }
    }
}
