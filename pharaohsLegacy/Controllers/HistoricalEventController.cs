using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class HistoricalEventController : Controller
    {
        private readonly AppDbContext _context;
        public HistoricalEventController(AppDbContext context) => _context = context;

        public IActionResult Index(string? category)
        {
            var events = _context.HistoricalEvents.OrderBy(e => e.Year).ToList();

            if (!string.IsNullOrEmpty(category) && category != "All")
                events = events.Where(e => e.Category == category).ToList();

            ViewBag.Categories = new List<string> { "Political", "Military", "Religious", "Cultural", "Scientific" };
            ViewBag.SelectedCategory = category ?? "All";
            return View(events);
        }

        public IActionResult Details(int id)
        {
            var ev = _context.HistoricalEvents.FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();

            Dynasty? dynasty = null;
            if (!string.IsNullOrEmpty(ev.DynastyTag))
                dynasty = _context.Dynasties.FirstOrDefault(d => d.Name == ev.DynastyTag);

            Pharaoh? pharaoh = null;
            if (!string.IsNullOrEmpty(ev.PharaohTag))
                pharaoh = _context.Pharaohs.FirstOrDefault(p => p.Name == ev.PharaohTag);

            var relatedEvents = _context.HistoricalEvents
                .Where(e => e.Id != id && !string.IsNullOrEmpty(ev.DynastyTag) && e.DynastyTag == ev.DynastyTag)
                .OrderBy(e => e.Year)
                .Take(3)
                .ToList();

            ViewBag.Dynasty = dynasty;
            ViewBag.Pharaoh = pharaoh;
            ViewBag.RelatedEvents = relatedEvents;
            return View(ev);
        }
    }
}