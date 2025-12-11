namespace EduAll.ViewModel
{
    public class InstructorDetails_vm
    {
        // 1. بيانات المدرب
        public string Id { get; set; }
        public string FullName { get; set; }
        public string ImgUrl { get; set; }
        public string JobTitle { get; set; }
        public string Bio { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // السوشيال ميديا
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string InstagramUrl { get; set; }

        // 2. الإحصائيات
        public int CoursesCount { get; set; }
        public int StudentsCount { get; set; }
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }

        // 3. كورسات المدرب
        public List<HomeCourse_vm> Courses { get; set; } = new List<HomeCourse_vm>();
    }
}
