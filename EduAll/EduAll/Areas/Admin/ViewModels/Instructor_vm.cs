namespace EduAll.Areas.Admin.ViewModels
{
    public class Instructor_vm
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ImgUrl { get; set; }
        public string JobTitle { get; set; } 
        public int CreatedCoursesCount { get; set; } 
        public bool IsActive { get; set; }
    }
}
