using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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

            var temples = _context.Temples
                .Where(t => t.Name.Contains(q) || t.Location.Contains(q)
                         || (t.NameAr != null && t.NameAr.Contains(q))
                         || (t.LocationAr != null && t.LocationAr.Contains(q)))
                .ToList();

            var gods = _context.Gods
                .Where(g => g.Name.Contains(q) || g.Role.Contains(q)
                         || (g.NameAr != null && g.NameAr.Contains(q))
                         || (g.RoleAr != null && g.RoleAr.Contains(q)))
                .ToList();

            var museums = _context.Museums
                .Where(m => m.Name.Contains(q) || m.Location.Contains(q)
                         || (m.NameAr != null && m.NameAr.Contains(q))
                         || (m.LocationAr != null && m.LocationAr.Contains(q)))
                .ToList();

            var artifacts = _context.Artifacts
                .Where(a => a.Name.Contains(q) || a.Category.Contains(q) || a.Origin.Contains(q)
                         || (a.NameAr != null && a.NameAr.Contains(q))
                         || (a.CategoryAr != null && a.CategoryAr.Contains(q))
                         || (a.OriginAr != null && a.OriginAr.Contains(q)))
                .ToList();

            var dynasties = _context.Dynasties
                .Where(d => d.Name.Contains(q) || d.Era.Contains(q) || d.CapitalCity.Contains(q)
                         || (d.NameAr != null && d.NameAr.Contains(q))
                         || (d.EraAr != null && d.EraAr.Contains(q))
                         || (d.CapitalCityAr != null && d.CapitalCityAr.Contains(q)))
                .ToList();

            ViewBag.Query = q;
            ViewBag.Pharaohs = pharaohs;
            ViewBag.Temples = temples;
            ViewBag.Gods = gods;
            ViewBag.Museums = museums;
            ViewBag.Artifacts = artifacts;
            ViewBag.Dynasties = dynasties;

            return View();
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