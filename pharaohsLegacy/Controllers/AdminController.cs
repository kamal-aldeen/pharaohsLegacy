using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using pharaohsLegacy.ViewModels;
using pharaohsLegacy.Services;
using System.Data;

namespace pharaohsLegacy.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;


        private const string AdminEmail = "kamalabdlbast89@gmail.com";

        public AdminController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        private bool IsAdmin()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            return email == AdminEmail;
        }


        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            // 🆕 استبعدنا PendingPayment — ده حجز لسه ملوش دفع ناجح (زي لو اليوزر
            // وقف عند خطوة الـ OTP ومكملش)، فمش لازم يظهر في لوحة الأدمن ولا يتحسب في الإحصائيات
            var bookings = await _context.Bookings
                .Where(b => b.Status != "PendingPayment")
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            var bookingRows = new List<AdminBookingRow>();


            foreach (var b in bookings)
            {
                string placeName = "";
                if (b.PlaceType?.ToLower() == "temple")
                    placeName = (await _context.Temples.FindAsync(b.PlaceId))?.Name ?? "";
                else
                    placeName = (await _context.Museums.FindAsync(b.PlaceId))?.Name ?? "";

                bookingRows.Add(new AdminBookingRow
                {
                    Id = b.Id,
                    UserEmail = b.UserEmail,
                    PlaceName = placeName,
                    PlaceType = b.PlaceType,
                    VisitDate = b.VisitDate,
                    NumberOfTickets = b.NumberOfTickets,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                });
            }

            var vm = new AdminOverviewViewModel
            {
                TotalUsers = await _context.Users.CountAsync(u => u.Email != AdminEmail),
                TotalPharaohs = await _context.Pharaohs.CountAsync(),
                TotalTemples = await _context.Temples.CountAsync(),
                TotalMuseums = await _context.Museums.CountAsync(),
                TotalGods = await _context.Gods.CountAsync(),
                TotalArtifacts = await _context.Artifacts.CountAsync(),
                TotalReviews = await _context.Reviews.CountAsync(),
                TotalBookings = bookings.Count,
                TotalVisited = bookings.Count(b => b.Status == "Visited"),
                TotalRevenue = bookings.Where(b => b.Status == "Confirmed" || b.Status == "Visited").Sum(b => b.TotalPrice),
                TotalDynasties = _context.Dynasties.Count(),
                Pharaohs = await _context.Pharaohs.OrderBy(p => p.Name).ToListAsync(),
                Temples = await _context.Temples.OrderBy(t => t.Name).ToListAsync(),
                Museums = await _context.Museums.OrderBy(m => m.Name).ToListAsync(),
                Users = await _context.Users.Where(u => u.Email != AdminEmail).OrderBy(u => u.Name).ToListAsync(),
                Gods = await _context.Gods.OrderBy(g => g.Name).ToListAsync(),
                Artifacts = await _context.Artifacts.OrderBy(a => a.Name).ToListAsync(),
                Reviews = await _context.Reviews.OrderByDescending(r => r.CreatedAt).ToListAsync(),
                Reports = await _context.ReviewReports.OrderByDescending(r => r.CreatedAt).ToListAsync(),
                Dynasties = _context.Dynasties.OrderBy(d => d.StartYear).ToList(),
                TotalHistoricalEvents = await _context.HistoricalEvents.CountAsync(),
                HistoricalEvents = await _context.HistoricalEvents.OrderBy(e => e.Year).ToListAsync(),
                TotalFacts = await _context.DailyFacts.CountAsync(),
                Facts = await _context.DailyFacts.OrderBy(f => f.Id).ToListAsync(),

                // 🆕 Shop
                TotalProducts = await _context.Products.CountAsync(),
                Products = await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync(),
                TotalShopOrders = await _context.ShopOrders.CountAsync(o => o.Status != "PendingPayment"),
                TotalShopRevenue = await _context.ShopOrders
                    .Where(o => o.Status == "Confirmed")
                    .SumAsync(o => o.TotalPrice),

                // 🆕 Categories
                TotalCategories = await _context.Categories.CountAsync(),
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),

                Bookings = bookingRows,





            };



            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> AddPharaoh(Pharaoh model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Name = model.Name ?? "";
            model.Period = model.Period ?? "";
            model.Dynasty = model.Dynasty ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            _context.Pharaohs.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pharaoh added successfully!";
            return RedirectToAction("Index", new { tab = "pharaohs" });
        }

        [HttpPost]
        public async Task<IActionResult> EditPharaoh(Pharaoh model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Name = model.Name ?? "";
            model.Period = model.Period ?? "";
            model.Dynasty = model.Dynasty ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            var existing = await _context.Pharaohs.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Dynasty = model.Dynasty;
            existing.Period = model.Period;
            existing.Description = model.Description;
            existing.ImageUrl = model.ImageUrl;
            existing.NameAr = model.NameAr;
            existing.DynastyAr = model.DynastyAr;
            existing.PeriodAr = model.PeriodAr;
            existing.DescriptionAr = model.DescriptionAr;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Pharaoh updated successfully!";
            return RedirectToAction("Index", new { tab = "pharaohs" });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePharaoh(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var item = await _context.Pharaohs.FindAsync(id);
            if (item != null) { _context.Pharaohs.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Pharaoh deleted.";
            return RedirectToAction("Index", new { tab = "pharaohs" });
        }


        [HttpPost]
        public async Task<IActionResult> AddTemple(Temple model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Period = model.Period ?? "";
            model.Location = model.Location ?? "";
            model.Name = model.Name ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            _context.Temples.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Temple added successfully!";
            return RedirectToAction("Index", new { tab = "temples" });
        }

        [HttpPost]
        public async Task<IActionResult> EditTemple(Temple model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Period = model.Period ?? "";
            model.Location = model.Location ?? "";
            model.Name = model.Name ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            var existing = await _context.Temples.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Location = model.Location;
            existing.Period = model.Period;
            existing.Description = model.Description;
            existing.ImageUrl = model.ImageUrl;
            existing.NameAr = model.NameAr;
            existing.LocationAr = model.LocationAr;
            existing.PeriodAr = model.PeriodAr;
            existing.DescriptionAr = model.DescriptionAr;


            await _context.SaveChangesAsync();
            TempData["Success"] = "Temple updated successfully!";
            return RedirectToAction("Index", new { tab = "temples" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTemple(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var item = await _context.Temples.FindAsync(id);
            if (item != null) { _context.Temples.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Temple deleted.";
            return RedirectToAction("Index", new { tab = "temples" });
        }


        [HttpPost]
        public async Task<IActionResult> AddMuseum(Museum model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Location = model.Location ?? "";
            model.Location = model.Location ?? "";
            model.Founded = model.Founded ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            _context.Museums.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Museum added successfully!";
            return RedirectToAction("Index", new { tab = "museums" });
        }

        [HttpPost]
        public async Task<IActionResult> EditMuseum(Museum model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Location = model.Location ?? "";
            model.Location = model.Location ?? "";
            model.Founded = model.Founded ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            var existing = await _context.Museums.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Location = model.Location;
            existing.Founded = model.Founded;
            existing.Description = model.Description;
            existing.ImageUrl = model.ImageUrl;

            existing.Category = model.Category;
            existing.NameAr = model.NameAr;
            existing.LocationAr = model.LocationAr;
            existing.DescriptionAr = model.DescriptionAr;
            existing.CategoryAr = model.CategoryAr;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Museum updated successfully!";
            return RedirectToAction("Index", new { tab = "museums" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMuseum(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var item = await _context.Museums.FindAsync(id);
            if (item != null) { _context.Museums.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Museum deleted.";
            return RedirectToAction("Index", new { tab = "museums" });
        }

        [HttpPost]
        public async Task<IActionResult> AddArtifact(Artifact model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Name = model.Name ?? "";
            model.Origin = model.Origin ?? "";
            model.Period = model.Period ?? "";
            model.Category = model.Category ?? "";
            model.Description = model.Description ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            model.Museum = model.Museum ?? "";
            model.CurrentLocation = model.CurrentLocation ?? "";
            _context.Artifacts.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Artifact added successfully!";
            return RedirectToAction("Index", new { tab = "artifacts" });
        }

        [HttpPost]
        public async Task<IActionResult> EditArtifact(Artifact model)
        {
            if (!IsAdmin()) return Unauthorized();
            var existing = await _context.Artifacts.FindAsync(model.Id);
            if (existing == null) return NotFound();
            existing.Name = model.Name ?? "";
            existing.Origin = model.Origin ?? "";
            existing.Period = model.Period ?? "";
            existing.Category = model.Category ?? "";
            existing.Description = model.Description ?? "";
            existing.ImageUrl = model.ImageUrl ?? "";
            existing.Museum = model.Museum ?? "";
            existing.CurrentLocation = model.CurrentLocation ?? "";
            existing.NameAr = model.NameAr;
            existing.OriginAr = model.OriginAr;
            existing.PeriodAr = model.PeriodAr;
            existing.CategoryAr = model.CategoryAr;
            existing.DescriptionAr = model.DescriptionAr;
            existing.CurrentLocationAr = model.CurrentLocationAr;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Artifact updated successfully!";
            return RedirectToAction("Index", new { tab = "artifacts" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteArtifact(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var item = await _context.Artifacts.FindAsync(id);
            if (item != null) { _context.Artifacts.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Artifact deleted.";
            return RedirectToAction("Index", new { tab = "artifacts" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeBookingStatus(int id, string status)
        {
            if (!IsAdmin()) return Unauthorized();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            // 🆕 كل منطق الانتقال بين الحالات (وقرار نداء البنك من عدمه) بقى مركزي
            // في BookingStatusService — عشان الأدمن مايقدرش يعمل انتقال غير منطقي
            // (زي إرجاع Refunded لـ Confirmed، أو تأكيد حجز فات معاده) ولضمان إن
            // الحالة المحلية تفضل متطابقة تمامًا مع اللي حصل في البنك فعليًا.
            var statusService = new BookingStatusService(_context, _httpClientFactory);
            var result = await statusService.ChangeStatusAsync(booking, status);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction("Index", new { tab = "bookings" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                //Foreign Key Constraint Error
                var favs = _context.Favorites.Where(f => f.UserEmail == user.Email);
                _context.Favorites.RemoveRange(favs);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "User deleted.";
            return RedirectToAction("Index", new { tab = "users" });
        }


        [HttpPost]
        public async Task<IActionResult> AddGod(God model)
        {
            if (!IsAdmin()) return Unauthorized();
            //Null Exception


            model.Description = model.Description ?? "";
            model.Symbol = model.Symbol ?? "";
            model.Role = model.Role ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            _context.Gods.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "God added successfully!";
            return RedirectToAction("Index", new { tab = "gods" });
        }

        [HttpPost]
        public async Task<IActionResult> EditGod(God model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.Description = model.Description ?? "";
            model.Symbol = model.Symbol ?? "";
            model.Role = model.Role ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            var existing = await _context.Gods.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Role = model.Role;
            existing.Description = model.Description;
            existing.ImageUrl = model.ImageUrl;
            existing.Symbol = model.Symbol;
            existing.NameAr = model.NameAr;
            existing.RoleAr = model.RoleAr;
            existing.DescriptionAr = model.DescriptionAr;
            existing.SymbolAr = model.SymbolAr;

            await _context.SaveChangesAsync();
            TempData["Success"] = "God updated successfully!";
            return RedirectToAction("Index", new { tab = "gods" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGod(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var item = await _context.Gods.FindAsync(id);
            if (item != null) { _context.Gods.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "God deleted.";
            return RedirectToAction("Index", new { tab = "gods" });
        }

        // ──────────────────────────────
        // 🆕 SHOP / PRODUCT CRUD — نفس باترن الـ Gods بالظبط
        // ──────────────────────────────

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product model)
        {
            if (!IsAdmin()) return Unauthorized();

            model.Description = model.Description ?? "";
            model.DescriptionAr = model.DescriptionAr ?? "";
            model.NameAr = model.NameAr ?? "";
            model.ImageUrl = model.ImageUrl ?? "";
            model.Material = model.Material ?? "";        // 🆕
            model.MaterialAr = model.MaterialAr ?? "";      // 🆕
            model.Dimensions = model.Dimensions ?? "";      // 🆕
            model.DimensionsAr = model.DimensionsAr ?? "";    // 🆕
            model.OriginRegion = model.OriginRegion ?? "";    // 🆕
            model.OriginRegionAr = model.OriginRegionAr ?? "";  // 🆕

            // 🆕 المرحلة 3 — لو OriginalPrice أقل من أو يساوي Price (خصم وهمي/غلط) بنتجاهلها
            // عشان الـ View مايبينش Strikethrough أو نسبة خصم غلط
            if (model.OriginalPrice.HasValue && model.OriginalPrice.Value <= model.Price)
                model.OriginalPrice = null;

            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product added successfully!";
            return RedirectToAction("Index", new { tab = "shop" });
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model)
        {
            if (!IsAdmin()) return Unauthorized();

            var existing = await _context.Products.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.NameAr = model.NameAr ?? "";
            existing.Description = model.Description ?? "";
            existing.DescriptionAr = model.DescriptionAr ?? "";
            existing.Price = model.Price;
            existing.ImageUrl = model.ImageUrl ?? "";
            existing.StockQuantity = model.StockQuantity;
            existing.CategoryId = model.CategoryId;   // 🆕
            existing.Material = model.Material ?? "";        // 🆕
            existing.MaterialAr = model.MaterialAr ?? "";      // 🆕
            existing.Dimensions = model.Dimensions ?? "";      // 🆕
            existing.DimensionsAr = model.DimensionsAr ?? "";    // 🆕
            existing.OriginRegion = model.OriginRegion ?? "";    // 🆕
            existing.OriginRegionAr = model.OriginRegionAr ?? "";  // 🆕

            // 🆕 المرحلة 3 — عروض وخصومات وشارات
            existing.OriginalPrice = (model.OriginalPrice.HasValue && model.OriginalPrice.Value > model.Price)
                ? model.OriginalPrice
                : null;
            existing.IsFeatured = model.IsFeatured;
            existing.IsBestSeller = model.IsBestSeller;
            existing.IsNew = model.IsNew;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction("Index", new { tab = "shop" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            // 🆕 لو المنتج ده اتباع قبل كده (فيه ShopOrders مرتبطة)، حذفه المباشر هيبوّظ
            // الـ Foreign Key / سجل الطلبات القديمة في MyOrders (ProductId هيفضل يشاور على حاجة مش موجودة).
            // بنمنع الحذف في الحالة دي بدل ما نكسر بيانات تاريخية، زي ما بنعمل مع أي جدول مرتبط بحجوزات.
            var hasOrders = await _context.ShopOrders.AnyAsync(o => o.ProductId == id && o.Status != "PendingPayment");
            if (hasOrders)
            {
                TempData["Error"] = "Can't delete a product with existing orders.";
                return RedirectToAction("Index", new { tab = "shop" });
            }

            var item = await _context.Products.FindAsync(id);
            if (item != null) { _context.Products.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Index", new { tab = "shop" });
        }

        // ──────────────────────────────
        // 🆕 CATEGORY CRUD — lookup table بسيطة (Name + NameAr بس)
        // ──────────────────────────────

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category model)
        {
            if (!IsAdmin()) return Unauthorized();

            model.NameAr = model.NameAr ?? "";
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category added successfully!";
            return RedirectToAction("Index", new { tab = "shop" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            // 🆕 منمنعش الحذف زي المنتجات — بس نفك ربط أي منتج مرتبط (CategoryId يرجع null)
            // بدل ما نكسر بيانات، لأن التصنيف هنا تسمية تنظيمية مش جزء أساسي من المنتج
            var linkedProducts = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();
            foreach (var p in linkedProducts) p.CategoryId = null;

            var cat = await _context.Categories.FindAsync(id);
            if (cat != null) { _context.Categories.Remove(cat); await _context.SaveChangesAsync(); }

            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index", new { tab = "shop" });
        }

        // ──────────────────────────────
        // DYNASTY CRUD
        // ──────────────────────────────

        [HttpGet]
        public IActionResult AddDynasty()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");
            return View();
        }

        [HttpPost]
        public IActionResult AddDynasty(Dynasty dynasty)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");
            if (ModelState.IsValid)
            {
                _context.Dynasties.Add(dynasty);
                _context.SaveChanges();
                TempData["Success"] = "Dynasty added successfully!";
                return RedirectToAction("Index", new { tab = "dynasties" });
            }
            return View(dynasty);
        }

        [HttpGet]
        public IActionResult EditDynasty(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");
            var dynasty = _context.Dynasties.Find(id);
            if (dynasty == null) return NotFound();
            return View(dynasty);
        }

        [HttpPost]
        public IActionResult EditDynasty(Dynasty dynasty)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            var existing = _context.Dynasties.Find(dynasty.Id);
            if (existing == null) return NotFound();

            existing.Name = dynasty.Name;
            existing.Era = dynasty.Era;
            existing.StartYear = dynasty.StartYear;
            existing.EndYear = dynasty.EndYear;
            existing.Description = dynasty.Description;
            existing.Achievements = dynasty.Achievements;
            existing.CapitalCity = dynasty.CapitalCity;
            existing.ImageUrl = dynasty.ImageUrl;
            existing.PharaohTag = dynasty.PharaohTag;
            existing.NameAr = dynasty.NameAr;
            existing.EraAr = dynasty.EraAr;
            existing.DescriptionAr = dynasty.DescriptionAr;
            existing.AchievementsAr = dynasty.AchievementsAr;
            existing.CapitalCityAr = dynasty.CapitalCityAr;

            _context.SaveChanges();
            TempData["Success"] = "Dynasty updated successfully!";
            return RedirectToAction("Index", new { tab = "dynasties" });
        }

        [HttpPost]
        public IActionResult DeleteDynasty(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");
            var dynasty = _context.Dynasties.Find(id);
            if (dynasty != null)
            {
                _context.Dynasties.Remove(dynasty);
                _context.SaveChanges();
                TempData["Success"] = "Dynasty deleted.";
            }
            return RedirectToAction("Index", new { tab = "dynasties" });
        }

        [HttpPost]
        public IActionResult AddHistoricalEvent(HistoricalEvent model)
        {
            if (!IsAdmin()) return Unauthorized();
            _context.HistoricalEvents.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Historical Event added successfully!";
            return RedirectToAction("Index", new { tab = "events" });
        }

        [HttpPost]
        public IActionResult EditHistoricalEvent(HistoricalEvent model)
        {
            if (!IsAdmin()) return Unauthorized();
            var ev = _context.HistoricalEvents.Find(model.Id);
            if (ev == null) return NotFound();
            ev.Title = model.Title;
            ev.Year = model.Year;
            ev.Category = model.Category;
            ev.Description = model.Description;
            ev.ImageUrl = model.ImageUrl;
            ev.DynastyTag = model.DynastyTag;
            ev.PharaohTag = model.PharaohTag;
            ev.TitleAr = model.TitleAr;
            ev.CategoryAr = model.CategoryAr;
            ev.DescriptionAr = model.DescriptionAr;
            _context.SaveChanges();
            TempData["Success"] = "Historical Event updated successfully!";
            return RedirectToAction("Index", new { tab = "events" });
        }

        [HttpPost]
        public IActionResult DeleteHistoricalEvent(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var ev = _context.HistoricalEvents.Find(id);
            if (ev != null) { _context.HistoricalEvents.Remove(ev); _context.SaveChanges(); }
            TempData["Success"] = "Historical Event deleted.";
            return RedirectToAction("Index", new { tab = "events" });
        }



        // ──────────────────────────────
        // DAILY FACT CRUD
        // ──────────────────────────────

        [HttpPost]
        public async Task<IActionResult> AddFact(DailyFact model)
        {
            if (!IsAdmin()) return Unauthorized();
            model.FactText = model.FactText ?? "";
            _context.DailyFacts.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Fact added successfully!";
            return RedirectToAction("Index", new { tab = "facts" });
        }

        [HttpPost]
        public async Task<IActionResult> EditFact(DailyFact model)
        {
            if (!IsAdmin()) return Unauthorized();
            var existing = await _context.DailyFacts.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.FactText = model.FactText ?? "";
            existing.FactTextAr = model.FactTextAr;
            existing.Category = model.Category;
            existing.CategoryAr = model.CategoryAr;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Fact updated successfully!";
            return RedirectToAction("Index", new { tab = "facts" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFact(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var item = await _context.DailyFacts.FindAsync(id);
            if (item != null) { _context.DailyFacts.Remove(item); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Fact deleted.";
            return RedirectToAction("Index", new { tab = "facts" });
        }

    }

}