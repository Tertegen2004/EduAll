namespace EduAll.Areas.Admin.ViewModels
{
    public class InstructorDetails_vm
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string ImgUrl { get; set; }
        public string JobTitle { get; set; }
        public bool IsActive { get; set; }

        // إحصائيات
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; } // مجموع الطلاب في كل كورساته

        // قائمة الكورسات التي أنشأها
        public List<CourseSummary_vm> CreatedCourses { get; set; } = new List<CourseSummary_vm>();
    }
}
