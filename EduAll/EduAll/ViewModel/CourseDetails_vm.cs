using EduAll.Domain;

namespace EduAll.ViewModel
{
    public class CourseDetails_vm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public decimal Price { get; set; }
        public string Level { get; set; }
        public string Language { get; set; } = "English"; // ممكن تضيفها في الداتا بيز
        public string Duration { get; set; }

        // 2. حالة الطالب مع الكورس
        public bool IsEnrolled { get; set; } // عشان نخفي زرار الشراء
        public bool IsWishlisted { get; set; }

        // 3. الإحصائيات
        public int LessonsCount { get; set; }
        public int StudentsCount { get; set; }
        public int QuizzesCount { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }

        // 4. بيانات المدرب
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public string InstructorImg { get; set; }
        public string InstructorJob { get; set; }
        public string InstructorBio { get; set; }
        public int InstructorCoursesCount { get; set; }
        public int InstructorStudentsCount { get; set; }
        public double InstructorRating { get; set; }

        // 5. المنهج (Curriculum)
        public List<Section_vm> Sections { get; set; } = new List<Section_vm>();

        // 6. التقييمات (Reviews)
        public List<Review_vm> Reviews { get; set; } = new List<Review_vm>();

        // 1. لعرض الريفيوهات الموجودة

        // 2. لاستقبال الريفيو الجديد
        public AddReview_vm NewReview { get; set; }

        // 3. إحصائيات سريعة
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
