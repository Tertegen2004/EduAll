using System.ComponentModel.DataAnnotations;

namespace EduAll.Areas.Admin.ViewModels
{
    public class EditQuestion_vmcs
    {
        public int CourseId { get; set; }
        public int QuestionId { get; set; }

        [Required] public string Text { get; set; }
        public int Score { get; set; }

        // الاختيارات (نفس فكرة الإضافة)
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; } // رقم الإجابة الصح (0-3)
    }
}
