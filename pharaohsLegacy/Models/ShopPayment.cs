using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    // 🆕 مش استخدمنا Payment.cs الموجود لأنه مربوط بـ BookingId كـ FK حقيقي (int إجباري +
    // navigation property Booking) — مفيش مكان نحط فيه ShopOrderId من غير ما نكسره أو نضيفله
    // حقل جديد نفسه. أبسط وأنضف حل: جدول Payment مستقل لعمليات المتجر بنفس الشكل بالظبط.
    public class ShopPayment
    {
        public int Id { get; set; }
        public int ShopOrderId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }

        public ShopOrder ShopOrder { get; set; }
    }
}
