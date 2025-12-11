namespace EduAll.Areas.Admin.ViewModels
{
    public class StudentDetails_vm
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string ImgUrl { get; set; }
        public bool IsActive { get; set; }
        public List<StudentCourseInfo> EnrolledCourses { get; set; } = new List<StudentCourseInfo>();
    }

    public class StudentCourseInfo
    {
        public string CourseName { get; set; }
        public int Progress { get; set; } // نسبة التقدم (0 - 100)
        public string Status { get; set; } // In Progress / Completed
    }
}
