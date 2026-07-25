using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class TripPlan
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";

        public int Days { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        // Comma-separated interests كده زي ما اتفقنا (مثال: "Temples,Museums,Gods")
        public string Interests { get; set; } = "";

        // Family / Student / Luxury
        public string Mode { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property — الوقفات اليومية للخطة دي
        public List<TripPlanStop> Stops { get; set; } = new();
    }
}
