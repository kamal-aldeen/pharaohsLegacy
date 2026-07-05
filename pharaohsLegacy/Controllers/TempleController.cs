using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class TempleController : Controller
    {
        private AppDbContext context;

        public TempleController(AppDbContext _context)
        {
            context = _context;
        }

      
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            var temples = context.Temples.ToList();
            return View(temples);
        }



        public async Task<IActionResult> Details(int id)
        {
            var temple = await context.Temples.FindAsync(id);
            if (temple == null) return NotFound();

            var email = HttpContext.Session.GetString("UserEmail");

            var fav = email != null && email != "guest"
                ? await context.Favorites
                    .FirstOrDefaultAsync(f => f.UserEmail == email && f.Type == "temple" && f.ItemId == id)
                : null;

            ViewBag.IsFav = fav != null;
            ViewBag.FavId = fav?.Id ?? 0;

            // ✅ Reviews
            var reviews = await context.Reviews
                .Where(r => r.Type == "temple" && r.ItemId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            bool hasReviewed = email != null && email != "guest" &&
                               reviews.Any(r => r.UserEmail == email);

            ViewBag.Reviews = reviews;
            ViewBag.HasReviewed = hasReviewed;

            return View(temple);
        }
    }
}