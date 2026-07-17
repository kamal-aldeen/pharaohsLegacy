using Microsoft.AspNetCore.Mvc;
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
                var soldQty = await _db.ShopOrders
                    .Where(o => o.Status == "Confirmed")
                    .GroupBy(o => o.ProductId)
                    .Select(g => new { g.Key, Qty = g.Sum(o => o.Quantity) })
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

        // 🆕 بينشئ/يحدّث ShopOrder كـ PendingPayment ويطلب OTP من البنك — مطابق تمامًا لـ Booking/RequestOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOtp(int productId, int quantity, int? existingOrderId)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return Json(new { success = false, message = _loc.Get("Booking_LoginRequired", lang) });

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return Json(new { success = false, message = _loc.Get("Shop_ProductNotFound", lang) });

            ShopOrder order;

            if (existingOrderId.HasValue)
            {
                order = await _db.ShopOrders.FirstOrDefaultAsync(o =>
                    o.Id == existingOrderId.Value && o.UserEmail == userEmail && o.Status == "PendingPayment");

                if (order == null)
                    return Json(new { success = false, message = _loc.Get("Booking_NotFoundRetry", lang) });

                if (quantity < 1 || quantity > 10)
                    return Json(new { success = false, message = _loc.Get("Shop_InvalidQuantity", lang) });

                // 🆕 نفس فلسفة تحديث بيانات الحجز عند إعادة طلب الكود — هنا كمان بنتأكد
                // إن الكمية المطلوبة لسه متاحة في المخزون وقت إعادة الطلب، مش وقت أول مرة بس
                if (quantity > product.StockQuantity)
                    return Json(new { success = false, message = _loc.Get("Shop_OutOfStock", lang) });

                order.Quantity = quantity;
                order.TotalPrice = product.Price * quantity;
                await _db.SaveChangesAsync();
            }
            else
            {
                if (quantity < 1 || quantity > 10)
                    return Json(new { success = false, message = _loc.Get("Shop_InvalidQuantity", lang) });

                if (quantity > product.StockQuantity)
                    return Json(new { success = false, message = _loc.Get("Shop_OutOfStock", lang) });

                order = new ShopOrder
                {
                    UserEmail = userEmail,
                    ProductId = productId,
                    Quantity = quantity,
                    TotalPrice = product.Price * quantity,
                    Status = "PendingPayment",
                    CreatedAt = DateTime.Now
                };
                _db.ShopOrders.Add(order);
                await _db.SaveChangesAsync();
            }

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

        // 🆕 مطابق تمامًا لـ Booking/Confirm — نفس الـ Validation ونفس منطق قراءة أخطاء البنك،
        // بالإضافة لخصم المخزون (Stock) بعد نجاح الدفع فعليًا (مش قبل كده)
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

            var order = await _db.ShopOrders.FirstOrDefaultAsync(o =>
                o.Id == orderId && o.UserEmail == userEmail && o.Status == "PendingPayment");

            if (order == null)
            {
                TempData["Error"] = _loc.Get("Booking_MustRequestOtpFirst", lang);
                return RedirectToAction("Index");
            }

            cardNumber = (cardNumber ?? "").Replace(" ", "");
            if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidCardNumber", lang);
                return RedirectToAction("Details", new { id = order.ProductId });
            }
            if (string.IsNullOrWhiteSpace(cardHolderName))
            {
                TempData["Error"] = _loc.Get("Booking_CardHolderRequired", lang);
                return RedirectToAction("Details", new { id = order.ProductId });
            }
            if (cvv == null || cvv.Length != 3 || !cvv.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidCVV", lang);
                return RedirectToAction("Details", new { id = order.ProductId });
            }
            if (otpCode == null || otpCode.Length != 6 || !otpCode.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidOtp", lang);
                return RedirectToAction("Details", new { id = order.ProductId });
            }

            // 🆕 نتأكد من المخزون تاني هنا (آخر لحظة قبل الدفع) — عشان لو حد تاني اشترى
            // نفس المنتج في نفس الوقت وخلّص الكمية، منخصمش فلوس على منتج خلص من المخزن
            var product = await _db.Products.FindAsync(order.ProductId);
            if (product == null || order.Quantity > product.StockQuantity)
            {
                _db.ShopOrders.Remove(order);
                await _db.SaveChangesAsync();
                TempData["Error"] = _loc.Get("Shop_OutOfStock", lang);
                return RedirectToAction("Index");
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
                note = $"Shop #{product.Id} — {product.Name} x{order.Quantity}"
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

                _db.ShopOrders.Remove(order);
                await _db.SaveChangesAsync();

                TempData["Error"] = _loc.Get(key, lang);
                return RedirectToAction("Details", new { id = order.ProductId });
            }

            var chargeResult = await chargeResponse.Content.ReadFromJsonAsync<ChargeResult>();

            if (chargeResult == null || !chargeResult.success)
            {
                _db.ShopOrders.Remove(order);
                await _db.SaveChangesAsync();

                TempData["Error"] = _loc.Get("Booking_PaymentFailed", lang);
                return RedirectToAction("Details", new { id = order.ProductId });
            }

            // 🎉 الدفع نجح — نأكد الأوردر، نخصم المخزون، ونسجل الدفعة
            order.Status = "Confirmed";
            product.StockQuantity -= order.Quantity;

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

        public async Task<IActionResult> MyOrders()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var orders = await _db.ShopOrders
                .Where(o => o.UserEmail == userEmail && o.Status != "PendingPayment")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var lang = Lang();
            foreach (var o in orders)
            {
                var product = await _db.Products.FindAsync(o.ProductId);
                o.ProductName = (lang == "ar" && !string.IsNullOrEmpty(product?.NameAr)) ? product.NameAr : (product?.Name ?? "");
                o.ProductImage = product?.ImageUrl ?? "";
            }

            return View(orders);
        }
    }
}
