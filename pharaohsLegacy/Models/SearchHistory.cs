using System;

namespace pharaohsLegacy.Models
{
    // 🆕 Smart Search — بند 16
    // بيتسجل صف لكل بحث (لو فيه UserEmail في الـ Session)
    // ResultType بيتملى بس لو اليوزر ضغط على نتيجة معينة بعد البحث (عن طريق TrackSearchClick)
    public class SearchHistory
    {
        public int Id { get; set; }
        public string? UserEmail { get; set; }
        public string Query { get; set; } = "";
        public DateTime SearchedAt { get; set; } = DateTime.Now;
        public string? ResultType { get; set; } // "pharaoh" / "temple" / "museum" / "god" / "dynasty" / "artifact" / "event" / "product"
    }
}
