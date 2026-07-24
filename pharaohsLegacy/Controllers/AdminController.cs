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

            // 🆕 Shop Orders — نفس فكرة bookingRows فوق، بس لأوردرات الشوب (بما فيها Items وتراك الشحن)
            var shopOrders = await _context.ShopOrders
                .Include(o => o.Items)
                .Where(o => o.Status != "PendingPayment")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var shopOrderProductIds = shopOrders.SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToList();
            var shopOrderProducts = await _context.Products
                .Where(p => shopOrderProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var o in shopOrders)
            {
                foreach (var item in o.Items)
                {
                    if (shopOrderProducts.TryGetValue(item.ProductId, out var p))
                        item.ProductName = p.Name;
                }
            }

            // 🆕 أسعار المعابد والمتاحف الحالية — تتعرض جنب كل صف في تابي Temples/Museums
            // ViewBag[TemplePrices][temple.Id] => السعر، وهكذا للمتاحف
            // بنستخدم GroupBy بدل ToDictionaryAsync مباشرة كحماية إضافية لو حصل تكرار
            // (لو الـ Unique Index اتفعّل صح، التكرار مش المفروض يحصل من الأساس)
            var templePricesRaw = await _context.Prices.Where(p => p.PlaceType == "Temple").ToListAsync();
            ViewBag.TemplePrices = templePricesRaw
                .GroupBy(p => p.PlaceId)
                .ToDictionary(g => g.Key, g => g.First().Amount);

            var museumPricesRaw = await _context.Prices.Where(p => p.PlaceType == "Museum").ToListAsync();
            ViewBag.MuseumPrices = museumPricesRaw
                .GroupBy(p => p.PlaceId)
                .ToDictionary(g => g.Key, g => g.First().Amount);

            // 🆕 حوّشناهم local هنا (بدل ما يتعملوا query جوه الـ initializer بس) عشان نعيد
            // استخدامهم في حساب الـ Analytics (خصوصًا Reviews name-lookup) من غير Query زيادة
            var pharaohsList = await _context.Pharaohs.OrderBy(p => p.Name).ToListAsync();
            var templesList = await _context.Temples.OrderBy(t => t.Name).ToListAsync();
            var museumsList = await _context.Museums.OrderBy(m => m.Name).ToListAsync();
            var godsList = await _context.Gods.OrderBy(g => g.Name).ToListAsync();

            // ──────────────────────────────
            // 📊 ANALYTICS DASHBOARD DATA (بند 13)
            // ──────────────────────────────
            var last30Start = DateTime.Now.Date.AddDays(-29);

            // 1) Revenue Trend — آخر 30 يوم، حجوزات + شوب مع بعض
            var revenueBookingsRaw = bookings
                .Where(b => (b.Status == "Confirmed" || b.Status == "Visited") && b.CreatedAt.Date >= last30Start)
                .GroupBy(b => b.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalPrice));

            var revenueShopRaw = shopOrders
                .Where(o => o.Status == "Confirmed" && o.CreatedAt.Date >= last30Start)
                .GroupBy(o => o.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalPrice));

            var revenueTrend = new List<RevenuePoint>();
            for (var d = last30Start; d <= DateTime.Now.Date; d = d.AddDays(1))
            {
                revenueTrend.Add(new RevenuePoint
                {
                    Label = d.ToString("MMM dd"),
                    BookingRevenue = revenueBookingsRaw.TryGetValue(d, out var br) ? br : 0,
                    ShopRevenue = revenueShopRaw.TryGetValue(d, out var sr) ? sr : 0
                });
            }

            // 2) Most Booked Places — أعلى 5 أماكن حجزًا
            var topBookedPlaces = bookingRows
                .GroupBy(b => new { b.PlaceName, b.PlaceType })
                .Select(g => new PlaceBookingCount { PlaceName = g.Key.PlaceName, PlaceType = g.Key.PlaceType, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // 3) User Growth — تسجيلات جديدة آخر 30 يوم (⚠️ محتاج عمود Users.CreatedAt يتضاف الأول)
            var newUsersRaw = await _context.Users
                .Where(u => u.Email != AdminEmail && u.CreatedAt.Date >= last30Start)
                .ToListAsync();
            var usersByDate = newUsersRaw.GroupBy(u => u.CreatedAt.Date).ToDictionary(g => g.Key, g => g.Count());
            var userGrowth = new List<UserGrowthPoint>();
            for (var d = last30Start; d <= DateTime.Now.Date; d = d.AddDays(1))
            {
                userGrowth.Add(new UserGrowthPoint
                {
                    Label = d.ToString("MMM dd"),
                    NewUsers = usersByDate.TryGetValue(d, out var c) ? c : 0
                });
            }

            // 4) Reviews Stats — متوسط عام + أعلى/أقل عنصر تقييمًا + متوسط لكل Type
            var allReviews = await _context.Reviews.ToListAsync();
            var reviewsStats = new ReviewsSummary();
            if (allReviews.Any())
            {
                reviewsStats.OverallAverageRating = Math.Round(allReviews.Average(r => r.Rating), 2);

                string GetPlaceName(string type, int itemId) => type?.ToLower() switch
                {
                    "pharaoh" => pharaohsList.FirstOrDefault(p => p.Id == itemId)?.Name ?? "Unknown",
                    "temple" => templesList.FirstOrDefault(t => t.Id == itemId)?.Name ?? "Unknown",
                    "museum" => museumsList.FirstOrDefault(m => m.Id == itemId)?.Name ?? "Unknown",
                    "god" => godsList.FirstOrDefault(g => g.Id == itemId)?.Name ?? "Unknown",
                    _ => "Unknown"
                };

                var reviewGroups = allReviews
                    .GroupBy(r => new { Type = r.Type, r.ItemId })
                    .Select(g => new { g.Key.Type, g.Key.ItemId, AvgRating = g.Average(r => r.Rating), Count = g.Count() })
                    .ToList();

                var topRated = reviewGroups.OrderByDescending(x => x.AvgRating).ThenByDescending(x => x.Count).FirstOrDefault();
                var lowestRated = reviewGroups.OrderBy(x => x.AvgRating).ThenByDescending(x => x.Count).FirstOrDefault();

                if (topRated != null)
                {
                    reviewsStats.TopRatedName = GetPlaceName(topRated.Type, topRated.ItemId);
                    reviewsStats.TopRatedAvg = Math.Round(topRated.AvgRating, 2);
                }
                if (lowestRated != null)
                {
                    reviewsStats.LowestRatedName = GetPlaceName(lowestRated.Type, lowestRated.ItemId);
                    reviewsStats.LowestRatedAvg = Math.Round(lowestRated.AvgRating, 2);
                }

                reviewsStats.AverageByType = allReviews
                    .GroupBy(r => r.Type)
                    .Select(g => new TypeRatingAvg { Type = g.Key, AverageRating = Math.Round(g.Average(r => r.Rating), 2) })
                    .ToList();
            }

            // 5) Quiz Stats — ⚠️ لازم يتأكد اسم الـ DbSet (مفترض _context.QuizHistories) وأسماء الأعمدة
            var allQuizHistories = await _context.QuizHistories.ToListAsync();
            var quizStats = new QuizSummary();
            if (allQuizHistories.Any())
            {
                quizStats.TotalPlayers = allQuizHistories.Select(q => q.UserEmail).Distinct().Count();
                quizStats.TotalPlays = allQuizHistories.Count;
                quizStats.AverageScorePercent = Math.Round(allQuizHistories.Average(q => q.ScorePercent), 2);
                quizStats.AverageStreakDays = Math.Round(allQuizHistories.Average(q => q.StreakDays), 2);
                quizStats.GradeDistribution = allQuizHistories
                    .GroupBy(q => q.Grade)
                    .Select(g => new GradeCount { Grade = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();
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
                Pharaohs = pharaohsList,
                Temples = templesList,
                Museums = museumsList,
                Users = await _context.Users.Where(u => u.Email != AdminEmail).OrderBy(u => u.Name).ToListAsync(),
                Gods = godsList,
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
                ShopOrders = shopOrders, // 🆕 تراك + إدارة حالة أوردرات الشوب (راجع تحت قبل الـ Index)

                // 🆕 Categories
                TotalCategories = await _context.Categories.CountAsync(),
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),

                Bookings = bookingRows,

                // 🆕 Analytics Dashboard (بند 13)
                RevenueTrend = revenueTrend,
                TopBookedPlaces = topBookedPlaces,
                UserGrowth = userGrowth,
                ReviewsStats = reviewsStats,
                QuizStats = quizStats,


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

            // 🆕 نضيف سعر افتراضي (150) للمعبد الجديد في جدول Prices عشان ميفضلش من غير
            // سعر مسجل — الأدمن يقدر يعدله بعدين من EditPrice
            _context.Prices.Add(new Price { PlaceType = "Temple", PlaceId = model.Id, Amount = 150 });
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
            if (item != null)
            {
                _context.Temples.Remove(item);

                // 🆕 نمسح السعر المرتبط بيه من جدول Prices عشان ميفضلش صف يتيم
                var relatedPrice = await _context.Prices
                    .FirstOrDefaultAsync(p => p.PlaceType == "Temple" && p.PlaceId == id);
                if (relatedPrice != null) _context.Prices.Remove(relatedPrice);

                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Temple deleted.";
            return RedirectToAction("Index", new { tab = "temples" });
        }


        // ──────────────────────────────
        // 🆕 PRICE — تعديل سعر أي معبد أو متحف (جدول Prices واحد للاتنين)
        // ──────────────────────────────

        [HttpPost]
        public async Task<IActionResult> EditPrice(string placeType, int placeId, decimal amount)
        {
            if (!IsAdmin()) return Unauthorized();
            if (placeType != "Temple" && placeType != "Museum") return BadRequest();

            var existing = await _context.Prices
                .FirstOrDefaultAsync(p => p.PlaceType == placeType && p.PlaceId == placeId);

            if (existing == null)
                _context.Prices.Add(new Price { PlaceType = placeType, PlaceId = placeId, Amount = amount });
            else
                existing.Amount = amount;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Price updated successfully!";
            return RedirectToAction("Index", new { tab = placeType == "Temple" ? "temples" : "museums" });
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

            // 🆕 نضيف سعر افتراضي (100) للمتحف الجديد في جدول Prices عشان ميفضلش من غير
            // سعر مسجل — الأدمن يقدر يعدله بعدين من EditPrice
            _context.Prices.Add(new Price { PlaceType = "Museum", PlaceId = model.Id, Amount = 100 });
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
            if (item != null)
            {
                _context.Museums.Remove(item);

                // 🆕 نمسح السعر المرتبط بيه من جدول Prices عشان ميفضلش صف يتيم
                var relatedPrice = await _context.Prices
                    .FirstOrDefaultAsync(p => p.PlaceType == "Museum" && p.PlaceId == id);
                if (relatedPrice != null) _context.Prices.Remove(relatedPrice);

                await _context.SaveChangesAsync();
            }
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

        // 🆕 نفس فكرة ChangeBookingStatus بالظبط بس لأوردرات الشوب — بيغيّر حالة الدفع
        // (Confirmed/Cancelled/Refunded) عن طريق ShopOrderStatusService (المصدر الوحيد للحقيقة)
        [HttpPost]
        public async Task<IActionResult> ChangeShopOrderStatus(int id, string status)
        {
            if (!IsAdmin()) return Unauthorized();

            var order = await _context.ShopOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var statusService = new ShopOrderStatusService(_context, _httpClientFactory);
            var result = await statusService.ChangeStatusAsync(order, status);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction("Index", new { tab = "shop-orders" });
        }

        // 🆕 تحديث تراك الشحن (Processing → Shipped → Delivered) — منفصل تمامًا عن حالة الدفع،
        // مفيش نداء بنك هنا خالص، مجرد تحديث حقل عادي. بس متاح للأوردرات Confirmed
        // (لو الأوردر اتلغى/اترفند، مفيش معنى إننا نحدّث تراك شحن لطلب أصلاً مش هيتشحن)
        [HttpPost]
        public async Task<IActionResult> UpdateShopOrderShipping(int id, string shippingStatus)
        {
            if (!IsAdmin()) return Unauthorized();

            var validStatuses = new[] { "Processing", "Shipped", "Delivered" };
            if (!validStatuses.Contains(shippingStatus))
            {
                TempData["Error"] = "Invalid shipping status.";
                return RedirectToAction("Index", new { tab = "shop-orders" });
            }

            var order = await _context.ShopOrders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Confirmed")
            {
                TempData["Error"] = "Shipping status can only be updated for confirmed orders.";
                return RedirectToAction("Index", new { tab = "shop-orders" });
            }

            order.ShippingStatus = shippingStatus;

            // 🆕 لو الأدمن نقل الأوردر لـ Shipped يدويًا وShippedAt لسه فاضي، سجله دلوقتي —
            // عشان الحساب التلقائي لـ Delivered (المعتمد على ShippedAt) في
            // ShopOrderShippingBackgroundService يشتغل صح بعد كده
            if (shippingStatus == "Shipped" && order.ShippedAt == null)
                order.ShippedAt = DateTime.Now;

            // 🆕 لو نقلها لـ Delivered مباشرة (تخطي Shipped)، سجل DeliveredAt للعرض بس في MyOrders
            if (shippingStatus == "Delivered" && order.DeliveredAt == null)
                order.DeliveredAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order #{order.Id} marked as {shippingStatus}.";
            return RedirectToAction("Index", new { tab = "shop-orders" });
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
            model.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU.Trim(); // 🆕 المرحلة 4

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
            existing.SKU = string.IsNullOrWhiteSpace(model.SKU) ? null : model.SKU.Trim(); // 🆕 المرحلة 4

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

            // 🆕 لو المنتج ده اتباع قبل كده (فيه ShopOrderItems مرتبطة بأوردر مؤكد)، حذفه المباشر هيبوّظ
            // الـ Foreign Key / سجل الطلبات القديمة في MyOrders (ProductId هيفضل يشاور على حاجة مش موجودة).
            // بنمنع الحذف في الحالة دي بدل ما نكسر بيانات تاريخية، زي ما بنعمل مع أي جدول مرتبط بحجوزات.
            // 🔄 بعد تعدد المنتجات في الأوردر (ShopOrderItem)، الفحص بقى عن طريق join مع ShopOrder للحالة
            var hasOrders = await _context.ShopOrderItems
                .AnyAsync(oi => oi.ProductId == id && oi.ShopOrder!.Status != "PendingPayment");
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