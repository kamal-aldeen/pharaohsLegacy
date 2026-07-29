using System.ComponentModel.DataAnnotations;

namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 بند 17 — Dynasty Expert / True Historian
    // تتبع عام (Generic) لأي "فتح تفاصيل" بيعمله اليوزر — مش مقصور على الأسرات بس،
    // ده عشان لو حبينا نوسّعه لأنواع تانية (متاحف/أماكن) مستقبلًا، من غير جدول جديد.
    // نفس فلسفة Favorite (Type + ItemId)، بس هنا بيتسجل تلقائيًا عند فتح صفحة Details
    // مش بفعل صريح من اليوزر.
    // ============================================================================
    public class ItemView
    {
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = "";

        [Required]
        public string Type { get; set; } = ""; // "dynasty" دلوقتي بس — قابل للتوسع لاحقًا

        public int ItemId { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.Now;
    }
}
