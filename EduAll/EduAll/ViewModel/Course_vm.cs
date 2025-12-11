namespace EduAll.ViewModel
{
    public class Course_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public string InstructorName { get; set; }
        public string InstructorImg { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public int Progress { get; set; } // نسبة التقدم
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public bool IsWishlisted { get; set; }
        public int StudentsCount { get; set; }
        public int LessonsCount { get; set; }
        public int Duration { get; set; }
    }
}
