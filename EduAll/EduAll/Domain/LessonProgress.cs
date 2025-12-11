namespace EduAll.Domain
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        public int CourseId { get; set; } // عشان نسهل الحسابات
        public bool IsCompleted { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}
