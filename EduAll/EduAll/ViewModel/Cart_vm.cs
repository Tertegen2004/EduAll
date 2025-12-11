using EduAll.Domain;

namespace EduAll.ViewModel
{
    public class Cart_vm
    {
        public List<CartItem_vm> Items { get; set; } = new List<CartItem_vm>();

        // حساب الإجمالي
        public decimal SubTotal => Items.Sum(i => i.Price);
        public decimal Discount { get; set; } = 0; // ممكن تفعله لاحقاً للكوبونات
        public decimal Tax { get; set; } = 0; // ممكن تحسبها نسبة مئوية
        public decimal Total => SubTotal + Tax - Discount;
    }
}
