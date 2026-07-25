using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class TripPlanStop
    {
        public int Id { get; set; }

        public int TripPlanId { get; set; }

        [ForeignKey(nameof(TripPlanId))]
        public TripPlan? TripPlan { get; set; }

        public int DayNumber { get; set; }

        // Temple / Museum / Pharaoh / God
        public string PlaceType { get; set; } = "";

        public int PlaceId { get; set; }

        // Snapshot لاسم المكان وقت توليد الخطة (مش NotMapped زي Booking —
        // هنا بنحتفظ بيه فعليًا عشان لو المكان اتعدل أو اتمسح بعدين، الخطة القديمة تفضل مفهومة)
        public string PlaceName { get; set; } = "";

        public string SuggestedTime { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedCost { get; set; }

        public string? Notes { get; set; }
    }
}
