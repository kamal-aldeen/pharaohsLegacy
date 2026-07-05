using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class FavoriteController : Controller
    {
        private AppDbContext context;

        public FavoriteController(AppDbContext _context)
        {
            context = _context;
        }

        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userEmail == null || userEmail == "guest")
                return RedirectToAction("Login", "User");

            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot use favorites!";
                return RedirectToAction("Index", "Home");
            }

            var favPharaohs = context.Favorites
                .Where(f => f.UserEmail == userEmail && f.Type == "pharaoh")
                .Join(context.Pharaohs, f => f.ItemId, p => p.Id, (f, p) => p)
                .ToList();

            var favTemples = context.Favorites
                .Where(f => f.UserEmail == userEmail && f.Type == "temple")
                .Join(context.Temples, f => f.ItemId, t => t.Id, (f, t) => t)
                .ToList();

            var favGods = context.Favorites
                .Where(f => f.UserEmail == userEmail && f.Type == "god")
                .Join(context.Gods, f => f.ItemId, g => g.Id, (f, g) => g)
                .ToList();

            var favMuseums = context.Favorites
                .Where(f => f.UserEmail == userEmail && f.Type == "museum")
                .Join(context.Museums, f => f.ItemId, m => m.Id, (f, m) => m)
                .ToList();

            // ✅ Artifacts
            var favArtifacts = context.Favorites
                .Where(f => f.UserEmail == userEmail && f.Type == "artifact")
                .Join(context.Artifacts, f => f.ItemId, a => a.Id, (f, a) => a)
                .ToList();

            ViewBag.Museums = favMuseums;
            ViewBag.Gods = favGods;
            ViewBag.Pharaohs = favPharaohs;
            ViewBag.Temples = favTemples;
            ViewBag.Artifacts = favArtifacts;

            return View();
        }

        public async Task<IActionResult> Add(string type, int itemId, string? returnUrl = null)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null || email == "guest")
                return RedirectToAction("Login", "User");

            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot add favorites!";
                return RedirectToAction("Index", "Home");
            }

            // ✅ artifact مضاف هنا
            bool exists = type.ToLower() switch
            {
                "pharaoh" => await context.Pharaohs.AnyAsync(p => p.Id == itemId),
                "temple" => await context.Temples.AnyAsync(t => t.Id == itemId),
                "god" => await context.Gods.AnyAsync(g => g.Id == itemId),
                "museum" => await context.Museums.AnyAsync(m => m.Id == itemId),
                "artifact" => await context.Artifacts.AnyAsync(a => a.Id == itemId),
                _ => false
            };

            if (!exists) return NotFound();

            var already = await context.Favorites
                .AnyAsync(f => f.UserEmail == email && f.Type == type && f.ItemId == itemId);

            if (!already)
            {
                context.Favorites.Add(new Favorite
                {
                    UserEmail = email,
                    Type = type.ToLower(),
                    ItemId = itemId
                });
                await context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            // ✅ redirect لـ Artifact/Details
            return type.ToLower() switch
            {
                "pharaoh" => RedirectToAction("Details", "Pharaoh", new { id = itemId }),
                "temple" => RedirectToAction("Details", "Temple", new { id = itemId }),
                "god" => RedirectToAction("Details", "God", new { id = itemId }),
                "museum" => RedirectToAction("Details", "Museum", new { id = itemId }),
                "artifact" => RedirectToAction("Details", "Artifact", new { id = itemId }),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public async Task<IActionResult> Remove(int id, string? returnUrl = null)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null || email == "guest")
                return RedirectToAction("Login", "User");

            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot remove favorites!";
                return RedirectToAction("Index", "Home");
            }

            var fav = await context.Favorites.FirstOrDefaultAsync(f =>
                f.Id == id && f.UserEmail == email);

            if (fav == null)
                return RedirectToAction("Dashboard", "User", new { tab = "favorites" });

            context.Favorites.Remove(fav);
            await context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard", "User", new { tab = "favorites" });
        }
    }
}