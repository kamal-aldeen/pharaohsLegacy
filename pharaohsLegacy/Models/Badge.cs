namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 بند 17 — Achievements & Badges
    // جدول الشارات نفسها (الكتالوج) — مش مرتبط بيوزر معين، ده الـ "تعريف" بس.
    // كل صف هنا = شارة واحدة بتاع Tier واحد (مثال: "Explorer" لها 3 صفوف: Bronze/Silver/Gold)
    // ============================================================================
    public class Badge
    {
        public int Id { get; set; }

        // كود ثابت فريد للشارة نفسها (بغض النظر عن الـ Tier) — بيتستخدم في الكود عشان نعرف
        // نلاقي شارة معينة بسرعة من غير ما نعتمد على الاسم القابل للترجمة
        // مثال: "explorer", "pharaoh_expert", "quiz_master", "legendary_explorer"
        public string Key { get; set; } = "";

        // Bronze / Silver / Gold — لو الشارة مالهاش Tiers فعليًا (زي Legendary)، بتتسجل كـ "Gold" بس
        // (صف واحد بس في الكتالوج ليها)
        public string Tier { get; set; } = "Bronze";

        public string NameEn { get; set; } = "";
        public string NameAr { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string DescriptionAr { get; set; } = "";

        // اسم Class أيقونة (زي Font Awesome) أو مسار صورة — حسب إيه اللي هيتستخدم في الـ Views
        public string IconClass { get; set; } = "";

        // Visit / Knowledge / Community / Legendary / Secret
        public string Category { get; set; } = "";

        // 🆕 Hidden Secret Achievements — لو true، تتعرض في الواجهة Grayscale/باهتة قبل ما تتفك
        // (مش تختفي خالص)، وشرط الفتح نفسه مش بيتعرض لليوزر قبل ما ياخدها
        public bool IsHidden { get; set; } = false;

        // 🆕 الحد المطلوب عشان الشارة/الـ Tier ده يتفتح — بيتفسر حسب الشارة (Key) في BadgeEvaluationService
        // مثال: Explorer Bronze = 3, Silver = 7, Gold = 15 (عدد الأماكن Visited)
        public int Threshold { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
