using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class ShopOrder
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }

        // 🔄 اتشالت ProductId/Quantity المباشرة — دلوقتي الأوردر بيحتوي أكتر من منتج عن طريق Items
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }   // = مجموع (سعر الوحدة × الكمية) لكل الـ Items + ShippingFee

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }  // 🆕 محسوبة حسب المحافظة وقت الـ Checkout

        // 🆕 بيانات التوصيل
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Governorate { get; set; } = "";

        public string Status { get; set; }        // PendingPayment / Confirmed / Cancelled / Refunded
        public DateTime CreatedAt { get; set; }

        // زي Booking.CancelledAt
        public DateTime? CancelledAt { get; set; }

        // 🆕 تراك الشحن — مستقل عن Status عشان منبوظش منطق الدفع الموجود أصلاً.
        // بس ذو معنى لما Status == "Confirmed". القيم: Processing / Shipped / Delivered
        // الإلغاء مسموح بس طالما لسه Processing (يعني قبل ما الطلب يخرج للشحن).
        public string ShippingStatus { get; set; } = "Processing";

        // 🆕 لحظة نجاح الدفع فعليًا (مش CreatedAt، لأن CreatedAt بيتسجل وقت أول RequestOtp
        // ولو اليوزر أخد وقته وهو بيدخل بيانات الكارت، الـ 48 ساعة لازم تتحسب من هنا مش من هناك).
        // أساس حساب التحول التلقائي Processing → Shipped في ShopOrderShippingBackgroundService.
        public DateTime? ConfirmedAt { get; set; }

        // 🆕 لحظة الخروج للشحن فعليًا (سواء اتحطت أوتوماتيك بعد 48 ساعة أو يدويًا من الأدمن) —
        // أساس حساب التحول التلقائي Shipped → Delivered (حسب عدد أيام المحافظة).
        public DateTime? ShippedAt { get; set; }

        // 🆕 لحظة الوصول فعليًا — للعرض بس في MyOrders ("وصلك بتاريخ كذا").
        public DateTime? DeliveredAt { get; set; }

        // 🆕 المنتجات المرتبطة بالأوردر ده
        public List<ShopOrderItem> Items { get; set; } = new();
    }
}
