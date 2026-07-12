// Controllers/DynastyController.cs
using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

            // جيب الفراعنة المنتمين لهذه الأسرة (نفس PharaohTag)
            // + فلترة إضافية بسنين الحكم عشان الحقب الفرعية (زي Amarna Period
            //   جوه الأسرة 18) متجيبش كل فراعنة الأسرة الأم، بس اللي فترة حكمهم
            //   فعليًا واقعة جوه مدى السنين بتاع الحقبة دي
            var pharaohsInTag = _context.Pharaohs
                .Where(p => p.Dynasty != null && p.Dynasty.ToLower() == dynasty.PharaohTag.ToLower())
                .ToList();

            var pharaohs = pharaohsInTag
                .Where(p =>
                {
                    var reignStartYear = ParsePharaohStartYear(p.Period);
                    // لو مقدرناش نقرأ السنة من النص، سيبه ظاهر بدل ما نضيّعه
                    if (reignStartYear == null) return true;
                    return reignStartYear >= dynasty.StartYear && reignStartYear <= dynasty.EndYear;
                })
                .ToList();

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

        // بيقرا أول سنة مذكورة في نص فترة الحكم (Period)، زي:
        // "1353–1336 BC" -> -1353   |   "c. 3200 BC" -> -3200   |   "44 BC" -> -44
        // BC بترجع رقم سالب، وأي حاجة تانية (AD أو من غير علامة) بترجع رقم موجب.
        // بترجع null لو مقدرتش تلاقي رقم في النص خالص.
        private static int? ParsePharaohStartYear(string? period)
        {
            if (string.IsNullOrWhiteSpace(period)) return null;

            var match = Regex.Match(period, @"\d+");
            if (!match.Success) return null;

            var year = int.Parse(match.Value);
            var isBC = period.Contains("BC", StringComparison.OrdinalIgnoreCase);

            return isBC ? -year : year;
        }
    }
}