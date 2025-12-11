namespace EduAll.Areas.Admin.ViewModels
{
    public class Students_vm
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ImgUrl { get; set; }
        public int EnrolledCoursesCount { get; set; }
        public bool IsActive { get; set; } 
    }
}
