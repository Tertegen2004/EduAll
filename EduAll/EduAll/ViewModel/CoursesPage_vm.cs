namespace EduAll.ViewModel
{
    public class CoursesPage_vm
    {
        // قائمة الكورسات المعروضة في الصفحة الحالية
        public List<HomeCourse_vm> Courses { get; set; } = new List<HomeCourse_vm>();

        // بيانات البحث والفلترة
        public string SearchQuery { get; set; }
        public string SortBy { get; set; } // Newest, Popular, etc.
        public int TotalCourses { get; set; } // العدد الكلي للكورسات (للعرض: Showing X of Y)

        // بيانات الباجينيشن
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
