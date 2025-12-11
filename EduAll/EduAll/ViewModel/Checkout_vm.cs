namespace EduAll.ViewModel
{
    public class Checkout_vm
    {
        // بيانات الفاتورة
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total => SubTotal + Tax - Discount;

        // بيانات المستخدم (للعرض فقط في الفورم)
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // بيانات الدفع (وهمية)
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}
