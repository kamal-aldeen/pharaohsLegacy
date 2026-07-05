using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class ArtifactController : Controller
    {
        private readonly AppDbContext _context;

        public ArtifactController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            var artifacts = _context.Artifacts.ToList();
            return View(artifacts);
        }
        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            var artifact = _context.Artifacts.FirstOrDefault(a => a.Id == id);
            if (artifact == null) return NotFound();

            var userEmail = HttpContext.Session.GetString("UserEmail");

            ViewBag.IsFav = _context.Favorites.Any(f =>
                f.UserEmail == userEmail && f.Type == "artifact" && f.ItemId == id);

            ViewBag.FavId = _context.Favorites
                .FirstOrDefault(f => f.UserEmail == userEmail && f.Type == "artifact" && f.ItemId == id)?.Id;

            // ✅ Reviews
            var reviews = _context.Reviews
                .Where(r => r.Type == "artifact" && r.ItemId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            bool hasReviewed = !string.IsNullOrEmpty(userEmail) &&
                               reviews.Any(r => r.UserEmail == userEmail);

            ViewBag.Reviews = reviews;
            ViewBag.HasReviewed = hasReviewed;

            return View(artifact);
        }
    }
}