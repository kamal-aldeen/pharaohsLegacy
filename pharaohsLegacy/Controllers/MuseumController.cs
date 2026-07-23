using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    public class MuseumController : Controller
    {
        private AppDbContext context;

        public MuseumController(AppDbContext _context)
        {
            context = _context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "User");

            ViewBag.Egyptian = context.Museums
                .Where(m => m.Category == "Egyptian")
                .ToList();

            ViewBag.World = context.Museums
                .Where(m => m.Category == "World")
                .ToList();

            // 🆕 أسعار كل المتاحف دفعة واحدة (PlaceId -> Amount)
            ViewBag.Prices = context.Prices
                .Where(p => p.PlaceType == "Museum")
                .ToDictionary(p => p.PlaceId, p => p.Amount);

            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var museum = await context.Museums.FindAsync(id);
            if (museum == null) return NotFound();

            var email = HttpContext.Session.GetString("UserEmail");

            var fav = email != null && email != "guest"
                ? await context.Favorites
                    .FirstOrDefaultAsync(f => f.UserEmail == email && f.Type == "museum" && f.ItemId == id)
                : null;

            ViewBag.IsFav = fav != null;
            ViewBag.FavId = fav?.Id ?? 0;

            // 🆕 السعر بقى بييجي من جدول Prices بدل ما يكون ثابت في الـ View — 100 قيمة احتياطية بس
            // لو معملش سعر للمتحف ده لسه (في الأدمن أو الـ SQL)
            var priceRecord = await context.Prices
                .FirstOrDefaultAsync(p => p.PlaceType == "Museum" && p.PlaceId == id);
            ViewBag.Price = priceRecord?.Amount ?? 100;

            // ✅ Reviews
            var reviews = await context.Reviews
                .Where(r => r.Type == "museum" && r.ItemId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            bool hasReviewed = email != null && email != "guest" &&
                               reviews.Any(r => r.UserEmail == email);

            ViewBag.Reviews = reviews;
            ViewBag.HasReviewed = hasReviewed;

            return View(museum);
        }

    }
}
