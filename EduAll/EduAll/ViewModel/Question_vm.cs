namespace EduAll.ViewModel
{
    public class Question_vm
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public int Score { get; set; }
        public List<Option_vm> Options { get; set; } = new List<Option_vm>();
    }
}
