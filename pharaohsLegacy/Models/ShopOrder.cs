using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class ShopOrder
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }        // PendingPayment / Confirmed / Cancelled / Refunded
        public DateTime CreatedAt { get; set; }

        // 🆕 زي Booking.CancelledAt — لو حبينا نضيف Cancel/Refund للمتجر بعدين بنفس منطق
        // BookingRefundBackgroundService، الحقل ده جاهز من دلوقتي
        public DateTime? CancelledAt { get; set; }

        [NotMapped]
        public string ProductName { get; set; } = "";

        [NotMapped]
        public string ProductImage { get; set; } = "";
    }
}
