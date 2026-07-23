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

            // 🆕 أسعار كل المعابد دفعة واحدة (PlaceId -> Amount)
            ViewBag.Prices = context.Prices
                .Where(p => p.PlaceType == "Temple")
                .ToDictionary(p => p.PlaceId, p => p.Amount);

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

            // 🆕 السعر بقى بييجي من جدول Prices بدل ما يكون ثابت في الـ View — 150 قيمة احتياطية بس
            // لو معملش سعر لِلمعبد ده لسه (في الأدمن أو الـ SQL)
            var priceRecord = await context.Prices
                .FirstOrDefaultAsync(p => p.PlaceType == "Temple" && p.PlaceId == id);
            ViewBag.Price = priceRecord?.Amount ?? 150;

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
