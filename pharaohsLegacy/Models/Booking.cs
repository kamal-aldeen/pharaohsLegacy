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
        
        [NotMapped]
        public string PlaceName { get; set; } = "";
    }
}