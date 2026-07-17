using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace pharaohsLegacy.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalizationService _loc;

        public BookingController(AppDbContext db, IHttpClientFactory httpClientFactory, LocalizationService loc)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _loc = loc;
        }

        // 🆕 بدل ما نكرر HttpContext.Session.GetString("Lang") ?? "en" في كل Action
        private string Lang() => HttpContext.Session.GetString("Lang") ?? "en";

        public async Task<IActionResult> MyBookings()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var bookings = await _db.Bookings
                .Where(b => b.UserEmail == userEmail && b.Status != "PendingPayment")
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var lang = Lang();

            foreach (var b in bookings)
            {
                if (b.PlaceType == "Temple")
                {
                    var temple = await _db.Temples.FindAsync(b.PlaceId);
                    b.PlaceName = (lang == "ar" && !string.IsNullOrEmpty(temple?.NameAr))
                        ? temple.NameAr
                        : (temple?.Name ?? "معبد غير معروف");
                }
                else if (b.PlaceType == "Museum")
                {
                    var museum = await _db.Museums.FindAsync(b.PlaceId);
                    b.PlaceName = (lang == "ar" && !string.IsNullOrEmpty(museum?.NameAr))
                        ? museum.NameAr
                        : (museum?.Name ?? "متحف غير معروف");
                }
            }

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                // 🆕 كانت Content("❌ NO SESSION") — رسالة ديباج مش مترجمة، بدلناها بتحويل مع رسالة واضحة لليوزر
                TempData["Error"] = _loc.Get("Booking_LoginRequired", lang);
                return RedirectToAction("Login", "User");
            }

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserEmail == userEmail);
            if (booking == null)
            {
                TempData["Error"] = _loc.Get("Booking_Cancel_NotFound", lang);
                return RedirectToAction("Dashboard", "User", new { tab = "bookings" });
            }

            if (booking.Status != "Confirmed")
            {
                TempData["Error"] = _loc.Get("Booking_Cancel_NotConfirmed", lang);
                return RedirectToAction("Dashboard", "User", new { tab = "bookings" });
            }

            if ((DateTime.Now - booking.CreatedAt).TotalHours > 48)
            {
                TempData["Error"] = _loc.Get("Booking_Cancel_Expired", lang);
                return RedirectToAction("Dashboard", "User", new { tab = "bookings" });
            }

            // 🆕 الإلغاء بقى بس يسجل CancelledAt — الفلوس بترجع فعليًا (نداء البنك)
            // بعد 24 ساعة أوتوماتيك عن طريق BookingRefundBackgroundService، مش فورًا هنا.
            // ده بيمنع تضارب زي: اليوزر يلغي، والفلوس ترجع فورًا، وبعدين الأدمن "يتراجع"
            // عن الإلغاء وهو أصلاً مسترجعش فلوسه لسه.
            var statusService = new BookingStatusService(_db, _httpClientFactory);
            var result = await statusService.ChangeStatusAsync(booking, "Cancelled");

            // 🆕 result.Message جاي من BookingStatusService — لو مش مترجم أصلاً هيفضل زي ما هو،
            // لكن على الأقل بطلنا نلزق "❌" يدويًا فوق رسالة ممكن تبقى مترجمة فعلاً
            TempData["Message"] = result.Message;

            return RedirectToAction("Dashboard", "User", new { tab = "bookings" });
        }

        public async Task<IActionResult> Create(string placeType, int placeId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");


            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot make bookings!";
                return RedirectToAction("Index", "Home");
            }

            string? placeName = null;
            string? placeImage = null;
            var lang = Lang();

            if (placeType == "Temple")
            {
                var temple = await _db.Temples.FindAsync(placeId);
                placeName = (lang == "ar" && !string.IsNullOrEmpty(temple?.NameAr)) ? temple.NameAr : temple?.Name;
                placeImage = temple?.ImageUrl;
            }
            else if (placeType == "Museum")
            {
                var museum = await _db.Museums.FindAsync(placeId);
                placeName = (lang == "ar" && !string.IsNullOrEmpty(museum?.NameAr)) ? museum.NameAr : museum?.Name;
                placeImage = museum?.ImageUrl;
            }

            if (placeName == null)
                return NotFound();

            ViewBag.PlaceType = placeType;
            ViewBag.PlaceId = placeId;
            ViewBag.PlaceName = placeName;
            ViewBag.PlaceImage = placeImage;
            ViewBag.TicketPrice = placeType == "Temple" ? 150 : 100;

            return View();
        }

        // 🆕 بينادى من الـ JS في صفحة الحجز — للتحقق من كود الخصم فورًا من غير ما يستخدمه فعليًا
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
            {
                // 🆕 مبنعرضش نص الخطأ الجاي من خدمة البنك زي ما هو (بيكون عربي دايمًا مهما كانت لغة اليوزر) —
                // بنترجم رسالتنا احنا بدله
                return Json(new { valid = false, message = _loc.Get("Booking_Coupon_Invalid", lang) });
            }

            var result = await response.Content.ReadFromJsonAsync<CouponValidateResult>();
            return Json(new { valid = true, discountPercent = result?.discount_percent ?? 0 });
        }

        // 🆕 بينادى من الـ JS لما اليوزر يدوس "ابعت كود تحقق" — بيحفظ الحجز مبدئيًا
        // (لو لسه معملوش) عشان يبقى عندنا booking.Id نربطه بيه الكود في البنك
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOtp(
            string placeType,
            int placeId,
            DateTime visitDate,
            int numberOfTickets,
            int? existingBookingId)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return Json(new { success = false, message = _loc.Get("Booking_LoginRequired", lang) });

            Booking booking;

            if (existingBookingId.HasValue)
            {
                // اليوزر داس "ابعت كود" تاني (مثلاً الكود الأول خلصت صلاحيته) — نستخدم نفس الحجز المبدئي
                booking = await _db.Bookings.FirstOrDefaultAsync(b =>
                    b.Id == existingBookingId.Value && b.UserEmail == userEmail && b.Status == "PendingPayment");

                if (booking == null)
                    return Json(new { success = false, message = _loc.Get("Booking_NotFoundRetry", lang) });

                // 🆕 باگ كان موجود: لو اليوزر غيّر التاريخ أو عدد التذاكر بعد أول "ابعت كود"،
                // الحجز المبدئي كان بيفضل بالبيانات القديمة وميتحدّثش، فيتحجز بتاريخ/سعر غلط
                // عن اللي شايفه على الشاشة. دلوقتي بنعيد التحقق ونحدّث الحجز بالبيانات الجديدة.
                if (visitDate < DateTime.Today.AddDays(1) || visitDate > DateTime.Today.AddMonths(1))
                    return Json(new { success = false, message = _loc.Get("Booking_InvalidDate", lang) });

                if (numberOfTickets < 1 || numberOfTickets > 10)
                    return Json(new { success = false, message = _loc.Get("Booking_InvalidTickets", lang) });

                int existingTicketPrice = booking.PlaceType == "Temple" ? 150 : 100;
                booking.VisitDate = visitDate;
                booking.NumberOfTickets = numberOfTickets;
                booking.TotalPrice = existingTicketPrice * numberOfTickets;
                await _db.SaveChangesAsync();
            }
            else
            {
                if (visitDate < DateTime.Today.AddDays(1) || visitDate > DateTime.Today.AddMonths(1))
                    return Json(new { success = false, message = _loc.Get("Booking_InvalidDate", lang) });

                if (numberOfTickets < 1 || numberOfTickets > 10)
                    return Json(new { success = false, message = _loc.Get("Booking_InvalidTickets", lang) });

                int ticketPrice = placeType == "Temple" ? 150 : 100;

                booking = new Booking
                {
                    UserEmail = userEmail,
                    PlaceType = placeType,
                    PlaceId = placeId,
                    VisitDate = visitDate,
                    NumberOfTickets = numberOfTickets,
                    TotalPrice = ticketPrice * numberOfTickets,
                    Status = "PendingPayment",
                    CreatedAt = DateTime.Now
                };
                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();   // عشان نجيب booking.Id
            }

            var client = _httpClientFactory.CreateClient("BankService");
            var otpResponse = await client.PostAsJsonAsync("payments/request-otp", new
            {
                user_email = userEmail,
                related_type = "Booking",
                related_id = booking.Id.ToString()
            });

            if (!otpResponse.IsSuccessStatusCode)
            {
                // 🆕 404 من البنك = مفيش حساب بنكي بالإيميل ده أصلاً، غير كده = فشل عام في الإرسال —
                // منفرقش هنا حسب نص عربي جاي من البنك، بنفرق حسب status code ونترجم إحنا
                var key = otpResponse.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "Booking_NoAccount"
                    : "Booking_Otp_SendFailed";
                return Json(new
                {
                    success = false,
                    bookingId = booking.Id,
                    message = _loc.Get(key, lang)
                });
            }

            return Json(new
            {
                success = true,
                bookingId = booking.Id,
                message = _loc.Get("Booking_Otp_Sent", lang)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            int bookingId,
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

            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot make bookings!";
                return RedirectToAction("Index", "Home");
            }

            // 🆕 الحجز اتعمل بالفعل (PendingPayment) لما اليوزر داس "ابعت كود تحقق" — بنجيبه هنا بدل ما ننشئ واحد جديد
            var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
                b.Id == bookingId && b.UserEmail == userEmail && b.Status == "PendingPayment");

            if (booking == null)
            {
                TempData["Error"] = _loc.Get("Booking_MustRequestOtpFirst", lang);
                return RedirectToAction("Index", "Home");
            }

            // 🆕 Validation أساسي لبيانات الكارت قبل ما نكلم البنك أصلاً (زي أي Checkout حقيقي)
            cardNumber = (cardNumber ?? "").Replace(" ", "");
            if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidCardNumber", lang);
                return RedirectToAction("Create", new { placeType = booking.PlaceType, placeId = booking.PlaceId });
            }
            if (string.IsNullOrWhiteSpace(cardHolderName))
            {
                TempData["Error"] = _loc.Get("Booking_CardHolderRequired", lang);
                return RedirectToAction("Create", new { placeType = booking.PlaceType, placeId = booking.PlaceId });
            }
            if (cvv == null || cvv.Length != 3 || !cvv.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidCVV", lang);
                return RedirectToAction("Create", new { placeType = booking.PlaceType, placeId = booking.PlaceId });
            }
            if (otpCode == null || otpCode.Length != 6 || !otpCode.All(char.IsDigit))
            {
                TempData["Error"] = _loc.Get("Booking_InvalidOtp", lang);
                return RedirectToAction("Create", new { placeType = booking.PlaceType, placeId = booking.PlaceId });
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
                amount = booking.TotalPrice,
                related_type = "Booking",
                related_id = booking.Id.ToString(),
                coupon_code = string.IsNullOrWhiteSpace(couponCode) ? null : couponCode,
                note = $"{booking.PlaceType} #{booking.PlaceId} — {booking.NumberOfTickets} تذكرة"
            });

            // 400/404 = بيانات الكارت أو الكود غلط أو مفيش حساب بنكي أصلاً لهذا الإيميل
            if (chargeResponse.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                chargeResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // 🆕 هنا برضه بنستبدل نص الخطأ الخام من البنك برسالة مترجمة حسب status code
                var key = chargeResponse.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "Booking_NoAccount"
                    : "Booking_PaymentDataInvalid";

                _db.Bookings.Remove(booking);   // مفيش حجز من غير دفع ناجح — لازم يدوس "ابعت كود" تاني لو عايز يعيد المحاولة
                await _db.SaveChangesAsync();

                TempData["Error"] = _loc.Get(key, lang);
                return RedirectToAction("Create", new { placeType = booking.PlaceType, placeId = booking.PlaceId });
            }

            var chargeResult = await chargeResponse.Content.ReadFromJsonAsync<ChargeResult>();

            if (chargeResult == null || !chargeResult.success)
            {
                _db.Bookings.Remove(booking);   // فشل الخصم (زي رصيد غير كافٍ) — نلغي الحجز المبدئي
                await _db.SaveChangesAsync();

                TempData["Error"] = _loc.Get("Booking_PaymentFailed", lang);
                return RedirectToAction("Create", new { placeType = booking.PlaceType, placeId = booking.PlaceId });
            }

            // 🎉 الدفع نجح فعليًا — نأكد الحجز ونسجل الدفعة
            booking.Status = "Confirmed";

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = chargeResult.final_amount,
                PaymentDate = DateTime.Now,
                PaymentMethod = "BankCard",
                Status = "Completed"
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            TempData["Message"] = chargeResult.discount_applied > 0
                ? _loc.GetFormatted("Booking_SuccessWithCoupon", lang, chargeResult.discount_applied.ToString("N2"))
                : _loc.Get("Booking_Success", lang);

            return RedirectToAction("MyBookings");
        }
    }
}
