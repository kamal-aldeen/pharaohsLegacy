// Controllers/DynastyController.cs
using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using Microsoft.EntityFrameworkCore;

namespace pharaohsLegacy.Controllers
{
    public class DynastyController : Controller
    {
        private readonly AppDbContext _context;

        public DynastyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Dynasty
        public IActionResult Index()
        {
            var dynasties = _context.Dynasties
                .OrderBy(d => d.StartYear)
                .ToList();

            // Group by Era
            var grouped = dynasties
                .GroupBy(d => d.Era)
                .OrderBy(g => g.Min(d => d.StartYear))
                .ToDictionary(g => g.Key, g => g.OrderBy(d => d.StartYear).ToList());

            return View(grouped);
        }

        // GET: /Dynasty/Details/5
        public IActionResult Details(int id)
        {
            var dynasty = _context.Dynasties.FirstOrDefault(d => d.Id == id);
            if (dynasty == null) return NotFound();

            // جيب الفراعنة المنتمين لهذه الأسرة
            var pharaohs = _context.Pharaohs.Where(p => p.Dynasty != null && p.Dynasty.ToLower() == dynasty.PharaohTag.ToLower()).ToList();

            // جيب الآثار المرتبطة بنفس الحقبة
            var artifacts = _context.Artifacts
                .Where(a => a.Period != null &&
                            a.Period.Contains(dynasty.StartYear > 0
                                ? dynasty.StartYear.ToString()
                                : Math.Abs(dynasty.StartYear).ToString()))
                .Take(6)
                .ToList();

            var events = _context.HistoricalEvents
   .Where(e => e.DynastyTag == dynasty.Name)
   .OrderBy(e => e.Year).ToList();
            ViewBag.HistoricalEvents = events;
            // الأسرة السابقة والتالية للـ navigation
            var allDynasties = _context.Dynasties.OrderBy(d => d.StartYear).ToList();
            var currentIndex = allDynasties.FindIndex(d => d.Id == id);
            var prevDynasty = currentIndex > 0 ? allDynasties[currentIndex - 1] : null;
            var nextDynasty = currentIndex < allDynasties.Count - 1 ? allDynasties[currentIndex + 1] : null;

            ViewBag.Pharaohs = pharaohs;
            ViewBag.Artifacts = artifacts;
            ViewBag.PrevDynasty = prevDynasty;
            ViewBag.NextDynasty = nextDynasty;

            return View(dynasty);
        }
    }
}