using EduAll.Domain;

namespace EduAll.ViewModel
{
    public class Section_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public bool IsLocked { get; set; } = false;
        public List<Lesson_vm> Lessons { get; set; } = new List<Lesson_vm>();


        public List<Quiz_vm> Quizzes { get; set; } = new List<Quiz_vm>();
    }

    public class Quiz_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; } 
    }
}
