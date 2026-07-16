# Pharaohs Legacy Bank — Fake Banking Microservice (Python)

نظام بنكي وهمي مستقل، بيحاكي بنك حقيقي (حساب + كارت + رصيد + عمليات + أكواد خصم).
مبني بـ **FastAPI**، وبيتكلم مع مشروع **ASP.NET Core** الرئيسي عن طريق REST API عادي.

---

## 1) التشغيل

```bash
cd bank_service
pip install -r requirements.txt
uvicorn main:app --reload --port 8001
```

بعد التشغيل:
- **Swagger Docs** (لتجربة كل Endpoint يدويًا): http://127.0.0.1:8001/docs
- **Dashboard** (لمتابعة كل حاجة لحظيًا في المتصفح): http://127.0.0.1:8001/dashboard

الداتابيز بتاعته `bank.db` (SQLite) بتتعمل أوتوماتيك أول ما تشغّل السيرفر، ومستقلة تمامًا عن الـ SQL Server بتاع المشروع الأساسي.

---

## 2) الـ Endpoints المتاحة

| Method | Endpoint | الوظيفة |
|---|---|---|
| POST | `/accounts/create` | إنشاء حساب بنكي جديد **(يدوي من الـ Swagger بس — مش مربوط بالـ Register في الموقع)** |
| GET | `/accounts/{email}` | عرض بيانات الحساب + الرصيد (رقم الكارت بيرجع Masked دايمًا، أبدًا كامل) |
| GET | `/accounts/{email}/transactions` | سجل كل العمليات |
| POST | `/accounts/{email}/topup` | شحن رصيد |
| **POST** | **`/payments/charge`** | **الدفع الفعلي (حجز/شراء) — بيتحقق من بيانات الكارت كاملة قبل الخصم** |
| **POST** | **`/payments/refund`** | **استرجاع فوري وكامل عند إلغاء الحجز** |
| POST | `/coupons/create` | إنشاء كود خصم (بيتنادى بعد الكويز) |
| GET | `/coupons/{email}` | كل أكواد اليوزر |
| POST | `/coupons/validate` | التأكد إن الكود شغال من غير ما يستخدمه |

---

## 3) 🔑 فلسفة النظام — مهم جدًا

**الموقع (ASP.NET) مفتوح لأي حد يعمل Register عادي — ده منفصل تمامًا عن البنك.**

البنك عنده عملاؤه الخاصين بس (بتتعمل يدويًا دلوقتي من الـ Swagger `/docs` وقت التست). يعني:
- أي حد يقدر يعمل حساب في الموقع ويتصفح كل حاجة
- **لكن** لما يجي يحجز، لازم يدخل بيانات كارت حقيقية (رقم + اسم + تاريخ + CVV) **مطابقة فعليًا** لحساب موجود في البنك
- لو البيانات مش مطابقة (حتى لو غلطة بسيطة) → رفض برسالة عامة واحدة: **"بيانات الدفع غير صحيحة"** (نفس فلسفة أي Payment Gateway حقيقي — مفيش تفاصيل عن أي حقل بالظبط غلط، عشان الأمان)
- الإيميل المستخدم في الحجز **لازم يطابق** إيميل صاحب الحساب في البنك (حماية إضافية — مينفعش حد يستخدم كارت حد تاني حتى لو عرف رقمه)

---

## 4) إزاي تكلمه من ASP.NET Core

هتحتاج تضيف `HttpClient` بسيط في المشروع، مثلاً في `Program.cs`:

```csharp
builder.Services.AddHttpClient("BankService", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8001/");
});
```

### مثال: صفحة تأكيد الحجز — الدفع الفعلي بالكارت

