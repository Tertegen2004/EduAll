namespace EduAll.ViewModel
{
    public class WatchCourse_vm
    {
        // بيانات الكورس العامة
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }

        // بيانات الدرس الحالي (اللي شغال في الفيديو)
        public int CurrentLessonId { get; set; }
        public string CurrentLessonTitle { get; set; }
        public string CurrentVideoUrl { get; set; }
        public string LessonDescription { get; set; } // لو فيه وصف للدرس

        public List<int> CompletedLessonIds { get; set; } = new List<int>();
        public int StudentProgress { get; set; } // النسبة المئوية الكلية

        // بيانات المدرب (للعرض تحت الفيديو)
        public string InstructorName { get; set; }
        public string InstructorImg { get; set; }
        public string InstructorJob { get; set; }

        // القائمة الجانبية (المنهج)
        public List<Section_vm> Sections { get; set; } = new List<Section_vm>();
    }
}
