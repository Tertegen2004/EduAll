namespace EduAll.Areas.Admin.ViewModels
{
    public class Suggestion_vm
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string StudentImage { get; set; }
        public string CourseName { get; set; }
        public string Content { get; set; } // نص الاقتراح
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public string AdminResponse { get; set; } // رد الأدمن السابق (لو موجود)
    }
}
