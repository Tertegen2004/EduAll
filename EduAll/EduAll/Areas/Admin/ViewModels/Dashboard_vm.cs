namespace EduAll.Areas.Admin.ViewModels
{
    public class Dashboard_vm
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public decimal TotalRevenue { get; set; } // إجمالي الأرباح

        // قوائم للعرض السريع
        public List<DashboardCourse_vm> LatestCourses { get; set; }
        public List<DashboardStudent_vm> NewStudents { get; set; }
    }
}
