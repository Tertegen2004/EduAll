namespace EduAll.ViewModel
{
    public class QuizSubmission_vm
    {
        public int QuizId { get; set; }
        public int CourseId { get; set; }

        public Dictionary<int, int> Answers { get; set; }
    }
}