```csharp
public class BookingPaymentViewModel
{
    [Required(ErrorMessage = "رقم الكارت مطلوب")]
    [CreditCard(ErrorMessage = "رقم الكارت غير صحيح")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "رقم الكارت لازم يكون 16 رقم")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "اسم حامل الكارت مطلوب")]
    public string CardHolderName { get; set; }

    [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "الصيغة MM/YY")]
    public string ExpiryDate { get; set; }

    [Required(ErrorMessage = "CVV مطلوب")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV لازم يكون 3 أرقام")]
    public string Cvv { get; set; }

    public string? CouponCode { get; set; }
}
```

```csharp
[HttpPost]
public async Task<IActionResult> ConfirmPayment(BookingPaymentViewModel model, int bookingId)
{
    if (!ModelState.IsValid) return View(model);

    var booking = await _context.Bookings.FindAsync(bookingId);
    var client = _httpClientFactory.CreateClient("BankService");

    var response = await client.PostAsJsonAsync("payments/charge", new
    {
        user_email = User.Identity.Name,   // لازم يطابق إيميل صاحب الحساب في البنك
        card_number = model.CardNumber,
        card_holder_name = model.CardHolderName,
        expiry_date = model.ExpiryDate,
        cvv = model.Cvv,
        amount = booking.TotalPrice,
        related_type = "Booking",
        related_id = booking.Id.ToString(),
        coupon_code = model.CouponCode
    });

    if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
    {
        var error = await response.Content.ReadFromJsonAsync<ErrorResult>();
        ModelState.AddModelError("", error.detail); // "بيانات الدفع غير صحيحة"
        return View(model);
    }

    var result = await response.Content.ReadFromJsonAsync<ChargeResult>();
    if (!result.success)
    {
        ModelState.AddModelError("", result.message); // "رصيد غير كافٍ"
        return View(model);
    }

    booking.Status = "Confirmed";
    await _context.SaveChangesAsync();
    return RedirectToAction("Success");
}
```

### مثال: إلغاء الحجز — استرجاع الفلوس (ضمن نفس قاعدة الـ 48 ساعة الموجودة)

```csharp
[HttpPost]
public async Task<IActionResult> CancelBooking(int bookingId)
{
    var booking = await _context.Bookings.FindAsync(bookingId);

    // نفس قاعدة الإلغاء الموجودة بالفعل في المشروع (48 ساعة)
    if ((booking.VisitDate - DateTime.Now).TotalHours < 48)
    {
        ModelState.AddModelError("", "لا يمكن الإلغاء قبل أقل من 48 ساعة من الموعد");
        return RedirectToAction("MyBookings");
    }

    var client = _httpClientFactory.CreateClient("BankService");
    var response = await client.PostAsJsonAsync("payments/refund", new
    {
        user_email = User.Identity.Name,
        related_type = "Booking",
        related_id = booking.Id.ToString()
        // مفيش amount — البنك بيرجع نفس مبلغ العملية الأصلية أوتوماتيك
    });

    var result = await response.Content.ReadFromJsonAsync<RefundResult>();
    booking.Status = "Cancelled";
    await _context.SaveChangesAsync();

    TempData["Message"] = $"تم استرجاع {result.refunded_amount} EGP إلى رصيدك";
    return RedirectToAction("MyBookings");
}
```

---

## 5) ملاحظات مهمة

- الـ Service ده لازم يكون شغال (uvicorn) في نفس وقت تشغيل الموقع — لو قفلته، أي عملية حجز/دفع هترجع Connection Error.
- الكوبونات: **استخدام واحد + مدة صلاحية 10 أيام** — تقدر تغيرها من `valid_days` في الـ request.
- الأرقام والكروت كلها وهمية 100% (مفيش أي بوابة دفع حقيقية أو بيانات حقيقية اتخزنت).
- **رقم الكارت الكامل ماينفعش يترجع من أي Endpoint أبدًا** — الـ API بترجع بس الشكل الـ Masked (`**** **** **** 1234`). لو محتاج تجيب رقم الكارت الكامل وقت التست، استخدم أداة زي DB Browser for SQLite على ملف `bank.db` مباشرة.
