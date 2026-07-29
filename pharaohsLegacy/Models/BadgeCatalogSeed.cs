namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 بند 17 — Achievements & Badges — Catalog Seed
    // بيتنادى من AppDbContext.OnModelCreating() زي HasData بتاعة Pharaohs/Temples بالظبط.
    //
    // Dynasty Expert: مصدر بياناته ItemViews (فتح صفحة Details فعليًا، مش Favorite) —
    // شوف BadgeEvaluationService.EvaluateDynastyExpertAsync().
    //
    // الشارات السرية (IsHidden = true): شروطها متنوعة لكل واحدة — شوف
    // BadgeEvaluationService.EvaluateHiddenAchievementsAsync() للتفاصيل الكاملة.
    // ============================================================================
    public static class BadgeCatalogSeed
    {
        public static Badge[] GetAll()
        {
            int id = 1;
            var list = new List<Badge>();

            void Add(string key, string tier, int threshold, string category,
                string nameEn, string nameAr, string descEn, string descAr, string icon, bool hidden = false)
            {
                list.Add(new Badge
                {
                    Id = id++,
                    Key = key,
                    Tier = tier,
                    Threshold = threshold,
                    Category = category,
                    NameEn = nameEn,
                    NameAr = nameAr,
                    DescriptionEn = descEn,
                    DescriptionAr = descAr,
                    IconClass = icon,
                    IsHidden = hidden,
                    CreatedAt = new DateTime(2026, 7, 28) // ثابت عشان الـ Migration ميتغيرش كل مرة
                });
            }

            // ---------------- Visit Achievements ----------------
            // المصدر: Bookings حيث Status == "Visited"
            Add("explorer", "Bronze", 3, "Visit",
                "Explorer", "مستكشف",
                "Visited 3 places", "زار 3 أماكن", "fa-solid fa-compass");
            Add("explorer", "Silver", 7, "Visit",
                "Temple Master", "سيد المعابد",
                "Visited 7 places", "زار 7 أماكن", "fa-solid fa-compass");
            Add("explorer", "Gold", 15, "Visit",
                "Grand Explorer", "المستكشف الأعظم",
                "Visited 15 places", "زار 15 مكان", "fa-solid fa-compass");

            // ---------------- Knowledge Badges ----------------
            // المصدر: Favorites حيث Type == "pharaoh"
            Add("pharaoh_expert", "Bronze", 5, "Knowledge",
                "Pharaoh Expert", "خبير الفراعنة",
                "Favorited 5 pharaohs", "أضاف 5 فراعنة للمفضلة", "fa-solid fa-crown");
            Add("pharaoh_expert", "Silver", 15, "Knowledge",
                "Pharaoh Expert", "خبير الفراعنة",
                "Favorited 15 pharaohs", "أضاف 15 فرعون للمفضلة", "fa-solid fa-crown");
            Add("pharaoh_expert", "Gold", 30, "Knowledge",
                "Pharaoh Expert", "خبير الفراعنة",
                "Favorited 30 pharaohs", "أضاف 30 فرعون للمفضلة", "fa-solid fa-crown");

            // المصدر: ItemViews حيث Type == "dynasty" (فتح صفحة Details فعليًا)
            Add("dynasty_expert", "Bronze", 5, "Knowledge",
                "Dynasty Expert", "خبير الأسرات",
                "Explored 5 dynasties", "استكشف 5 أسرات", "fa-solid fa-scroll");
            Add("dynasty_expert", "Silver", 15, "Knowledge",
                "Dynasty Expert", "خبير الأسرات",
                "Explored 15 dynasties", "استكشف 15 أسرة", "fa-solid fa-scroll");
            Add("dynasty_expert", "Gold", 30, "Knowledge",
                "Dynasty Expert", "خبير الأسرات",
                "Explored 30 dynasties", "استكشف 30 أسرة", "fa-solid fa-scroll");

            // ---------------- Community Badges ----------------
            // المصدر: Reviews (عدد الريفيوهات اللي كتبها اليوزر)
            Add("reviewer", "Bronze", 3, "Community",
                "Reviewer", "كاتب مراجعات",
                "Wrote 3 reviews", "كتب 3 مراجعات", "fa-solid fa-pen");
            Add("reviewer", "Silver", 10, "Community",
                "Reviewer", "كاتب مراجعات",
                "Wrote 10 reviews", "كتب 10 مراجعات", "fa-solid fa-pen");
            Add("reviewer", "Gold", 25, "Community",
                "Reviewer", "كاتب مراجعات",
                "Wrote 25 reviews", "كتب 25 مراجعة", "fa-solid fa-pen");

            // المصدر: ReviewHelpfuls (مجموع الأصوات المفيدة على ريفيوهات اليوزر)
            Add("community_helper", "Bronze", 5, "Community",
                "Community Helper", "مساعد المجتمع",
                "Got 5 helpful votes", "حصل على 5 أصوات مفيدة", "fa-solid fa-handshake-angle");
            Add("community_helper", "Silver", 20, "Community",
                "Community Helper", "مساعد المجتمع",
                "Got 20 helpful votes", "حصل على 20 صوت مفيد", "fa-solid fa-handshake-angle");
            Add("community_helper", "Gold", 50, "Community",
                "Community Helper", "مساعد المجتمع",
                "Got 50 helpful votes", "حصل على 50 صوت مفيد", "fa-solid fa-handshake-angle");

            // المصدر: QuizHistories (عدد مرات اللعب) + StreakDays — أيهما يتحقق الأول (شوف BadgeEvaluationService)
            Add("quiz_master", "Bronze", 5, "Knowledge",
                "Quiz Master", "سيد الأسئلة",
                "Played 5 quizzes or reached a 3-day streak", "لعب 5 كويزات أو وصل لاستمرارية 3 أيام", "fa-solid fa-brain");
            Add("quiz_master", "Silver", 20, "Knowledge",
                "Quiz Master", "سيد الأسئلة",
                "Played 20 quizzes or reached a 7-day streak", "لعب 20 كويز أو وصل لاستمرارية 7 أيام", "fa-solid fa-brain");
            Add("quiz_master", "Gold", 50, "Knowledge",
                "Quiz Master", "سيد الأسئلة",
                "Played 50 quizzes or reached a 30-day streak", "لعب 50 كويز أو وصل لاستمرارية 30 يوم", "fa-solid fa-brain");

            // ---------------- Legendary ----------------
            // شرط: Gold في كل الشارات التانية المفعّلة حاليًا (شوف BadgeEvaluationService.EvaluateLegendaryAsync)
            // ✅ دلوقتي شامل: Explorer + Pharaoh Expert + Dynasty Expert + Reviewer + Community Helper + Quiz Master
            Add("legendary_explorer", "Gold", 0, "Legendary",
                "Legendary Explorer", "المستكشف الأسطوري",
                "Earned Gold in every other badge", "حصل على الذهبي في كل شارة تانية", "fa-solid fa-trophy");

            // ---------------- Hidden Secret Achievements ----------------
            // كلهم IsHidden = true: قبل الفتح بيظهروا باهتة/Grayscale زي أي بادج مقفول
            // تاني (اسمهم ووصفهم بيظهروا عادي، من غير تقنيع "؟؟؟" — قرار 28 يوليو).
            // شرط كل واحدة بالتفصيل في BadgeEvaluationService.EvaluateHiddenAchievementsAsync()

            Add("perfect_score", "Gold", 1, "Hidden",
                "Perfect Score", "الدرجة الكاملة",
                "Scored 100% on a quiz", "حقق 100% في كويز", "fa-solid fa-star", hidden: true);

            Add("streak_legend", "Gold", 30, "Hidden",
                "Streak Legend", "أسطورة الاستمرارية",
                "Reached a 30-day quiz streak", "وصل لاستمرارية 30 يوم في الكويز", "fa-solid fa-fire", hidden: true);

            // ⚠️ Threshold = عدد المتاحف الحالي (42) — لازم يتحدّث يدويًا لو اتضاف متحف جديد
            Add("museum_completionist", "Gold", 42, "Hidden",
                "Museum Completionist", "جامع المتاحف",
                "Visited every museum", "زار كل المتاحف", "fa-solid fa-building-columns", hidden: true);

            // ⚠️ Threshold = عدد الأسرات الحالي (35) — لازم يتحدّث يدويًا لو اتضافت أسرة جديدة
            Add("true_historian", "Gold", 35, "Hidden",
                "True Historian", "المؤرخ الحقيقي",
                "Explored every dynasty", "استكشف كل الأسرات", "fa-solid fa-landmark-dome", hidden: true);

            // Threshold بالأيام (365 يوم = سنة كاملة من Users.CreatedAt)
            Add("loyal_explorer", "Gold", 365, "Hidden",
                "Loyal Explorer", "المستكشف الوفي",
                "Member for a full year", "عضو من سنة كاملة", "fa-solid fa-heart", hidden: true);

            Add("night_owl", "Gold", 1, "Hidden",
                "Night Owl", "بومة الليل",
                "Booked a visit or played a quiz at 3 AM", "حجز زيارة أو لعب كويز الساعة 3 الفجر", "fa-solid fa-moon", hidden: true);

            return list.ToArray();
        }
    }
}
