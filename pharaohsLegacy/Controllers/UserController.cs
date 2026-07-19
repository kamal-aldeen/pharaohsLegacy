using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using pharaohsLegacy.Models.DTOs;
using pharaohsLegacy.ViewModels;

namespace pharaohsLegacy.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext context;
        private readonly IHttpClientFactory _httpClientFactory;

        // 🆕 إيميل التواصل لتفعيل الحساب البنكي يدويًا — نفس إيميل الأدمن المستخدم في AdminController
        private const string BankContactEmail = "kamalabdlbast89@gmail.com";

        public UserController(AppDbContext _context, IHttpClientFactory httpClientFactory)
        {
            context = _context;
            _httpClientFactory = httpClientFactory;
        }

        private static string ToArabicDigits(string input)
        {
            var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
            var sb = new System.Text.StringBuilder();
            foreach (var c in input)
                sb.Append(c >= '0' && c <= '9' ? arabicDigits[c - '0'] : c);
            return sb.ToString();
        }

        private static string FormatVisitDate(DateTime date, string lang)
        {
            var culture = lang == "ar"
                ? new System.Globalization.CultureInfo("ar-EG")
                : new System.Globalization.CultureInfo("en-US");
            var formatted = date.ToString("dd MMM yyyy", culture);
            return lang == "ar" ? ToArabicDigits(formatted) : formatted;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        public IActionResult Guest()
        {
            HttpContext.Session.SetString("UserEmail", "guest");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginDTO login)
        {
            if (!ModelState.IsValid)
                return View();

            var user = context.Users
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);

                if (user.Email == "kamalabdlbast89@gmail.com")
                {
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    HttpContext.Session.SetString("UserRole", "User");
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.LoginError = "Invalid email or password";
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterDTO register)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShowRegister = true;
                return View("Login");
            }

            bool emailExists = context.Users.Any(u => u.Email == register.Email);

            if (emailExists)
            {
                ViewBag.RegError = "This email is already registered";
                ViewBag.ShowRegister = true;
                return View("Login");
            }

            User user = new User
            {
                Name = register.Name,
                Email = register.Email,
                Password = register.Password
            };

            context.Users.Add(user);
            context.SaveChanges();

            return RedirectToAction("Login", "User");
        }


        public async Task<IActionResult> Dashboard(string tab = "overview")
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email) || email == "guest")
                return RedirectToAction("Login");

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return RedirectToAction("Login");

            var lang = HttpContext.Session.GetString("Lang") ?? "en";

            // ===== BOOKINGS =====
            // ملحوظة: شلنا شرط "&& b.Status == "Confirmed"" اللي كان هنا —
            // كان بيمنع أي حجز اتلغى أو اتعمله Refund من الوصول للداشبورد
            // من الأصل. الفلترة حسب الحالة (Confirmed/Refunded/Visited)
            // بتحصل تحت في كل حساب لوحده حسب الحاجة.
            // 🆕 استبعدنا PendingPayment تحديدًا — ده حجز لسه ملوش دفع ناجح
            // (زي لو اليوزر وقف عند خطوة الـ OTP ومكملش)، فمالوش لازمة يظهر هنا خالص
            var bookings = await context.Bookings
                .Where(b => b.UserEmail == email && b.Status != "PendingPayment")
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bookingCards = new List<BookingCardViewModel>();

            foreach (var b in bookings)
            {
                string placeName = b.PlaceName;
                string? imageUrl = null;

                if (b.PlaceType?.ToLower() == "temple")
                {
                    var temple = await context.Temples.FindAsync(b.PlaceId);
                    placeName = (lang == "ar" && !string.IsNullOrEmpty(temple?.NameAr)) ? temple.NameAr : (temple?.Name ?? placeName);
                    imageUrl = temple?.ImageUrl;
                }
                else
                {
                    var museum = await context.Museums.FindAsync(b.PlaceId);
                    placeName = (lang == "ar" && !string.IsNullOrEmpty(museum?.NameAr)) ? museum.NameAr : (museum?.Name ?? placeName);
                    imageUrl = museum?.ImageUrl;
                }

                bookingCards.Add(new BookingCardViewModel
                {
                    Id = b.Id,
                    PlaceName = placeName,
                    PlaceType = b.PlaceType,
                    ImageUrl = imageUrl,
                    VisitDate = b.VisitDate,
                    NumberOfTickets = b.NumberOfTickets,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                });
            }

            // ===== SHOP ORDERS COUNT =====
            // 🆕 عدد طلبات الشوب — بيتعرض كـ badge جنب تاب "My Orders" في الداشبورد.
            // مبعتهاش جوه DashboardViewModel عشان الكلاس ده مش متاح عندي دلوقتي —
            // ماشي بنفس أسلوب ViewBag.JourneyCount اللي تحت، فمفيش داعي نلمس الـ ViewModel خالص.
            // لو اسم الـ DbSet في AppDbContext مختلف عن "ShopOrders" غيّره هنا بس.
            ViewBag.TotalOrders = await context.ShopOrders
                .Where(o => o.UserEmail == email && o.Status != "PendingPayment")
                .CountAsync();

            // ===== FAVORITES =====
            var favorites = await context.Favorites
                .Where(f => f.UserEmail == email)
                .ToListAsync();

            var favPharaohs = new List<FavoriteCardViewModel>();
            var favTemples = new List<FavoriteCardViewModel>();
            var favGods = new List<FavoriteCardViewModel>();
            var favMuseums = new List<FavoriteCardViewModel>();
            var favArtifacts = new List<FavoriteCardViewModel>(); // ✅ جديد
            var favProducts = new List<FavoriteCardViewModel>(); // 🆕 المرحلة 4 — Wishlist

            foreach (var fav in favorites)
            {
                if (fav.Type.ToLower() == "pharaoh")
                {
                    var p = await context.Pharaohs.FindAsync(fav.ItemId);
                    if (p != null)
                        favPharaohs.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = p.Id,
                            Name = (lang == "ar" && !string.IsNullOrEmpty(p.NameAr)) ? p.NameAr : p.Name,
                            Type = "pharaoh",
                            ImageUrl = p.ImageUrl,
                            SubTitle = (lang == "ar" && !string.IsNullOrEmpty(p.DynastyAr)) ? p.DynastyAr : p.Dynasty
                        });
                }
                else if (fav.Type.ToLower() == "temple")
                {
                    var t = await context.Temples.FindAsync(fav.ItemId);
                    if (t != null)
                        favTemples.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = t.Id,
                            Name = (lang == "ar" && !string.IsNullOrEmpty(t.NameAr)) ? t.NameAr : t.Name,
                            Type = "temple",
                            ImageUrl = t.ImageUrl,
                            SubTitle = (lang == "ar" && !string.IsNullOrEmpty(t.LocationAr)) ? t.LocationAr : t.Location
                        });
                }
                else if (fav.Type.ToLower() == "god")
                {
                    var g = await context.Gods.FindAsync(fav.ItemId);
                    if (g != null)
                        favGods.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = g.Id,
                            Name = (lang == "ar" && !string.IsNullOrEmpty(g.NameAr)) ? g.NameAr : g.Name,
                            Type = "god",
                            ImageUrl = g.ImageUrl,
                            SubTitle = (lang == "ar" && !string.IsNullOrEmpty(g.RoleAr)) ? g.RoleAr : g.Role
                        });
                }
                else if (fav.Type.ToLower() == "museum")
                {
                    var m = await context.Museums.FindAsync(fav.ItemId);
                    if (m != null)
                        favMuseums.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = m.Id,
                            Name = (lang == "ar" && !string.IsNullOrEmpty(m.NameAr)) ? m.NameAr : m.Name,
                            Type = "museum",
                            ImageUrl = m.ImageUrl,
                            SubTitle = (lang == "ar" && !string.IsNullOrEmpty(m.LocationAr)) ? m.LocationAr : m.Location
                        });
                }
                // ✅ Artifacts
                else if (fav.Type.ToLower() == "artifact")
                {
                    var a = await context.Artifacts.FindAsync(fav.ItemId);
                    if (a != null)
                        favArtifacts.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = a.Id,
                            Name = (lang == "ar" && !string.IsNullOrEmpty(a.NameAr)) ? a.NameAr : a.Name,
                            Type = "artifact",
                            ImageUrl = a.ImageUrl,
                            SubTitle = (lang == "ar" && !string.IsNullOrEmpty(a.CategoryAr)) ? a.CategoryAr : a.Category
                        });
                }
                // 🆕 المرحلة 4 — Wishlist للمنتجات
                else if (fav.Type.ToLower() == "product")
                {
                    var pr = await context.Products.Include(pp => pp.Category).FirstOrDefaultAsync(pp => pp.Id == fav.ItemId);
                    if (pr != null)
                    {
                        var prCatName = pr.Category != null
                            ? ((lang == "ar" && !string.IsNullOrEmpty(pr.Category.NameAr)) ? pr.Category.NameAr : pr.Category.Name)
                            : "";
                        favProducts.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = pr.Id,
                            Name = (lang == "ar" && !string.IsNullOrEmpty(pr.NameAr)) ? pr.NameAr : pr.Name,
                            Type = "product",
                            ImageUrl = pr.ImageUrl,
                            SubTitle = prCatName
                        });
                    }
                }
            }

            // ===== DASHBOARD VIEWMODEL =====
            var vm = new DashboardViewModel
            {
                UserName = user.Name,
                UserEmail = user.Email,
                TotalBookings = bookingCards.Count,
                ActiveBookings = bookingCards.Count(b => b.Status == "Confirmed"),
                TotalFavorites = favorites.Count,
                VisitedCount = bookingCards.Count(b => b.Status == "Visited"),
                TotalSpent = bookingCards
                                    .Where(b => b.Status != "Cancelled" && b.Status != "Refunded")
                                    .Sum(b => b.TotalPrice),
                Bookings = bookingCards,
                FavoritePharaohs = favPharaohs,
                FavoriteTemples = favTemples,
                FavoriteGods = favGods,
                FavoriteMuseums = favMuseums,
                FavoriteArtifacts = favArtifacts,  // ✅ جديد
                FavoriteProducts = favProducts // 🆕 المرحلة 4 — Wishlist
            };

            // ===== MY JOURNEY PINS =====
            var journeyPins = new List<JourneyPin>();

            var confirmedBookings = bookings
                .Where(b => b.Status == "Confirmed" || b.Status == "Visited")
                .ToList();

            foreach (var booking in confirmedBookings)
            {
                double lat = 0, lng = 0;
                string name = "", img = "", desc = "";

                if (booking.PlaceType?.ToLower() == "temple")
                {
                    var temple = await context.Temples.FindAsync(booking.PlaceId);
                    if (temple == null || temple.Latitude == 0) continue;
                    lat = temple.Latitude;
                    lng = temple.Longitude;
                    name = (lang == "ar" && !string.IsNullOrEmpty(temple.NameAr)) ? temple.NameAr : temple.Name;
                    img = temple.ImageUrl ?? "";
                    desc = (lang == "ar" && !string.IsNullOrEmpty(temple.DescriptionAr)) ? temple.DescriptionAr : (temple.Description ?? "");
                }
                else if (booking.PlaceType?.ToLower() == "museum")
                {
                    var museum = await context.Museums.FindAsync(booking.PlaceId);
                    if (museum == null || museum.Latitude == 0) continue;
                    lat = museum.Latitude;
                    lng = museum.Longitude;
                    name = (lang == "ar" && !string.IsNullOrEmpty(museum.NameAr)) ? museum.NameAr : museum.Name;
                    img = museum.ImageUrl ?? "";
                    desc = (lang == "ar" && !string.IsNullOrEmpty(museum.DescriptionAr)) ? museum.DescriptionAr : (museum.Description ?? "");
                }

                if (journeyPins.Any(p => p.ItemId == booking.PlaceId
                    && p.Type == booking.PlaceType?.ToLower()
                    && p.PinType == "booking")) continue;

                journeyPins.Add(new JourneyPin
                {
                    ItemId = booking.PlaceId,
                    Name = name,
                    Type = booking.PlaceType?.ToLower() ?? "",
                    PinType = booking.Status == "Visited" ? "Visited" : "booking",
                    Latitude = lat,
                    Longitude = lng,
                    ImageUrl = img,
                    Description = desc.Length > 100 ? desc[..100] + "..." : desc,
                    VisitDate = FormatVisitDate(booking.VisitDate, lang),
                    Status = booking.Status ?? ""
                });
            }

            foreach (var fav in favorites.Where(f => f.Type.ToLower() == "temple"
                                                   || f.Type.ToLower() == "museum"))
            {
                bool alreadyBooked = journeyPins.Any(p => p.ItemId == fav.ItemId
                                                       && p.Type == fav.Type.ToLower());
                double lat = 0, lng = 0;
                string name = "", img = "", desc = "";

                if (fav.Type.ToLower() == "temple")
                {
                    var temple = await context.Temples.FindAsync(fav.ItemId);
                    if (temple == null || temple.Latitude == 0) continue;
                    lat = temple.Latitude;
                    lng = temple.Longitude;
                    name = (lang == "ar" && !string.IsNullOrEmpty(temple.NameAr)) ? temple.NameAr : temple.Name;
                    img = temple.ImageUrl ?? "";
                    desc = (lang == "ar" && !string.IsNullOrEmpty(temple.DescriptionAr)) ? temple.DescriptionAr : (temple.Description ?? "");
                }
                else if (fav.Type.ToLower() == "museum")
                {
                    var museum = await context.Museums.FindAsync(fav.ItemId);
                    if (museum == null || museum.Latitude == 0) continue;
                    lat = museum.Latitude;
                    lng = museum.Longitude;
                    name = (lang == "ar" && !string.IsNullOrEmpty(museum.NameAr)) ? museum.NameAr : museum.Name;
                    img = museum.ImageUrl ?? "";
                    desc = (lang == "ar" && !string.IsNullOrEmpty(museum.DescriptionAr)) ? museum.DescriptionAr : (museum.Description ?? "");
                }

                journeyPins.Add(new JourneyPin
                {
                    ItemId = fav.ItemId,
                    Name = name,
                    Type = fav.Type.ToLower(),
                    PinType = alreadyBooked ? "both" : "favorite",
                    Latitude = lat,
                    Longitude = lng,
                    ImageUrl = img,
                    Description = desc.Length > 100 ? desc[..100] + "..." : desc
                });
            }

            ViewBag.JourneyPins = journeyPins;
            ViewBag.JourneyCount = journeyPins.Count;

            // 🆕 نجيب رصيد اليوزر الحقيقي من البنك للعرض بس — من غير ما ننشئ حساب
            // تلقائيًا لو مش موجود (الإنشاء لسه يدوي بإيدك زي ما اتفقنا)
            ViewBag.HasBankAccount = false;
            ViewBag.BankBalance = 0m;
            ViewBag.BankContactEmail = BankContactEmail;
            ViewBag.BankConnectionError = false;

            try
            {
                var client = _httpClientFactory.CreateClient("BankService");
                var bankResponse = await client.GetAsync($"accounts/{Uri.EscapeDataString(email)}");

                if (bankResponse.IsSuccessStatusCode)
                {
                    var account = await bankResponse.Content.ReadFromJsonAsync<BankAccountResult>();
                    ViewBag.HasBankAccount = true;
                    ViewBag.BankBalance = account?.balance ?? 0m;
                }
                // 404 = مفيش حساب بنكي لسه لليوزر ده — HasBankAccount بتفضل false
                // وده اللي بيخلي الداشبورد يوريله يتواصل مع الإيميل بدل ما يكسر أو يعرض صفر بصمت
                else if (bankResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    // أي رد غريب غير 200 أو 404 (500 مثلاً) — ده مش "مفيش حساب"، ده مشكلة في البنك نفسه
                    Console.WriteLine($"[BankService] Unexpected status {bankResponse.StatusCode} for {email}");
                    ViewBag.BankConnectionError = true;
                }
            }
            catch (Exception ex)
            {
                // فرق مهم: ده معناه إحنا أصلاً معرفناش نوصل للبنك (Service مقفول، أو
                // "BankService" مش مسجل في Program.cs) — مش إن اليوزر مفيش عنده حساب.
                // نسجل الخطأ الحقيقي في اللوج عشان متضيعش وقت تفتكر إن الحساب هو المشكلة.
                Console.WriteLine($"[BankService] Connection failed for {email}: {ex.Message}");
                ViewBag.HasBankAccount = false;
                ViewBag.BankConnectionError = true;
            }

            ViewBag.ActiveTab = tab;
            return View(vm);
        }
    }

    // 🆕 بيمثل رد GET /accounts/{email} بتاع البنك — Masked (مش رقم كارت كامل)
    public class BankAccountResult
    {
        public string? user_email { get; set; }
        public string? card_holder_name { get; set; }
        public string? masked_card_number { get; set; }
        public string? expiry_date { get; set; }
        public decimal balance { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }
}