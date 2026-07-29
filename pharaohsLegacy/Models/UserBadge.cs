namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 بند 17 — Achievements & Badges
    // جدول الشارات اللي اليوزر خدها فعليًا. صف واحد = بادج واحدة اتفتحت.
    //
    // ⚠️ ملاحظة تصميم مهمة: لما اليوزر يترقّى من Bronze لـ Silver في نفس الشارة، بنضيف صف جديد
    // (مش بنعدّل القديم) — عشان يفضل عندنا تاريخ كامل لمتى فتح كل Tier (مفيد للـ Legendary
    // Explorer وللعرض "خدت Gold يوم كذا"). الـ Badge.Key بيبقى واحد، بس Badge.Id مختلف لكل Tier.
    // ============================================================================
    public class UserBadge
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";
        public int BadgeId { get; set; } // FK → Badge (صف الـ Tier المحدد اللي اتفتح)
        public DateTime EarnedAt { get; set; } = DateTime.Now;

        // 🆕 لو اليوزر شاف إشعار/بوب-أب الفوز بالشارة دي أو لسه — مش أساسي للمنطق،
        // بس مفيد لو حبينا نعمل "Seen/Unseen" في الواجهة مستقبلًا من غير ما نغيّر الجدول تاني
        public bool Seen { get; set; } = false;
    }
}
