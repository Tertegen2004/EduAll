namespace EduAll.ViewModel
{
    public class Notification_vm
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
        public string CourseName { get; set; }
        public DateTime Time { get; set; }
        public bool IsApproved { get; set; }
        public string Link { get; set; }
    }
}
