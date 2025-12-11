using EduAll.Domain;

namespace EduAll.ViewModel
{
    public class QuizAttemp_vm
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int CourseId { get; set; } 

        public List<Question_vm> Questions { get; set; } = new List<Question_vm>();
    }
}
