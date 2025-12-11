namespace EduAll.ViewModel
{
    public class CartItem_vm
    {
        public int CartItemId { get; set; } // ID الصف في جدول CartItem (للحذف)
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseImg { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
    }
}
