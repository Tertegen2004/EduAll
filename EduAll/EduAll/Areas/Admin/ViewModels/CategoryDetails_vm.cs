namespace EduAll.Areas.Admin.ViewModels
{
    public class CategoryDetails_vm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Img { get; set; }

        // إحصائيات
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; } // مجموع الطلاب في كل كورسات القسم

        public List<CategoryCourse_vm> Courses { get; set; } = new List<CategoryCourse_vm>();


    }
}

