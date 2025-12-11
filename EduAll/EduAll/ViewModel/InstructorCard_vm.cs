namespace EduAll.ViewModel
{
    public class InstructorCard_vm
    {
        public string Id { get; set; } // أو int حسب نوع الـ ID عندك
        public string FullName { get; set; }
        public string ImgUrl { get; set; }
        public string JobTitle { get; set; } // Web Developer, etc.
        public int CoursesCount { get; set; }
        public int StudentsCount { get; set; }
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }

        // روابط السوشيال (اختياري)
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string InstagramUrl { get; set; }
    }
}
