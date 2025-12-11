namespace EduAll.ViewModel
{
    public class HomeCourse_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public string CategoryName { get; set; }
        public string Level { get; set; }
        public bool IsWishlisted { get; set; }

        public int StudentsCount { get; set; }
        public string CategoryFilter { get; set; }
        public string InstructorName { get; set; }
        public string InstructorImg { get; set; }
        public bool IsEnrolled { get; set; }
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }
        public int LessonsCount { get; set; }
        public int DurationHours { get; set; }
    }
}
