using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            //_logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");


            ViewBag.Pharaohs = _context.Pharaohs.Take(3).ToList();
            ViewBag.Temples = _context.Temples.Take(3).ToList();
            ViewBag.Museums = _context.Museums.Take(3).ToList();
            ViewBag.Gods = _context.Gods.Take(3).ToList();
            ViewBag.TodaysFact = GetTodaysFact();


            return View();
        }

        // بيرجع حقيقة ثابتة طول اليوم (نفس الحقيقة لكل اليوزرز)، وتتغير أوتوماتيك كل يوم
        private DailyFact? GetTodaysFact()
        {
            var facts = _context.DailyFacts.ToList();
            if (!facts.Any())
                return null;

            int seed = DateTime.Now.Year * 1000 + DateTime.Now.DayOfYear;
            var rng = new Random(seed);
            int index = rng.Next(facts.Count);
            return facts[index];
        }

        // بيرجع حقيقة عشوائية تانية غير "حقيقة اليوم" — بيستخدمها زرار "حقيقة تانية" في الـ Home Page
        [HttpGet]
        public IActionResult GetRandomFact(int excludeId)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return Json(new { success = false });

            var lang = HttpContext.Session.GetString("Lang") ?? "en";

            var facts = _context.DailyFacts
                .Where(f => f.Id != excludeId)
                .ToList();

            if (!facts.Any())
                return Json(new { success = false });

            var random = new Random();
            var fact = facts[random.Next(facts.Count)];
            var text = (lang == "ar" && !string.IsNullOrEmpty(fact.FactTextAr)) ? fact.FactTextAr : fact.FactText;
            var category = (lang == "ar" && !string.IsNullOrEmpty(fact.CategoryAr)) ? fact.CategoryAr : fact.Category;

            return Json(new
            {
                success = true,
                id = fact.Id,
                text = text,
                category = category
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // =========================================================
        // 🔍 Smart Search — بند 16
        // =========================================================

        public IActionResult Search(string q)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            if (string.IsNullOrEmpty(q))
                return RedirectToAction("Index");

            var pharaohs = _context.Pharaohs
                .Where(p => p.Name.Contains(q) || p.Dynasty.Contains(q)
                         || (p.NameAr != null && p.NameAr.Contains(q))
                         || (p.DynastyAr != null && p.DynastyAr.Contains(q)))
                .ToList();
            if (!pharaohs.Any())
                pharaohs = FuzzyFilter(_context.Pharaohs.ToList(), q,
                    p => new[] { p.Name, p.NameAr, p.Dynasty, p.DynastyAr });

            var temples = _context.Temples
                .Where(t => t.Name.Contains(q) || t.Location.Contains(q)
                         || (t.NameAr != null && t.NameAr.Contains(q))
                         || (t.LocationAr != null && t.LocationAr.Contains(q)))
                .ToList();
            if (!temples.Any())
                temples = FuzzyFilter(_context.Temples.ToList(), q,
                    t => new[] { t.Name, t.NameAr, t.Location, t.LocationAr });

            var gods = _context.Gods
                .Where(g => g.Name.Contains(q) || g.Role.Contains(q)
                         || (g.NameAr != null && g.NameAr.Contains(q))
                         || (g.RoleAr != null && g.RoleAr.Contains(q)))
                .ToList();
            if (!gods.Any())
                gods = FuzzyFilter(_context.Gods.ToList(), q,
                    g => new[] { g.Name, g.NameAr, g.Role, g.RoleAr });

            var museums = _context.Museums
                .Where(m => m.Name.Contains(q) || m.Location.Contains(q)
                         || (m.NameAr != null && m.NameAr.Contains(q))
                         || (m.LocationAr != null && m.LocationAr.Contains(q)))
                .ToList();
            if (!museums.Any())
                museums = FuzzyFilter(_context.Museums.ToList(), q,
                    m => new[] { m.Name, m.NameAr, m.Location, m.LocationAr });

            var artifacts = _context.Artifacts
                .Where(a => a.Name.Contains(q) || a.Category.Contains(q) || a.Origin.Contains(q)
                         || (a.NameAr != null && a.NameAr.Contains(q))
                         || (a.CategoryAr != null && a.CategoryAr.Contains(q))
                         || (a.OriginAr != null && a.OriginAr.Contains(q)))
                .ToList();
            if (!artifacts.Any())
                artifacts = FuzzyFilter(_context.Artifacts.ToList(), q,
                    a => new[] { a.Name, a.NameAr, a.Category, a.CategoryAr, a.Origin, a.OriginAr });

            var dynasties = _context.Dynasties
                .Where(d => d.Name.Contains(q) || d.Era.Contains(q) || d.CapitalCity.Contains(q)
                         || (d.NameAr != null && d.NameAr.Contains(q))
                         || (d.EraAr != null && d.EraAr.Contains(q))
                         || (d.CapitalCityAr != null && d.CapitalCityAr.Contains(q)))
                .ToList();
            if (!dynasties.Any())
                dynasties = FuzzyFilter(_context.Dynasties.ToList(), q,
                    d => new[] { d.Name, d.NameAr, d.Era, d.EraAr, d.CapitalCity, d.CapitalCityAr });

            // 🆕 HistoricalEvents مضافة للبحث الموحّد (كانت ناقصة قبل كده)
            var historicalEvents = _context.HistoricalEvents
                .Where(e => e.Title.Contains(q) || e.Category.Contains(q)
                         || (e.TitleAr != null && e.TitleAr.Contains(q))
                         || (e.CategoryAr != null && e.CategoryAr.Contains(q)))
                .ToList();
            if (!historicalEvents.Any())
                historicalEvents = FuzzyFilter(_context.HistoricalEvents.ToList(), q,
                    e => new[] { e.Title, e.TitleAr, e.Category, e.CategoryAr });

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(q)
                         || (p.NameAr != null && p.NameAr.Contains(q))
                         || (p.Category != null && p.Category.Name.Contains(q))
                         || (p.Category != null && p.Category.NameAr != null && p.Category.NameAr.Contains(q)))
                .ToList();
            if (!products.Any())
                products = FuzzyFilter(_context.Products.Include(p => p.Category).ToList(), q,
                    p => new[] { p.Name, p.NameAr, p.Category?.Name, p.Category?.NameAr });

            LogSearch(q);

            ViewBag.Query = q;
            ViewBag.Pharaohs = pharaohs;
            ViewBag.Temples = temples;
            ViewBag.Gods = gods;
            ViewBag.Museums = museums;
            ViewBag.Artifacts = artifacts;
            ViewBag.Dynasties = dynasties;
            ViewBag.Products = products;
            ViewBag.HistoricalEvents = historicalEvents;
            ViewBag.RecentSearches = GetRecentSearches(q);
            ViewBag.TrendingSearches = GetTrendingSearches(q);

            return View();
        }

        // 🆕 Autocomplete — بيتنادى بالـ AJAX من smart-search.js أثناء الكتابة
        [HttpGet]
        public IActionResult SearchSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var suggestions = new List<object>();

            suggestions.AddRange(_context.Pharaohs
                .Where(p => p.Name.Contains(term) || (p.NameAr != null && p.NameAr.Contains(term)))
                .Take(3)
                .Select(p => new { type = "pharaoh", id = p.Id, name = p.Name, image = p.ImageUrl }));

            suggestions.AddRange(_context.Temples
                .Where(t => t.Name.Contains(term) || (t.NameAr != null && t.NameAr.Contains(term)))
                .Take(3)
                .Select(t => new { type = "temple", id = t.Id, name = t.Name, image = t.ImageUrl }));

            suggestions.AddRange(_context.Museums
                .Where(m => m.Name.Contains(term) || (m.NameAr != null && m.NameAr.Contains(term)))
                .Take(2)
                .Select(m => new { type = "museum", id = m.Id, name = m.Name, image = m.ImageUrl }));

            suggestions.AddRange(_context.Gods
                .Where(g => g.Name.Contains(term) || (g.NameAr != null && g.NameAr.Contains(term)))
                .Take(2)
                .Select(g => new { type = "god", id = g.Id, name = g.Name, image = g.ImageUrl }));

            suggestions.AddRange(_context.Artifacts
                .Where(a => a.Name.Contains(term) || (a.NameAr != null && a.NameAr.Contains(term)))
                .Take(2)
                .Select(a => new { type = "artifact", id = a.Id, name = a.Name, image = a.ImageUrl }));

            suggestions.AddRange(_context.Dynasties
                .Where(d => d.Name.Contains(term) || (d.NameAr != null && d.NameAr.Contains(term)))
                .Take(2)
                .Select(d => new { type = "dynasty", id = d.Id, name = d.Name, image = d.ImageUrl }));

            return Json(suggestions.Take(8));
        }

        // 🆕 بيتنادى من الـ JS لما اليوزر يضغط على نتيجة بحث معينة — بيسجل ResultType
        [HttpPost]
        public IActionResult TrackSearchClick(string query, string resultType)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new { success = false });

            _context.SearchHistories.Add(new SearchHistory
            {
                UserEmail = HttpContext.Session.GetString("UserEmail"),
                Query = query,
                SearchedAt = DateTime.Now,
                ResultType = resultType
            });
            _context.SaveChanges();

            return Json(new { success = true });
        }

        private void LogSearch(string q)
        {
            _context.SearchHistories.Add(new SearchHistory
            {
                UserEmail = HttpContext.Session.GetString("UserEmail"),
                Query = q,
                SearchedAt = DateTime.Now
            });
            _context.SaveChanges();
        }

        // آخر 5 عمليات بحث مختلفة لنفس اليوزر (من غير الكلمة الحالية)
        private List<string> GetRecentSearches(string currentQuery)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return new List<string>();

            return _context.SearchHistories
                .Where(s => s.UserEmail == email && s.Query != currentQuery)
                .OrderByDescending(s => s.SearchedAt)
                .Select(s => s.Query)
                .Distinct()
                .Take(5)
                .ToList();
        }

        // أكتر 5 كلمات اتبحثت آخر 7 أيام (كل اليوزرز)، من غير الكلمة الحالية
        private List<string> GetTrendingSearches(string currentQuery)
        {
            var since = DateTime.Now.AddDays(-7);

            return _context.SearchHistories
                .Where(s => s.SearchedAt >= since && s.Query != currentQuery)
                .GroupBy(s => s.Query)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(5)
                .ToList();
        }

        // =========================================================
        // 🔤 Fuzzy Matching (Levenshtein) — بيشتغل بس لو الـ Contains العادي رجّع صفر نتائج
        // =========================================================

        private List<T> FuzzyFilter<T>(List<T> items, string query, Func<T, IEnumerable<string?>> fieldsSelector)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<T>();

            int maxDistance = query.Length <= 4 ? 1 : 2;

            return items
                .Where(item => fieldsSelector(item).Any(field => IsFuzzyMatch(field, query, maxDistance)))
                .ToList();
        }

        private bool IsFuzzyMatch(string? source, string query, int maxDistance)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            var words = source.Split(new[] { ' ', ',', '-', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            var q = query.ToLowerInvariant();

            foreach (var word in words)
            {
                if (LevenshteinDistance(word.ToLowerInvariant(), q) <= maxDistance)
                    return true;
            }

            return false;
        }

        private int LevenshteinDistance(string a, string b)
        {
            var dp = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            return dp[a.Length, b.Length];
        }

        public IActionResult Timeline()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            var pharaohs = _context.Pharaohs
                .OrderBy(p => p.Dynasty)
                .ThenBy(p => p.Period)
                .ToList();

            var grouped = pharaohs
                .GroupBy(p => p.Dynasty)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(grouped);
        }
        public IActionResult Map()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            ViewBag.Temples = _context.Temples.ToList();
            ViewBag.Museums = _context.Museums.ToList();
            return View();
        }
    }
}
