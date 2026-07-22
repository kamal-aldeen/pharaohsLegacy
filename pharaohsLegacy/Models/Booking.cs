using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string PlaceType { get; set; }   
        public int PlaceId { get; set; }
        public DateTime VisitDate { get; set; }
        public int NumberOfTickets { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }       
        public DateTime CreatedAt { get; set; }

        // 🆕 بيتسجل لما الحجز يتحول لـ Cancelled — الأساس اللي بيقيس عليه
        // الـ Background Job الـ 24 ساعة قبل ما يحوله أوتوماتيك لـ Refunded
        public DateTime? CancelledAt { get; set; }

        // 🆕 بيتولد لما الحجز يبقى Confirmed لأول مرة — ده الـ "مفتاح" اللي بيفتح صفحة
        // التذكرة (Booking/Ticket). من غيره محدش يقدر يفتح تذكرة حجز مش بتاعه، حتى لو عرف الـ Id.
        public Guid? TicketToken { get; set; }

        [NotMapped]
        public string PlaceName { get; set; } = "";
    }
}