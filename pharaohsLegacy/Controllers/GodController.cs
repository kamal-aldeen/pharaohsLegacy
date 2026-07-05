using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class GodController : Controller
    {
        private readonly AppDbContext _context;
        public GodController(AppDbContext context) => _context = context;

        
        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "User");

            var gods = await _context.Gods.OrderBy(g => g.Name).ToListAsync();
            return View(gods);
        }



        public async Task<IActionResult> Details(int id)
        {
            var god = await _context.Gods.FindAsync(id);
            if (god == null) return NotFound();

            var email = HttpContext.Session.GetString("UserEmail");

            var fav = email != null && email != "guest"
                ? await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserEmail == email && f.Type == "god" && f.ItemId == id)
                : null;

            ViewBag.IsFav = fav != null;
            ViewBag.FavId = fav?.Id ?? 0;

            // ✅ Reviews
            var reviews = await _context.Reviews
                .Where(r => r.Type == "god" && r.ItemId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            bool hasReviewed = email != null && email != "guest" &&
                               reviews.Any(r => r.UserEmail == email);

            ViewBag.Reviews = reviews;
            ViewBag.HasReviewed = hasReviewed;

            return View(god);
        }



    }
}