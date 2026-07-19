using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // 🆕 عشان OnActionExecutionAsync (CartCount)
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace pharaohsLegacy.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalizationService _loc;

        public ShopController(AppDbContext db, IHttpClientFactory httpClientFactory, LocalizationService loc)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _loc = loc;
        }

        private string Lang() => HttpContext.Session.GetString("Lang") ?? "en";

        // 🆕 بيتحقق إن اليوزر مسجل دخول وليس Admin (نفس الشرط المستخدم في كل مكان في الكنترولر ده)
        private bool IsRegularUser(string? userEmail) =>
            !string.IsNullOrEmpty(userEmail) && userEmail != "guest"
            && HttpContext.Session.GetString("UserRole") != "Admin";

        // 🆕 CartCount بيتحسب تلقائيًا قبل كل Action في الكنترولر ده وبيتحط في ViewBag —
        // عشان أي View جوه Shop تقدر تعرض عدد عناصر السلة (زرار السلة فوق) من غير ما نكررها في كل Action لوحدها
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.CartCount = IsRegularUser(userEmail)
                ? await _db.CartItems.Where(c => c.UserEmail == userEmail).SumAsync(c => (int?)c.Quantity) ?? 0
                : 0;

            await next();
        }

        // ──────────────────────────────
        // عرض المنتجات
        // ──────────────────────────────
        public async Task<IActionResult> Index(int? categoryId, string? sort, bool? onSale, bool? bestSeller, bool? isNew)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            // 🆕 فلاتر الشارات والعروض — كل فلتر مستقل وممكن تجمع بينهم (AND) في نفس الوقت
            if (onSale == true)
                query = query.Where(p => p.OriginalPrice != null && p.OriginalPrice > p.Price);

            if (bestSeller == true)
                query = query.Where(p => p.IsBestSeller);

            if (isNew == true)
                query = query.Where(p => p.IsNew);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "bestselling" => query.OrderBy(p => p.Name), // الترتيب الحقيقي بيحصل تحت بعد الجلب
                _ => query.OrderByDescending(p => p.Id)       // "newest" (الافتراضي)
            };

            var products = await query.ToListAsync();

            if (sort == "bestselling")
            {
                // 🔄 بعد تعدد المنتجات في الأوردر، الكمية المباعة بقت بتتحسب من ShopOrderItems
                // (مربوطة بأوردرات الحالتها Confirmed) بدل ShopOrders مباشرة
                var soldQty = await _db.ShopOrderItems
                    .Where(oi => oi.ShopOrder!.Status == "Confirmed")
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new { g.Key, Qty = g.Sum(oi => oi.Quantity) })
                    .ToDictionaryAsync(x => x.Key, x => x.Qty);

                products = products
                    .OrderByDescending(p => soldQty.TryGetValue(p.Id, out var q) ? q : 0)
                    .ToList();
            }

            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSort = sort ?? "newest";
            ViewBag.OnSale = onSale == true; // 🆕
            ViewBag.BestSeller = bestSeller == true; // 🆕
            ViewBag.IsNewFilter = isNew == true; // 🆕

            // 🆕 المرحلة 4 — Wishlist: هات IDs المنتجات المفضّلة لليوزر الحالي (لو مسجل دخول وليس أدمن)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var isLoggedInUser = !string.IsNullOrEmpty(userEmail) && userEmail != "guest"
                && HttpContext.Session.GetString("UserRole") != "Admin";

            ViewBag.CanFavorite = isLoggedInUser;
            // 🆕 Dictionary: ProductId -> Favorite.Id (محتاجين الـ Favorite.Id عشان نستخدم Favorite/Remove الموجود أصلاً)
            ViewBag.FavoriteMap = isLoggedInUser
                ? await _db.Favorites.Where(f => f.UserEmail == userEmail && f.Type == "product")
                    .ToDictionaryAsync(f => f.ItemId, f => f.Id)
                : new Dictionary<int, int>();

            return View(products);
        }

        // صفحة تفاصيل المنتج + فورم الدفع (نفس دور Booking/Create)
        public async Task<IActionResult> Details(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot make purchases!";
                return RedirectToAction("Index", "Home");
            }

            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var lang = Lang();
            ViewBag.ProductName = (lang == "ar" && !string.IsNullOrEmpty(product.NameAr)) ? product.NameAr : product.Name;
            ViewBag.ProductImage = product.ImageUrl;
            ViewBag.UnitPrice = product.Price;
            ViewBag.MaxQuantity = Math.Max(0, Math.Min(10, product.StockQuantity));
            ViewBag.ProductId = product.Id;

            // 🆕 المرحلة 4 — Wishlist: الـ Details أصلاً بترفض الـ Guest والـ Admin فوق، فأي حد وصل هنا مضمون إنه يوزر عادي
            var existingFav = await _db.Favorites.FirstOrDefaultAsync(f =>
                f.UserEmail == userEmail && f.Type == "product" && f.ItemId == id);
            ViewBag.IsFav = existingFav != null;
            ViewBag.FavoriteId = existingFav?.Id;

            // 🆕 Reviews — نفس الباترن المستخدم في باقي الصفحات (Type = "product")
            var reviews = await _db.Reviews
                .Where(r => r.Type == "product" && r.ItemId == id)
                .ToListAsync();
            ViewBag.Reviews = reviews;
            ViewBag.UserReviewed = reviews.Any(r => r.UserEmail == userEmail);

            // 🆕 منتجات مشابهة — الأساس بقى نفس الـ Category (بعد ما خلصت)، ولو مش كفاية
            // (أو المنتج نفسه من غير Category) بنكمل بأقرب سعر عشان السكشن ميفضلش فاضي
            const int similarCount = 4;
            var similarProducts = new List<Product>();

            if (product.CategoryId.HasValue)
            {
                similarProducts = await _db.Products
                    .Where(p => p.Id != id && p.CategoryId == product.CategoryId)
                    .OrderByDescending(p => p.Id)
                    .Take(similarCount)
                    .ToListAsync();
            }

            if (similarProducts.Count < similarCount)
            {
                var excludeIds = similarProducts.Select(p => p.Id).Append(id).ToList();
                var fallback = await _db.Products
                    .Where(p => !excludeIds.Contains(p.Id))
                    .OrderBy(p => Math.Abs(p.Price - product.Price))
                    .Take(similarCount - similarProducts.Count)
                    .ToListAsync();
                similarProducts.AddRange(fallback);
            }

            ViewBag.SimilarProducts = similarProducts;

            return View(product);
        }

        // ──────────────────────────────
        // 🛒 السلة (Cart)
        // ──────────────────────────────

        // 🆕 Endpoint خفيف بيتنادى من _Layout.cshtml في أي صفحة بالموقع عشان يعرض عدد السلة في الـ nav
        [HttpGet]
        public async Task<IActionResult> CartCount()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return Json(new { count = 0 });

            var count = await _db.CartItems.Where(c => c.UserEmail == userEmail).SumAsync(c => (int?)c.Quantity) ?? 0;
            return Json(new { count });
        }

        // 🆕 إضافة منتج للسلة (أو زيادة الكمية لو موجود فيها أصلًا) — بيتنادى بـ AJAX من Index/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return Json(new { success = false, message = _loc.Get("Booking_LoginRequired", lang) });

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return Json(new { success = false, message = _loc.Get("Shop_ProductNotFound", lang) });

            if (quantity < 1)
                return Json(new { success = false, message = _loc.Get("Shop_InvalidQuantity", lang) });

            var existing = await _db.CartItems.FirstOrDefaultAsync(c =>
                c.UserEmail == userEmail && c.ProductId == productId);

            var newQuantity = (existing?.Quantity ?? 0) + quantity;
            newQuantity = Math.Min(newQuantity, 10); // نفس السقف المستخدم في RequestOtp

            if (newQuantity > product.StockQuantity)
                return Json(new { success = false, message = _loc.Get("Shop_OutOfStock", lang) });

            if (existing != null)
            {
                existing.Quantity = newQuantity;
            }
            else
            {
                _db.CartItems.Add(new CartItem
                {
                    UserEmail = userEmail,
                    ProductId = productId,
                    Quantity = newQuantity,
                    AddedAt = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();

            var cartCount = await _db.CartItems.Where(c => c.UserEmail == userEmail).SumAsync(c => (int?)c.Quantity) ?? 0;
            return Json(new { success = true, cartCount, message = _loc.Get("Shop_AddedToCart", lang) });
        }

        // 🆕 عرض صفحة السلة
        public async Task<IActionResult> Cart()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return RedirectToAction("Login", "User");

            var items = await _db.CartItems.Where(c => c.UserEmail == userEmail).OrderByDescending(c => c.AddedAt).ToListAsync();
            var lang = Lang();

            // بنجيب بيانات المنتجات مرة واحدة بدل ما نستعلم جوه الـ Loop لكل عنصر
            var productIds = items.Select(c => c.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            // 🆕 لو منتج اتحذف من المتجر وهو لسه في السلة، بنشيله تلقائيًا هنا بدل ما يبوّظ العرض
            var staleItems = items.Where(c => !products.ContainsKey(c.ProductId)).ToList();
            if (staleItems.Any())
            {
                _db.CartItems.RemoveRange(staleItems);
                await _db.SaveChangesAsync();
                items = items.Except(staleItems).ToList();
            }

            foreach (var c in items)
            {
                var p = products[c.ProductId];
                c.ProductName = (lang == "ar" && !string.IsNullOrEmpty(p.NameAr)) ? p.NameAr : p.Name;
                c.ProductImage = p.ImageUrl;
                c.ProductPrice = p.Price;
                c.ProductOriginalPrice = p.OriginalPrice;
                c.ProductStock = p.StockQuantity;

                // لو الكمية في السلة بقت أكبر من المخزون المتاح (اتباع بعد ما اليوزر ضاف السلة) نقصّها تلقائيًا
                if (c.Quantity > c.ProductStock)
                {
                    c.Quantity = Math.Max(0, c.ProductStock);
                }
            }

            // نمسح أي عنصر كميته بقت صفر بعد التصحيح فوق
            var zeroItems = items.Where(c => c.Quantity <= 0).ToList();
            if (zeroItems.Any())
            {
                _db.CartItems.RemoveRange(zeroItems);
                await _db.SaveChangesAsync();
                items = items.Except(zeroItems).ToList();
            }

            ViewBag.Subtotal = items.Sum(c => c.ProductPrice * c.Quantity);
            return View(items);
        }

        // 🆕 تحديث كمية عنصر في السلة (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return Json(new { success = false, message = _loc.Get("Booking_LoginRequired", lang) });

            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserEmail == userEmail);
            if (item == null)
                return Json(new { success = false, message = _loc.Get("Booking_NotFoundRetry", lang) });

            var product = await _db.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
                return Json(new { success = false, message = _loc.Get("Shop_ProductNotFound", lang), removed = true });
            }

            if (quantity < 1)
                return Json(new { success = false, message = _loc.Get("Shop_InvalidQuantity", lang) });

            quantity = Math.Min(quantity, 10);
            if (quantity > product.StockQuantity)
                return Json(new { success = false, message = _loc.Get("Shop_OutOfStock", lang) });

            item.Quantity = quantity;
            await _db.SaveChangesAsync();

            var subtotal = product.Price * quantity;
            var cartCount = await _db.CartItems.Where(c => c.UserEmail == userEmail).SumAsync(c => (int?)c.Quantity) ?? 0;
            return Json(new { success = true, quantity, itemSubtotal = subtotal, cartCount });
        }

        // 🆕 حذف عنصر من السلة (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return Json(new { success = false });

            var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserEmail == userEmail);
            if (item == null)
                return Json(new { success = false });

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();

            var cartCount = await _db.CartItems.Where(c => c.UserEmail == userEmail).SumAsync(c => (int?)c.Quantity) ?? 0;
            return Json(new { success = true, cartCount });
        }

        // 🆕 نفس ValidateCoupon بتاعة الحجز بالظبط — كود واحد شغال في المكانين
        [HttpGet]
        public async Task<IActionResult> ValidateCoupon(string code)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return Json(new { valid = false, message = _loc.Get("Booking_LoginRequired", lang) });

            if (string.IsNullOrWhiteSpace(code))
                return Json(new { valid = false, message = _loc.Get("Booking_Coupon_EnterCode", lang) });

            var client = _httpClientFactory.CreateClient("BankService");
            var response = await client.PostAsJsonAsync("coupons/validate", new
            {
                code = code,
                user_email = userEmail
            });

            if (!response.IsSuccessStatusCode)
                return Json(new { valid = false, message = _loc.Get("Booking_Coupon_Invalid", lang) });

            var result = await response.Content.ReadFromJsonAsync<CouponValidateResult>();
            return Json(new { valid = true, discountPercent = result?.discount_percent ?? 0 });
        }

        // ──────────────────────────────
        // 🧾 Checkout — عنوان + تليفون + محافظة + دفع
        // ──────────────────────────────

        // 🆕 صفحة الدفع النهائية — بتاخد كل عناصر السلة الحالية وتعرضها + فورم العنوان والدفع
        public async Task<IActionResult> Checkout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return RedirectToAction("Login", "User");

            var cartItems = await _db.CartItems.Where(c => c.UserEmail == userEmail).ToListAsync();
            if (!cartItems.Any())
            {
                TempData["Error"] = _loc.Get("Shop_Cart_Empty", Lang());
                return RedirectToAction("Cart");
            }

            var lang = Lang();
            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            // منتج اتحذف من المتجر وهو لسه في السلة — بنشيله ومنكملش بيه هنا برضو
            cartItems = cartItems.Where(c => products.ContainsKey(c.ProductId)).ToList();
            if (!cartItems.Any())
            {
                TempData["Error"] = _loc.Get("Shop_Cart_Empty", lang);
                return RedirectToAction("Cart");
            }

            foreach (var c in cartItems)
            {
                var p = products[c.ProductId];
                c.ProductName = (lang == "ar" && !string.IsNullOrEmpty(p.NameAr)) ? p.NameAr : p.Name;
                c.ProductImage = p.ImageUrl;
                c.ProductPrice = p.Price;
                c.ProductStock = p.StockQuantity;
            }

            ViewBag.Subtotal = cartItems.Sum(c => c.ProductPrice * c.Quantity);
            ViewBag.Governorates = Governorates.All;
            return View(cartItems);
        }

        // 🆕 بينشئ/يحدّث ShopOrder (PendingPayment) من كل عناصر السلة الحالية + بيانات التوصيل، ويطلب OTP من البنك
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOtp(string phoneNumber, string address, string governorate, int? existingOrderId)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!IsRegularUser(userEmail))
                return Json(new { success = false, message = _loc.Get("Booking_LoginRequired", lang) });

            phoneNumber = (phoneNumber ?? "").Trim();
            if (phoneNumber.Length != 11 || !phoneNumber.All(char.IsDigit) || !phoneNumber.StartsWith("01"))
                return Json(new { success = false, message = _loc.Get("Shop_InvalidPhone", lang) });

            if (string.IsNullOrWhiteSpace(address))
                return Json(new { success = false, message = _loc.Get("Shop_AddressRequired", lang) });

            if (!Governorates.IsValid(governorate))
                return Json(new { success = false, message = _loc.Get("Shop_InvalidGovernorate", lang) });

            var cartItems = await _db.CartItems.Where(c => c.UserEmail == userEmail).ToListAsync();
            if (!cartItems.Any())
                return Json(new { success = false, message = _loc.Get("Shop_Cart_Empty", lang) });

            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            // تحقق من المخزون لكل عنصر في السلة قبل ما ننشئ/نحدّث الأوردر
            foreach (var c in cartItems)
            {
                if (!products.TryGetValue(c.ProductId, out var p) || c.Quantity > p.StockQuantity)
                    return Json(new { success = false, message = _loc.Get("Shop_OutOfStock", lang) });
            }

            var shippingFee = Governorates.GetFee(governorate);
            var subtotal = cartItems.Sum(c => products[c.ProductId].Price * c.Quantity);
            var total = subtotal + shippingFee;

            ShopOrder order;

            if (existingOrderId.HasValue)
            {
                order = await _db.ShopOrders.Include(o => o.Items).FirstOrDefaultAsync(o =>
                    o.Id == existingOrderId.Value && o.UserEmail == userEmail && o.Status == "PendingPayment");

                if (order == null)
                    return Json(new { success = false, message = _loc.Get("Booking_NotFoundRetry", lang) });

                // بنمسح عناصر الأوردر القديمة ونبني بيانات جديدة من السلة الحالية —
                // عشان لو اليوزر عدّل الكميات في السلة قبل ما يبعت OTP تاني
                _db.ShopOrderItems.RemoveRange(order.Items);
                order.Items = cartItems.Select(c => new ShopOrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = products[c.ProductId].Price
                }).ToList();
            }
            else
            {
                order = new ShopOrder
                {
                    UserEmail = userEmail,
                    Status = "PendingPayment",
                    CreatedAt = DateTime.Now,
                    Items = cartItems.Select(c => new ShopOrderItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        UnitPrice = products[c.ProductId].Price
                    }).ToList()
                };
                _db.ShopOrders.Add(order);
            }

            order.PhoneNumber = phoneNumber;
            order.Address = address.Trim();
            order.Governorate = governorate;
            order.ShippingFee = shippingFee;
            order.TotalPrice = total;

            await _db.SaveChangesAsync();

            var client = _httpClientFactory.CreateClient("BankService");
            var otpResponse = await client.PostAsJsonAsync("payments/request-otp", new
            {
                user_email = userEmail,
                related_type = "ShopOrder",
                related_id = order.Id.ToString()
            });

            if (!otpResponse.IsSuccessStatusCode)
            {
                var key = otpResponse.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "Booking_NoAccount"
                    : "Booking_Otp_SendFailed";
                return Json(new { success = false, orderId = order.Id, message = _loc.Get(key, lang) });
            }

            return Json(new { success = true, orderId = order.Id, message = _loc.Get("Booking_Otp_Sent", lang) });
        }

        // 🆕 مطابق لمنطق Booking/Confirm — بس بيتحقق من المخزون ويخصمه لكل عنصر في الأوردر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            int orderId,
            string cardNumber,
            string cardHolderName,
            string expiryDate,
            string cvv,
            string otpCode,
            string? couponCode)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var order = await _db.ShopOrders.Include(o => o.Items).FirstOrDefaultAsync(o =>
                o.Id == orderId && o.UserEmail == userEmail && o.Status == "PendingPayment");

            if (order == null)
            {
                TempData["Error"] = _loc.Get("Booking_MustRequestOtpFirst", lang);
                return RedirectToAction("Cart");
            }

            cardNumber = (cardNumber ?? "").Replace(" ", "");
            if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidCardNumber", lang);
                return RedirectToAction("Checkout");
            }
            if (string.IsNullOrWhiteSpace(cardHolderName))
            {
                TempData["Error"] = _loc.Get("Booking_CardHolderRequired", lang);
                return RedirectToAction("Checkout");
            }
            if (cvv == null || cvv.Length != 3 || !cvv.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidCVV", lang);
                return RedirectToAction("Checkout");
            }
            if (otpCode == null || otpCode.Length != 6 || !otpCode.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidOtp", lang);
                return RedirectToAction("Checkout");
            }

            // 🆕 نتأكد من المخزون تاني هنا (آخر لحظة قبل الدفع) لكل عنصر في الأوردر —
            // عشان لو حد تاني اشترى نفس المنتج في نفس الوقت وخلّص الكمية، منخصمش فلوس على منتج خلص من المخزن
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            foreach (var item in order.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var p) || item.Quantity > p.StockQuantity)
                {
                    _db.ShopOrderItems.RemoveRange(order.Items);
                    _db.ShopOrders.Remove(order);
                    await _db.SaveChangesAsync();
                    TempData["Error"] = _loc.Get("Shop_OutOfStock", lang);
                    return RedirectToAction("Cart");
                }
            }

            var client = _httpClientFactory.CreateClient("BankService");
            var chargeResponse = await client.PostAsJsonAsync("payments/charge", new
            {
                user_email = userEmail,
                card_number = cardNumber,
                card_holder_name = cardHolderName,
                expiry_date = expiryDate,
                cvv = cvv,
                otp_code = otpCode,
                amount = order.TotalPrice,
                related_type = "ShopOrder",
                related_id = order.Id.ToString(),
                coupon_code = string.IsNullOrWhiteSpace(couponCode) ? null : couponCode,
                note = $"Shop Order #{order.Id} — {order.Items.Count} item(s)"
            });

            if (chargeResponse.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                chargeResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var error = await chargeResponse.Content.ReadFromJsonAsync<BankErrorResult>();

                string key = error?.detail switch
                {
                    "كود الخصم غير موجود لهذا المستخدم" => "Booking_Coupon_Invalid",
                    "الكود ده اتستخدم قبل كده" => "Booking_Coupon_Invalid",
                    "الكود ده منتهي الصلاحية" => "Booking_Coupon_Invalid",
                    "كود التحقق غير صحيح أو منتهي الصلاحية" => "Booking_InvalidOtp",
                    _ => chargeResponse.StatusCode == System.Net.HttpStatusCode.NotFound
                        ? "Booking_NoAccount"
                        : "Booking_PaymentDataInvalid"
                };

                _db.ShopOrderItems.RemoveRange(order.Items);
                _db.ShopOrders.Remove(order);
                await _db.SaveChangesAsync();

                TempData["Error"] = _loc.Get(key, lang);
                return RedirectToAction("Checkout");
            }

            var chargeResult = await chargeResponse.Content.ReadFromJsonAsync<ChargeResult>();

            if (chargeResult == null || !chargeResult.success)
            {
                _db.ShopOrderItems.RemoveRange(order.Items);
                _db.ShopOrders.Remove(order);
                await _db.SaveChangesAsync();

                TempData["Error"] = _loc.Get("Booking_PaymentFailed", lang);
                return RedirectToAction("Checkout");
            }

            // 🎉 الدفع نجح — نأكد الأوردر، نخصم المخزون لكل عنصر، نفضّي السلة، ونسجل الدفعة
            order.Status = "Confirmed";
            order.ConfirmedAt = DateTime.Now; // 🆕 أساس حساب الـ 48 ساعة قبل الشحن التلقائي (ShopOrderShippingBackgroundService)
            foreach (var item in order.Items)
            {
                products[item.ProductId].StockQuantity -= item.Quantity;
            }

            // السلة كلها اتحوّلت لأوردر واحد، فبنفضّيها بالكامل بعد نجاح الدفع
            var cartItems = await _db.CartItems.Where(c => c.UserEmail == userEmail).ToListAsync();
            _db.CartItems.RemoveRange(cartItems);

            var payment = new ShopPayment
            {
                ShopOrderId = order.Id,
                Amount = chargeResult.final_amount,
                PaymentDate = DateTime.Now,
                PaymentMethod = "BankCard",
                Status = "Completed"
            };
            _db.ShopPayments.Add(payment);
            await _db.SaveChangesAsync();

            TempData["Message"] = chargeResult.discount_applied > 0
                ? _loc.GetFormatted("Booking_SuccessWithCoupon", lang, chargeResult.discount_applied.ToString("N2"))
                : _loc.Get("Shop_PurchaseSuccess", lang);

            return RedirectToAction("MyOrders");
        }

        // 🆕 نفس منطق BookingController.Cancel بالظبط، بس بيتحقق من ShippingStatus بدل VisitDate.
        // مسموح تلغي الأوردر طالما لسه Processing (يعني قبل ما يتشحن). الفلوس بترجع تلقائي
        // بعد 24 ساعة عن طريق ShopOrderRefundBackgroundService، مش فورًا هنا.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
            {
                TempData["Error"] = _loc.Get("Booking_LoginRequired", lang);
                return RedirectToAction("Login", "User");
            }

            var order = await _db.ShopOrders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserEmail == userEmail);

            if (order == null)
            {
                TempData["Error"] = _loc.Get("Shop_Cancel_NotFound", lang);
                return RedirectToAction("MyOrders");
            }

            if (order.Status != "Confirmed")
            {
                TempData["Error"] = _loc.Get("Shop_Cancel_NotConfirmed", lang);
                return RedirectToAction("MyOrders");
            }

            // 🆕 القاعدة اللي طلبتها: طالما الطلب لسه ما خرجش للشحن، يقدر يتلغي
            if (order.ShippingStatus != "Processing")
            {
                TempData["Error"] = _loc.Get("Shop_Cancel_AlreadyShipped", lang);
                return RedirectToAction("MyOrders");
            }

            var statusService = new ShopOrderStatusService(_db, _httpClientFactory);
            var result = await statusService.ChangeStatusAsync(order, "Cancelled");

            TempData["Message"] = result.Message;
            return RedirectToAction("MyOrders");
        }

        public async Task<IActionResult> MyOrders()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var orders = await _db.ShopOrders
                .Include(o => o.Items)
                .Where(o => o.UserEmail == userEmail && o.Status != "PendingPayment")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var lang = Lang();
            var productIds = orders.SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            foreach (var o in orders)
            {
                foreach (var item in o.Items)
                {
                    if (products.TryGetValue(item.ProductId, out var p))
                    {
                        item.ProductName = (lang == "ar" && !string.IsNullOrEmpty(p.NameAr)) ? p.NameAr : p.Name;
                        item.ProductImage = p.ImageUrl;
                    }
                }
            }

            return View(orders);
        }
    }
}
