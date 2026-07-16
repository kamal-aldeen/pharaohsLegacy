using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }  
        public string Status { get; set; }         

        public Booking Booking { get; set; }
    }
}