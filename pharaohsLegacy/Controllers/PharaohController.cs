using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class PharaohController : Controller
    {
        private AppDbContext context;

        public PharaohController(AppDbContext _context)
        {
            context = _context;
        }

        
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            var pharaohs = context.Pharaohs.ToList();
            return View(pharaohs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pharaoh = await context.Pharaohs.FindAsync(id);
            if (pharaoh == null) return NotFound();

            var email = HttpContext.Session.GetString("UserEmail");

            var fav = email != null && email != "guest"
                ? await context.Favorites
                    .FirstOrDefaultAsync(f => f.UserEmail == email && f.Type == "pharaoh" && f.ItemId == id)
                : null;

            ViewBag.IsFav = fav != null;
            ViewBag.FavId = fav?.Id ?? 0;

            // ✅ Reviews
            var reviews = await context.Reviews
                .Where(r => r.Type == "pharaoh" && r.ItemId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var events = context.HistoricalEvents
    .Where(e => e.PharaohTag == pharaoh.Name)
    .OrderBy(e => e.Year).ToList();
            ViewBag.HistoricalEvents = events;
           
            bool hasReviewed = email != null && email != "guest" &&
                               reviews.Any(r => r.UserEmail == email);

            ViewBag.Reviews = reviews;
            ViewBag.HasReviewed = hasReviewed;

            return View(pharaoh);
        }








    }


}