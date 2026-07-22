namespace pharaohsLegacy.Models
{
    // ============================================================================
    // كل حاجة هنا Transient — بتتخزن جوه الـ Session كـ JSON بس (JsonSerializer)،
    // مفيش جدول DB لها خالص. القاعدة الذهبية: IsCorrect ينفعش يوصل للـ Client —
    // شوف QuizController.ToPublicDto اللي بيبني DTO جديد من غير الحقل ده أصلاً.
    // ============================================================================

    public enum QuizDifficulty { Easy, Medium, Hard }

    public class QuizChoice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
        public string Text { get; set; } = "";
        public bool IsCorrect { get; set; } // 🔐 بيتخزن في الـ Session بس، مبيتبعتش للمتصفح أبدًا
    }

    public class QuizQuestion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Category { get; set; } = ""; // Pharaoh / God / Temple / Museum / Dynasty / HistoricalEvent
        public QuizDifficulty Difficulty { get; set; }
        public string Text { get; set; } = "";
        public List<QuizChoice> Choices { get; set; } = new();

        // 🔐 بيتحدد لحظة ما السؤال يتبعت فعليًا للمتصفح (مش وقت التوليد) — أساس الـ Timeout سيرفر-سايد
        public DateTime? ShownAt { get; set; }

        public bool Answered { get; set; }
        public bool AnsweredCorrectly { get; set; }
    }

    public class QuizAttempt
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string UserEmail { get; set; } = "";

        // ⚠️ مبقاش بيتحدد ولا بيتستخدم بعد التحويل لصعوبة تلقائية مختلطة لكل كويز —
        // كل سؤال بقى ليه Difficulty خاص بيه جوه QuizQuestion نفسه. سايبينه هنا
        // (بياخد القيمة الافتراضية Easy) عشان مايكسرش أي كود تاني بيقرا منه.
        public QuizDifficulty Difficulty { get; set; }

        public List<QuizQuestion> Questions { get; set; } = new();
        public int CurrentIndex { get; set; } = 0;
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public bool Finished { get; set; } = false;
        public int TimeLimitSeconds { get; set; } = 20; // لكل سؤال

        // 🔐 Anti-Cheat: وقت كل إجابة بالثواني، بالترتيب — بيتستخدم في
        // QuizController.IsSuspiciousTimingPattern عشان نكشف نمط بوت/سكريبت
        // (سرعة غير طبيعية أو ثبات غير طبيعي في التوقيت) قبل ما نديله الكوبون.
        public List<double> AnswerTimesSeconds { get; set; } = new();
    }
}
