namespace EduAll.Areas.Admin.ViewModels
{
    public class Category_vm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Img { get; set; } 
        public int CoursesCount { get; set; } 
        public int StudentCategory { get; set; }
    }
}
