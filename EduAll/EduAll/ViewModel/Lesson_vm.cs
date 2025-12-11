namespace EduAll.ViewModel
{
    public class Lesson_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public bool IsActive { get; set; } // عشان نلون الدرس الشغال حالياً
        public bool IsCompleted { get; set; } // لو عايز تميز الدروس اللي خلصت
    }
}
