namespace EduAll.Areas.Admin.ViewModels
{
    public class Course_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public decimal Price { get; set; }
        public string InstructorName { get; set; }
        public string CategoryName { get; set; }
        public int LessonsCount { get; set; }
        public bool IsPublished { get; set; } = true;
    }
}
