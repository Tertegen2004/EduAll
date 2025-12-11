namespace EduAll.ViewModel
{
    public class MyCourses_vm
    {
        public List<Course_vm> OngoingCourses { get; set; } = new List<Course_vm>();
        public List<Course_vm> CompletedCourses { get; set; } = new List<Course_vm>();
        public List<Course_vm> WishlistCourses { get; set; } = new List<Course_vm>(); // Saved/Favorite
    }
}
