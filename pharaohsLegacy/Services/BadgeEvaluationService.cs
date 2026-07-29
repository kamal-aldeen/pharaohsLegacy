using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Services;

namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 بند 17 — Achievements & Badges
    //
    // الفكرة: مش بتضيف Action جديد لليوزر — بتراقب حاجات بتحصل في جداول تانية
    // (Bookings/Reviews/ReviewHelpfuls/QuizHistories/Favorites/ItemViews) وبتترجمها لشارة.
    //
    // بيتنادى من نهاية كل Trigger Point (بعد الـ SaveChangesAsync الأساسي)، Best-effort
    // زي أي إشعار تاني — فشل تقييم البادجات ميوقفش أي عملية أساسية.
    //
    // كل دالة Evaluate بترجع List<Badge> فيها الشارات الجديدة اللي اتفتحت فعليًا (عشان
    // الكنترولر يقدر يعرض/يبعت إشعار "مبروك خدت بادج!" لكل واحدة منهم).
    // ============================================================================
    public class BadgeEvaluationService
    {
        private readonly AppDbContext _db;
        private readonly LocalizationService _loc;

        public BadgeEvaluationService(AppDbContext db, LocalizationService loc)
        {
            _db = db;
            _loc = loc;
        }

        // ---------------------------------------------------------------
        // Helper عام: بياخد Key + العداد الحالي، ويفتح كل Tier اليوزر مستحقه ولسه معندوش
        // ---------------------------------------------------------------
        private async Task<List<Badge>> AwardTiersAsync(string userEmail, string badgeKey, int currentCount)
        {
            var newlyAwarded = new List<Badge>();

            var tiers = await _db.Badges
                .Where(b => b.Key == badgeKey)
                .OrderBy(b => b.Threshold)
                .ToListAsync();

            var alreadyEarnedBadgeIds = await _db.UserBadges
                .Where(ub => ub.UserEmail == userEmail && tiers.Select(t => t.Id).Contains(ub.BadgeId))
                .Select(ub => ub.BadgeId)
                .ToListAsync();

            foreach (var tier in tiers)
            {
                if (currentCount >= tier.Threshold && !alreadyEarnedBadgeIds.Contains(tier.Id))
                {
                    _db.UserBadges.Add(new UserBadge
                    {
                        UserEmail = userEmail,
                        BadgeId = tier.Id,
                        EarnedAt = DateTime.Now
                    });
                    newlyAwarded.Add(tier);
                }
            }

            if (newlyAwarded.Count > 0)
                await _db.SaveChangesAsync();

            return newlyAwarded;
        }

        // ---------------------------------------------------------------
        // Visit Achievements — بعد ما BookingStatusUpdater يحوّل حجز لـ Visited
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateVisitAsync(string userEmail)
        {
            int visitedCount = await _db.Bookings
                .CountAsync(b => b.UserEmail == userEmail && b.Status == "Visited");

            return await AwardTiersAsync(userEmail, "explorer", visitedCount);
        }

        // ---------------------------------------------------------------
        // Pharaoh Expert — بعد إضافة Favorite بتاع pharaoh
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluatePharaohExpertAsync(string userEmail)
        {
            int favCount = await _db.Favorites
                .CountAsync(f => f.UserEmail == userEmail && f.Type == "pharaoh");

            return await AwardTiersAsync(userEmail, "pharaoh_expert", favCount);
        }

        // ---------------------------------------------------------------
        // 🆕 Dynasty Expert — بعد فتح صفحة Details بتاعة أسرة (أول مرة بس لكل أسرة)
        // المصدر: ItemViews حيث Type == "dynasty" — عدد الأسرات المختلفة اللي اتفتحت
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateDynastyExpertAsync(string userEmail)
        {
            int viewedCount = await _db.ItemViews
                .Where(v => v.UserEmail == userEmail && v.Type == "dynasty")
                .Select(v => v.ItemId)
                .Distinct()
                .CountAsync();

            return await AwardTiersAsync(userEmail, "dynasty_expert", viewedCount);
        }

        // ---------------------------------------------------------------
        // Reviewer Badge — بعد إضافة Review جديد
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateReviewerAsync(string userEmail)
        {
            int reviewCount = await _db.Reviews.CountAsync(r => r.UserEmail == userEmail);
            return await AwardTiersAsync(userEmail, "reviewer", reviewCount);
        }

        // ---------------------------------------------------------------
        // Community Helper Badge — بعد ما حد ياخد Helpful vote على ريفيو المستخدم
        // (مش بعد ما هو نفسه يعمل Helpful — بعد ما هو ياخد Vote من حد تاني)
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateCommunityHelperAsync(string userEmail)
        {
            // كل الـ ReviewIds بتاعة ريفيوهات اليوزر ده
            var myReviewIds = await _db.Reviews
                .Where(r => r.UserEmail == userEmail)
                .Select(r => r.Id)
                .ToListAsync();

            int helpfulVotesReceived = await _db.ReviewHelpfuls
                .CountAsync(h => myReviewIds.Contains(h.ReviewId));

            return await AwardTiersAsync(userEmail, "community_helper", helpfulVotesReceived);
        }

        // ---------------------------------------------------------------
        // Quiz Master — بعد ما الكويز يخلص ويتسجل في QuizHistories
        // شرط "عدد المرات + Streak" — بناخد أعلى تقدم بين الاتنين (أيهما يوصل للـ Threshold الأول)
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateQuizMasterAsync(string userEmail)
        {
            int playCount = await _db.QuizHistories.CountAsync(h => h.UserEmail == userEmail);

            int bestStreak = await _db.QuizHistories
                .Where(h => h.UserEmail == userEmail)
                .Select(h => (int?)h.StreakDays)
                .MaxAsync() ?? 0;

            // بناخد الأكبر بين "تقدم عدد المرات" و"تقدم الاستريك" كـ "مؤشر تقدم" واحد للـ Tiers
            int progressMetric = Math.Max(playCount, bestStreak);

            return await AwardTiersAsync(userEmail, "quiz_master", progressMetric);
        }

        // ---------------------------------------------------------------
        // 🆕 Hidden Secret Achievements — الـ 6 شارات السرية مع بعض
        // بتتنادى Best-effort من أكتر من Trigger Point (بعد زيارة أسرة/حجز يتحول
        // Visited/كويز يخلص) عشان تتفحص أول ما أي شرط منهم يتحقق.
        // كل شارة فيها شرحها جنب الكود.
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateHiddenAchievementsAsync(string userEmail)
        {
            var newlyAwarded = new List<Badge>();

            // Perfect Score — حقق 100% (Score == Total) ولو مرة واحدة في أي كويز
            bool hasPerfectScore = await _db.QuizHistories
                .AnyAsync(h => h.UserEmail == userEmail && h.Score == h.Total);
            newlyAwarded.AddRange(await AwardTiersAsync(userEmail, "perfect_score", hasPerfectScore ? 1 : 0));

            // Streak Legend — استمرارية 30 يوم متواصلة في الكويز
            int bestStreak = await _db.QuizHistories
                .Where(h => h.UserEmail == userEmail)
                .Select(h => (int?)h.StreakDays)
                .MaxAsync() ?? 0;
            newlyAwarded.AddRange(await AwardTiersAsync(userEmail, "streak_legend", bestStreak));

            // Museum Completionist — زيارة (Visited) كل المتاحف بدون استثناء
            int visitedMuseums = await _db.Bookings
                .Where(b => b.UserEmail == userEmail && b.Status == "Visited" && b.PlaceType == "museum")
                .Select(b => b.PlaceId)
                .Distinct()
                .CountAsync();
            newlyAwarded.AddRange(await AwardTiersAsync(userEmail, "museum_completionist", visitedMuseums));

            // True Historian — فتح تفاصيل كل الـ 35 أسرة (نفس مصدر Dynasty Expert، بس شرط كامل)
            int viewedDynasties = await _db.ItemViews
                .Where(v => v.UserEmail == userEmail && v.Type == "dynasty")
                .Select(v => v.ItemId)
                .Distinct()
                .CountAsync();
            newlyAwarded.AddRange(await AwardTiersAsync(userEmail, "true_historian", viewedDynasties));

            // Loyal Explorer — عضو من سنة كاملة (365 يوم من Users.CreatedAt)
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                int daysSinceJoined = (DateTime.Now - user.CreatedAt).Days;
                newlyAwarded.AddRange(await AwardTiersAsync(userEmail, "loyal_explorer", daysSinceJoined));
            }

            // Night Owl — حجز أو كويز اتسجل الساعة 3 بالظبط (3:00–3:59 الفجر)
            bool nightOwlBooking = await _db.Bookings
                .AnyAsync(b => b.UserEmail == userEmail && b.CreatedAt.Hour == 3);
            bool nightOwlQuiz = await _db.QuizHistories
                .AnyAsync(h => h.UserEmail == userEmail && h.PlayedAt.Hour == 3);
            newlyAwarded.AddRange(await AwardTiersAsync(userEmail, "night_owl", (nightOwlBooking || nightOwlQuiz) ? 1 : 0));

            return newlyAwarded;
        }

        // ---------------------------------------------------------------
        // Legendary Explorer — Meta-badge: لازم Gold في كل الشارات التانية المفعّلة حاليًا
        // بتتنادى في الآخر بعد أي Evaluate تاني (عشان تتفحص أول ما آخر Gold ينفتح)
        // ---------------------------------------------------------------
        public async Task<List<Badge>> EvaluateLegendaryAsync(string userEmail)
        {
            // ⚠️ القايمة دي لازم تتحدث يدويًا لو ضفنا شارة عادية (Tiered) جديدة
            var requiredGoldKeys = new[] { "explorer", "pharaoh_expert", "dynasty_expert", "reviewer", "community_helper", "quiz_master" };

            var goldBadgeIds = await _db.Badges
                .Where(b => requiredGoldKeys.Contains(b.Key) && b.Tier == "Gold")
                .Select(b => b.Id)
                .ToListAsync();

            var earnedGoldCount = await _db.UserBadges
                .Where(ub => ub.UserEmail == userEmail && goldBadgeIds.Contains(ub.BadgeId))
                .CountAsync();

            bool hasAllGold = earnedGoldCount >= requiredGoldKeys.Length;
            if (!hasAllGold) return new List<Badge>();

            return await AwardTiersAsync(userEmail, "legendary_explorer", 1); // Threshold = 0، فأي رقم >= 0 كافي
        }

        // ---------------------------------------------------------------
        // 🔔 بتبعت إشعار "مبروك! خدت شارة X" لكل بادج جديدة، بنفس أسلوب NotificationHelper
        // المستخدم في باقي الكنترولرز (Best-effort، جوه try/catch من عند الكولر)
        // ---------------------------------------------------------------
        public void NotifyNewBadges(string userEmail, List<Badge> newBadges, string lang)
        {
            foreach (var badge in newBadges)
            {
                string name = lang == "ar" ? badge.NameAr : badge.NameEn;

                NotificationHelper.Create(
                    _db,
                    userEmail: userEmail,
                    title: _loc.GetFormatted("Badge_EarnedNotifTitle", lang, name),
                    message: _loc.GetFormatted("Badge_EarnedNotifMessage", lang, name, badge.Tier),
                    type: "Badge",
                    link: "/User/Dashboard?tab=badges");
            }
        }

        // ---------------------------------------------------------------
        // 🖥️ للعرض في الداشبورد — بيرجع كل شارة في الكتالوج مع حالتها لليوزر ده:
        // اتفتحت؟ إمتى؟ التقدم الحالي/المطلوب للـ Tier الجاي؟
        //
        // 🔄 تحديث (28 يوليو): الشارات المخفية (IsHidden = true) بقت بتتعامل زي أي شارة
        // تانية بالظبط — بترجع اسمها ووصفها وتقدمها الحقيقي عادي، وباهتة/Grayscale بس
        // (زي أي بادج مقفول تاني) من غير إخفاء "؟؟؟" ومن غير تصفير التقدم. قرار سابق
        // كان بيقنّع الاسم بـ "؟؟؟" ويصفّر التقدم — اتلغى لصالح تصميم أبسط وأوضح بصريًا.
        // IsHidden لسه موجودة في الـ DTO لو حبينا نستخدمها للتجميع/الفرز بس.
        // ---------------------------------------------------------------
        public async Task<List<BadgeDisplayItem>> GetDashboardBadgesAsync(string userEmail)
        {
            var allBadges = await _db.Badges.OrderBy(b => b.Key).ThenBy(b => b.Threshold).ToListAsync();
            var earned = await _db.UserBadges.Where(ub => ub.UserEmail == userEmail).ToListAsync();
            var earnedBadgeIds = earned.Select(e => e.BadgeId).ToHashSet();

            // مؤشر التقدم الحالي لكل Key (نفس منطق EvaluateXxxAsync، بس بدون منح — قراءة بس)
            var progressByKey = new Dictionary<string, int>
            {
                ["explorer"] = await _db.Bookings.CountAsync(b => b.UserEmail == userEmail && b.Status == "Visited"),
                ["pharaoh_expert"] = await _db.Favorites.CountAsync(f => f.UserEmail == userEmail && f.Type == "pharaoh"),
                ["dynasty_expert"] = await _db.ItemViews
                    .Where(v => v.UserEmail == userEmail && v.Type == "dynasty")
                    .Select(v => v.ItemId).Distinct().CountAsync(),
                ["reviewer"] = await _db.Reviews.CountAsync(r => r.UserEmail == userEmail),
                ["community_helper"] = await (
                    from h in _db.ReviewHelpfuls
                    join r in _db.Reviews on h.ReviewId equals r.Id
                    where r.UserEmail == userEmail
                    select h.Id).CountAsync(),
                ["quiz_master"] = Math.Max(
                    await _db.QuizHistories.CountAsync(h => h.UserEmail == userEmail),
                    await _db.QuizHistories.Where(h => h.UserEmail == userEmail)
                        .Select(h => (int?)h.StreakDays).MaxAsync() ?? 0),
                ["legendary_explorer"] = 0 // مالوش progress bar، بتتفتح مرة واحدة أو لأ
            };

            var result = new List<BadgeDisplayItem>();

            foreach (var group in allBadges.GroupBy(b => b.Key))
            {
                var tiers = group.OrderBy(b => b.Threshold).ToList();
                var earnedTiers = tiers.Where(t => earnedBadgeIds.Contains(t.Id)).ToList();
                var highestEarned = earnedTiers.OrderByDescending(t => t.Threshold).FirstOrDefault();
                var nextTier = tiers.FirstOrDefault(t => !earnedBadgeIds.Contains(t.Id));

                progressByKey.TryGetValue(group.Key, out int currentProgress);

                // 🆕 Tier ladder كامل — للـ Modal اللي بيعرض تفاصيل الشارة (Bronze/Silver/Gold
                // مع حالة كل واحدة وتاريخ اكتسابها لو اتكسبت)
                var tiersInfo = tiers.Select(t => new BadgeTierInfo
                {
                    Tier = t.Tier,
                    Threshold = t.Threshold,
                    NameEn = t.NameEn,
                    NameAr = t.NameAr,
                    DescriptionEn = t.DescriptionEn,
                    DescriptionAr = t.DescriptionAr,
                    Earned = earnedBadgeIds.Contains(t.Id),
                    EarnedAt = earned.Where(e => e.BadgeId == t.Id).Select(e => (DateTime?)e.EarnedAt).FirstOrDefault()
                }).ToList();

                result.Add(new BadgeDisplayItem
                {
                    Key = group.Key,
                    Category = tiers[0].Category,
                    IsHidden = tiers[0].IsHidden,
                    IconClass = tiers[0].IconClass,
                    NameEn = highestEarned?.NameEn ?? nextTier?.NameEn ?? tiers[0].NameEn,
                    NameAr = highestEarned?.NameAr ?? nextTier?.NameAr ?? tiers[0].NameAr,
                    DescriptionEn = highestEarned?.DescriptionEn ?? nextTier?.DescriptionEn ?? "",
                    DescriptionAr = highestEarned?.DescriptionAr ?? nextTier?.DescriptionAr ?? "",
                    EarnedTier = highestEarned?.Tier,
                    EarnedAt = earned.Where(e => e.BadgeId == highestEarned?.Id).Select(e => (DateTime?)e.EarnedAt).FirstOrDefault(),
                    NextTier = nextTier?.Tier,
                    NextThreshold = nextTier?.Threshold,
                    CurrentProgress = currentProgress,
                    Tiers = tiersInfo
                });
            }

            return result;
        }
    }

    // 🆕 DTO جاهز للعرض المباشر في الـ View (مش Model في الداتا بيز، مجرد شكل نتيجة)
    public class BadgeDisplayItem
    {
        public string Key { get; set; } = "";
        public string Category { get; set; } = "";
        public bool IsHidden { get; set; }
        public string IconClass { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string NameAr { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string DescriptionAr { get; set; } = "";
        public string? EarnedTier { get; set; }      // null = لسه معملتش أي Tier منها
        public DateTime? EarnedAt { get; set; }
        public string? NextTier { get; set; }         // null = خدها كاملة (كل الـ Tiers)
        public int? NextThreshold { get; set; }
        public int CurrentProgress { get; set; }
        public List<BadgeTierInfo> Tiers { get; set; } = new(); // 🆕 Tier ladder كامل — لـ Modal التفاصيل
    }

    // 🆕 معلومات Tier واحد داخل شارة (لعرضه في Modal التفاصيل — Bronze/Silver/Gold ladder)
    public class BadgeTierInfo
    {
        public string Tier { get; set; } = "";
        public int Threshold { get; set; }
        public string NameEn { get; set; } = "";
        public string NameAr { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string DescriptionAr { get; set; } = "";
        public bool Earned { get; set; }
        public DateTime? EarnedAt { get; set; }
    }
}
