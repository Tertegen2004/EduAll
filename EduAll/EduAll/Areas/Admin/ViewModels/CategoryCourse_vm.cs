namespace EduAll.Areas.Admin.ViewModels
{
    public class CategoryCourse_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public string InstructorName { get; set; }
        public decimal Price { get; set; }
        public int StudentsCount { get; set; }
        public bool IsPublished { get; set; }
    }
}
