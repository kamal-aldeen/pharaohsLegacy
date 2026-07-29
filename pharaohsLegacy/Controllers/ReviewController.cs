using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;

namespace pharaohsLegacy.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly LocalizationService _loc;
        private readonly BadgeEvaluationService _badges; // 🆕 بند 17 — Achievements & Badges
        private const string AdminEmail = "kamalabdlbast89@gmail.com";

        public ReviewController(AppDbContext context, LocalizationService loc, BadgeEvaluationService badges)
        {
            _context = context;
            _loc = loc;
            _badges = badges;
        }

        // 🆕 بدل ما نكرر HttpContext.Session.GetString("Lang") ?? "en" في كل Action (زي باقي الكنترولرز)
        private string Lang() => HttpContext.Session.GetString("Lang") ?? "en";

        // ─────────────────────────────────────────────
        //  ADD
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Add(string type, int itemId, int rating, string comment)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "You must be logged in to leave a review." });

            if (role == "Admin")
                return Json(new { success = false, message = "Admins cannot write reviews." });

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            var name = user?.Name ?? "Unknown";

            var existing = _context.Reviews
                .FirstOrDefault(r => r.UserEmail == email &&
                                     r.Type == type.ToLower() &&
                                     r.ItemId == itemId);
            if (existing != null)
                return Json(new { success = false, message = "You have already reviewed this." });

            if (string.IsNullOrWhiteSpace(comment) || rating < 1 || rating > 5)
                return Json(new { success = false, message = "Invalid input." });

            var review = new Review
            {
                UserEmail = email,
                UserName = name,
                Type = type.ToLower(),
                ItemId = itemId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // 🔔 بند 17 — Achievements & Badges: بعد إضافة ريفيو جديد، نفحص Reviewer Badge + Legendary
            try
            {
                var newBadges = await _badges.EvaluateReviewerAsync(email);
                newBadges.AddRange(await _badges.EvaluateLegendaryAsync(email));
                _badges.NotifyNewBadges(email, newBadges, Lang());
                _context.SaveChanges();
            }
            catch
            {
                // Best effort — الريفيو خلص بنجاح، فشل تقييم البادجات ميوقفش حاجة
            }

            return Json(new { success = true });
        }

        // ─────────────────────────────────────────────
        //  EDIT  ⭐
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult Edit(int id, int rating, string comment)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Not logged in." });

            var review = _context.Reviews.Find(id);
            if (review == null || review.UserEmail != email)
                return Json(new { success = false, message = "Not authorized." });

            if (string.IsNullOrWhiteSpace(comment) || rating < 1 || rating > 5)
                return Json(new { success = false, message = "Invalid input." });

            review.Rating = rating;
            review.Comment = comment.Trim();
            review.IsEdited = true;
            _context.SaveChanges();

            return Json(new { success = true, rating, comment = review.Comment });
        }

        // ─────────────────────────────────────────────
        //  DELETE (user — admin email check)
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email != AdminEmail)
                return Json(new { success = false });

            var review = _context.Reviews.Find(id);
            if (review == null)
                return Json(new { success = false });

            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // ─────────────────────────────────────────────
        //  DELETE ADMIN (from Admin Dashboard)
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult DeleteAdmin(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email != AdminEmail)
                return RedirectToAction("Index", "Admin");

            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                // حذف الـ Helpfuls والـ Reports المرتبطة
                var helpfuls = _context.ReviewHelpfuls.Where(h => h.ReviewId == id).ToList();
                var reports = _context.ReviewReports.Where(r => r.ReviewId == id).ToList();
                _context.ReviewHelpfuls.RemoveRange(helpfuls);
                _context.ReviewReports.RemoveRange(reports);

                _context.Reviews.Remove(review);
                _context.SaveChanges();
                TempData["Success"] = "Review deleted successfully.";
            }

            return RedirectToAction("Index", "Admin", new { tab = "reports" });
        }

        // ─────────────────────────────────────────────
        //  HELPFUL TOGGLE  👍
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleHelpful(int reviewId)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email) || email == "guest")
                return Json(new { success = false, message = "Login required." });

            var existing = _context.ReviewHelpfuls
                .FirstOrDefault(h => h.ReviewId == reviewId && h.UserEmail == email);

            bool isHelpful;
            if (existing != null)
            {
                _context.ReviewHelpfuls.Remove(existing);
                isHelpful = false;
            }
            else
            {
                _context.ReviewHelpfuls.Add(new ReviewHelpful
                {
                    ReviewId = reviewId,
                    UserEmail = email
                });
                isHelpful = true;
            }

            _context.SaveChanges();
            var count = _context.ReviewHelpfuls.Count(h => h.ReviewId == reviewId);

            // 🔔 بند 17 — Achievements & Badges: بعد ما حد ياخد Helpful vote، نفحص بادج
            // Community Helper بتاع صاحب الريفيو (مش بتاع اللي داس الزرار) + Legendary بتاعه
            if (isHelpful)
            {
                try
                {
                    var review = _context.Reviews.Find(reviewId);
                    if (review != null)
                    {
                        // ⚠️ نفس قرار إشعار الأدمن في BookingController.Confirm(): مفيش لغة مخزنة
                        // لصاحب الريفيو في السياق ده، فبيتسجل الإشعار بلغة اللي داس "مفيد" (Lang())
                        // مش لغة صاحب الريفيو نفسه. قرار مقبول مؤقتًا، مش باگ.
                        var newBadges = await _badges.EvaluateCommunityHelperAsync(review.UserEmail);
                        newBadges.AddRange(await _badges.EvaluateLegendaryAsync(review.UserEmail));
                        _badges.NotifyNewBadges(review.UserEmail, newBadges, Lang());
                        _context.SaveChanges();
                    }
                }
                catch
                {
                    // Best effort — الـ Helpful vote خلص بنجاح، فشل تقييم البادجات ميوقفش حاجة
                }
            }

            return Json(new { success = true, isHelpful, count });
        }

        // ─────────────────────────────────────────────
        //  GET HELPFUL DATA (AJAX on page load)
        // ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetHelpfulData(string type, int itemId)
        {
            var email = HttpContext.Session.GetString("UserEmail") ?? "";

            var reviewIds = _context.Reviews
                .Where(r => r.Type == type.ToLower() && r.ItemId == itemId)
                .Select(r => r.Id)
                .ToList();

            var counts = _context.ReviewHelpfuls
                .Where(h => reviewIds.Contains(h.ReviewId))
                .GroupBy(h => h.ReviewId)
                .ToDictionary(g => g.Key, g => g.Count());

            var userVotes = string.IsNullOrEmpty(email)
                ? new List<int>()
                : _context.ReviewHelpfuls
                    .Where(h => h.UserEmail == email && reviewIds.Contains(h.ReviewId))
                    .Select(h => h.ReviewId)
                    .ToList();

            return Json(new { counts, userVotes });
        }

        // ─────────────────────────────────────────────
        //  REPORT  🚩
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult Report(int reviewId, string reason)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email) || email == "guest")
                return Json(new { success = false, message = "Login required." });

            var existing = _context.ReviewReports
                .FirstOrDefault(r => r.ReviewId == reviewId && r.ReporterEmail == email);
            if (existing != null)
                return Json(new { success = false, message = "You already reported this review." });

            if (string.IsNullOrWhiteSpace(reason))
                return Json(new { success = false, message = "Please provide a reason." });

            _context.ReviewReports.Add(new ReviewReport
            {
                ReviewId = reviewId,
                ReporterEmail = email,
                Reason = reason.Trim(),
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ─────────────────────────────────────────────
        //  RESOLVE REPORT (Admin)
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult ResolveReport(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email != AdminEmail)
                return Json(new { success = false });

            var report = _context.ReviewReports.Find(id);
            if (report == null)
                return Json(new { success = false });

            report.IsResolved = true;

            // 🔔 بند 15 — Notification System: نبلّغ اللي عمل الـ Report إن الأدمن راجعه
            NotificationHelper.Create(
                _context,
                userEmail: report.ReporterEmail,
                title: _loc.Get("Review_ReportResolvedNotifTitle", Lang()),
                message: _loc.Get("Review_ReportResolvedNotifMessage", Lang()),
                type: "Review",
                link: null);

            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}