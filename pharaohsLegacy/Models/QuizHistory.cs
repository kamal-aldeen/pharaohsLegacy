namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 على عكس QuizAttempt (Session-only, بيتمسح)، الجدول ده بيتخزن في الداتابيز
    // دايمًا — عشان نقدر نحسب "هل لعب النهاردة؟" و"الـ Streak كام يوم متتالي؟"
    // حتى لو الـ Session انتهت أو اليوزر قفل المتصفح.
    // ============================================================================
    public class QuizHistory
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";

        // تاريخ ووقت انتهاء الكويز (بيتحسب عليه "نفس اليوم؟" و"يوم ورا يوم؟")
        public DateTime PlayedAt { get; set; } = DateTime.Now;

        public int Score { get; set; }
        public int Total { get; set; }
        public int ScorePercent { get; set; }

        // "A+", "A", "B+", "B", "Fail" — Fail يعني معدّاش حد الكوبون (70%)، حتى لو كان فوق حد الـ Streak
        public string Grade { get; set; } = "";

        // 🆕 أهلية الاستمرار في الـ Streak (حد 50%) — منفصلة تمامًا عن أهلية الكوبون (حد 70%)
        public bool StreakEligible { get; set; }

        // طول الـ Streak بعد الكويز ده (0 لو فشل حد الـ Streak أو قطع الاستمرارية)
        public int StreakDays { get; set; }

        // 0 لو مفيش كوبون النهاردة (Grade = Fail)
        public int DiscountPercent { get; set; }

        public string? CouponCode { get; set; }
    }
}
