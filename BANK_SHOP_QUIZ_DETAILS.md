# 🏦🛍️🧠 Bank + Shop + Quiz Ecosystem — التفاصيل الكاملة (✅ مكتمل بالكامل)

> **ملف منفصل** عشان الملف الرئيسي (`progress_update.md`) كان بقى كبير جدًا. النقطة دي
> (Bank + Shop + Quiz) كانت أكبر جزء فيه، فاتنقلت هنا بالكامل مع كل تفاصيلها،
> والملف الرئيسي فيه بس ملخص قصير بيشاور على الملف ده.
>
> لو في شات جديد وعايز تكمل شغل مرتبط بالبنك/المتجر/الكويز، ابعت الملف ده مع
> `progress_update.md`.

---

## 🏦 Bank + Shop + Quiz Ecosystem — الخطة الكاملة (✅ مكتمل بالكامل — Bank + Shop + Quiz التلاتة خلصوا)

> ده الـ Feature الكبير اللي شغالين عليه دلوقتي بدل ما نكمل الـ Roadmap بترتيبه العادي.
> القسم ده موجود عشان في أي شات جديد، تبعتلي الملف وأنا أبقى فاهم إحنا واقفين فين بالظبط ومحتاجين نعمل إيه بعد كده.

### 🎯 الفكرة الأساسية
مش بس Quiz عادي — دخلنا في بناء **نظام اقتصادي متكامل جوه المشروع**، بيربط 3 حاجات ببعض:

```
        ┌───────────────────────┐
        │   🏦 Fake Bank (Python) │  ← الأساس اللي كل حاجة بتتبني عليه
        └───────────┬───────────┘
                     │ (خصم / شحن)
        ┌────────────┼─────────────┐
        │                          │
┌───────▼────────┐       ┌─────────▼────────┐
│ 🎫 Booking      │       │ 🛍️ Shop (متجر)     │
│ (معابد/متاحف)   │       │ (تذكارات/منتجات)   │
└───────▲────────┘       └─────────▲────────┘
        │                          │
        └───────────┬──────────────┘
                     │ (كود خصم)
        ┌────────────▼────────────┐
        │  🧠 Quiz (صعوبة متدرجة)   │
        │  Score كويس → Coupon Code │
        └──────────────────────────┘
```

- **البنك (Bank)** هو الأساس: كل يوزر ليه حساب بنكي وهمي (كارت + رصيد)، وأي عملية دفع (حجز أو شراء من المتجر) بتخصم فعليًا من رصيده.
- **الكويز (Quiz)**: مش أسئلة عادية — بيكبر مع الداتابيز (كل ما الفراعنة/الآلهة/الأسر تزيد، الأسئلة تزيد وتتنوع صعوبة). اليوزر لو جاب Score كويس بيطلعله **Coupon Code**.
- **الكوبون (Coupon)**: كود واحد شغال في مكانين — في الحجز (Booking) وفي المتجر (Shop) — بخصم نسبة معينة (افتراضي 20%).
- **المتجر (Shop)**: منتجات تذكارية (زي اللي بتتباع فعليًا عند الأهرامات) — نظام بيع مبسط، بيتدفع من نفس البنك.

---

### ✅ قرارات اتاخدت فعليًا (خلاص متفق عليها، متتراجعش عنها)

| القرار | التفاصيل |
|---|---|
| **لغة البنك** | Python (مش C#) — Microservice مستقل تمامًا |
| **الفريموورك** | FastAPI (مش Flask) — لأنه بيديك Swagger Docs مجانًا + أسرع |
| **الداتابيز بتاعة البنك** | SQLite منفصلة (`bank.db`) — مش SQL Server بتاع الموقع الأساسي |
| **الاتصال بين الموقع والبنك** | REST API عادي (HTTP) — الموقع (ASP.NET) بينادي الـ API عن طريق `HttpClient` |
| **الـ GUI للمتابعة** | Web Dashboard بسيطة (`/dashboard`) — بتتحدث كل 5 ثواني، تفتحها في تاب في المتصفح |
| **الرصيد الابتدائي لليوزر الجديد** | مبلغ ثابت افتراضي (1000 EGP وهمي) **+** إمكانية شحن يدوي (Top-Up) بعد كده |
| **كود الخصم (Coupon)** | Single-use (يتقفل بعد أول استخدام) **+** له تاريخ صلاحية (10 أيام من وقت ما اتجاب) |
| **نسبة الخصم الافتراضية** | 20% (قابلة للتغيير وقت إنشاء الكود) |
| **⚠️ Register في الموقع ≠ حساب بنكي** | الموقع مفتوح لأي حد يعمل Register عادي — ده **منفصل تمامًا** عن البنك. الـ Register **مبقاش** بينادي `/accounts/create` أوتوماتيك |
| **إزاي بتتعمل الحسابات البنكية** | يدويًا بس من الـ Swagger (`/docs`) بتاع البنك وقت التست — إحنا (المطور) بنعمل الحسابات دي مش الموقع |
| **مين يقدر يحجز فعليًا** | بس اللي عنده حساب بنكي حقيقي (Sara/Ahmed... إلخ اللي اتعملهم حساب يدوي). أي حد تاني يقدر يتصفح الموقع لكن مش هيقدر يخلص حجز |
| **إزاي بيتحقق الحجز إن اليوزر عميل بنك فعلي** | لما يفتح صفحة الدفع، بيدخل بيانات الكارت (رقم/اسم/تاريخ/CVV) بنفسه — والبنك بيتحقق إنها **مطابقة فعليًا** لحساب موجود، زي أي Payment Gateway حقيقي (Stripe/PayPal) |
| **رسالة الرفض لو البيانات غلط** | رسالة عامة واحدة: "بيانات الدفع غير صحيحة" — بدون تحديد أي حقل بالظبط غلط (نفس فلسفة الأمان في البوابات الحقيقية) |
| **ربط الإيميل بالكارت** | لازم إيميل اليوزر المسجل في الموقع يطابق إيميل صاحب الحساب في البنك (حماية إضافية — مينفعش حد يستخدم كارت حد تاني) |
| **الـ Refund عند إلغاء الحجز** | فوري وكامل (100% من المبلغ) — بس لو جوه الـ 48 ساعة زي قاعدة الإلغاء الموجودة بالفعل في نظام الحجوزات |

---

### ✅ اللي خلص فعليًا (Python Bank Service)

المشروع بالكامل موجود في مجلد `bank_service/` وبقى شغال ومتستنج بالكامل (بما فيه الـ Card Validation والـ Refund):

```
bank_service/
├── main.py                          ← كل الـ Endpoints (Accounts, Payments, Coupons, Dashboard)
├── models.py                        ← SQLAlchemy Models (BankAccount, BankTransaction, Coupon) + bank.db
├── schemas.py                       ← Pydantic Validation لكل Request/Response
├── requirements.txt
├── static/
│   └── dashboard.html               ← لوحة المتابعة (بتعرض TopUp/Purchase/Refund بألوان مختلفة)
├── README.md                        ← إزاي تشغّله + أمثلة C# كاملة للربط
```

**الـ Endpoints الجاهزة ومتستنجة (Tested ✅):**
```
POST   /accounts/create               → إنشاء حساب بنكي (يدوي من الـ Swagger بس)
GET    /accounts/{email}              → عرض الرصيد + الكارت (Masked دايمًا)
GET    /accounts/{email}/transactions → سجل العمليات
POST   /accounts/{email}/topup        → شحن رصيد
POST   /payments/charge               → 🆕 الدفع الفعلي — بيتحقق من بيانات الكارت كاملة (رقم+اسم+تاريخ+CVV) قبل الخصم
POST   /payments/refund               → 🆕 استرجاع فوري وكامل عند إلغاء الحجز
POST   /coupons/create                → إنشاء كود خصم (بيتنادى بعد الكويز)
GET    /coupons/{email}               → كل أكواد اليوزر
POST   /coupons/validate              → التأكد إن الكود شغال من غير استخدامه فعليًا
GET    /dashboard                     → صفحة المتابعة
```

**تم اختبار الـ Flow الكامل يدويًا وشغال 100%:**
- إنشاء حساب يدوي (2000) → محاولة دفع برقم كارت غلط → **رفضت صح** برسالة عامة
- محاولة دفع بـ CVV غلط (نفس الكارت) → **رفضت صح** بنفس الرسالة العامة (مفيش تلميح لمين الحقل الغلط)
- دفع ببيانات كارت صحيحة 100% (300 EGP) → **اتخصمت صح**
- إلغاء الحجز ونداء `/payments/refund` → **الفلوس رجعت كاملة وفورًا** (الرصيد رجع بالظبط زي ما كان)

تشغيل السيرفر:
```bash
cd bank_service
pip install -r requirements.txt
uvicorn main:app --reload --port 8001
```
- Swagger: `http://127.0.0.1:8001/docs`
- Dashboard: `http://127.0.0.1:8001/dashboard`

---

### ✅ اللي خلص فعليًا (ASP.NET Integration — حقيقي مش تخمين)

بعد ما بعتلي `BookingController.cs` و`Booking.cs` الحقيقيين، اتعملت التعديلات دي فعليًا على أساسهم (مش تخمين):

```
aspnet_integration/
├── BankModels.cs          ← DTOs (ChargeResult, RefundResult, CouponValidateResult, BankErrorResult)
├── BookingController.cs   ← نسخة كاملة معدّلة (Confirm + Cancel + ValidateCoupon الجديد)
└── INTEGRATION_STEPS.md   ← خطوات اللصق بالظبط + فيلدز الفورم المطلوب إضافتها + Program.cs
```

**اللي اتغيّر في `BookingController.cs`:**
- `Confirm`: بقى بياخد `cardNumber, cardHolderName, expiryDate, cvv, couponCode` من الفورم. بيحفظ الحجز مبدئيًا كـ `PendingPayment`، يكلم `/payments/charge`، ولو نجح → `Confirmed` + يسجل `Payment`. لو فشل (بيانات كارت غلط أو رصيد مش كافي) → بيمسح الحجز المبدئي ويرجع رسالة خطأ واضحة
- `Cancel`: **مبقاش الأدمن هو اللي بيرجع الفلوس يدويًا** — بينادي `/payments/refund` أوتوماتيك فور الإلغاء (لسه جوه نفس قاعدة الـ 48 ساعة الموجودة أصلاً)
- `ValidateCoupon` (Action جديدة): بتتنادى من الـ JS في صفحة الحجز للتحقق من الكود قبل الإرسال

**⚠️ لسه محتاج يدويًا:**
- إضافة فيلدز الكارت (`cardNumber`, `cardHolderName`, `expiryDate`, `cvv`, `couponCode`) في `Views/Booking/Create.cshtml` — الـ HTML + JS الجاهزين موجودين في `INTEGRATION_STEPS.md`
- تسجيل `HttpClient("BankService")` في `Program.cs`
- التأكد من أسماء فيلدز `Payment.cs` (افترضتها `BookingId, Amount, PaymentDate, PaymentMethod, Status` زي الكود الأصلي)

**🕐 الحالة الحالية (Blocked — مستنيين ملفات):**
طلبت من صاحب المشروع يبعت الملفات دي الحقيقية عشان أعدلها وأبعتها جاهزة 100% للصق المباشر (بدل ما هو يلزق كود يدوي):
- `Payment.cs`
- `Create.cshtml` (صفحة الحجز الحالية)
- `Program.cs`
- (اختياري) `_Layout.cshtml` أو أي CSS خاص بشكل الفورمات، عشان فورم الدفع يطلع بنفس شكل باقي الموقع

**✅ الملفات وصلت واتصلحت — لكن مش بالترتيب المتوقع.** بدل ما نكمل فيلدز الكارت في `Create.cshtml` مباشرة، صاحب المشروع رجع بمشاكل حقيقية طلعت في اللوجيك بتاع الـ Cancel/Refund/Confirm (تفاصيل كاملة في القسم الجديد تحت 👇). القسم ده اتحل بالكامل، ودلوقتي نقدر نرجع لخطوة 3.5 (فيلدز الكارت في Create.cshtml) في أي وقت.

---

### ✅ اللي خلص فعليًا (Cancel / Refund State Machine + إصلاحات لوجيك الأدمن)

> ده Session كامل منفصل حصل بعد ما الأدمن بدأ يستخدم النظام فعليًا ولاحظ تضارب في اللوجيك.
> المشاكل الأصلية اللي اتكتشفت:
> 1. Cancel من اليوزر كان بيرجع الفلوس فورًا (مفيش فترة انتظار) — مش منطقي زي المواقع الكبيرة
> 2. في الأدمن داشبورد: Refund بيحدّث Wallet Balance و Total Spent صح، لكن لو رجّع الحجز Confirmed تاني، Total Spent بيتحدث بس Wallet Balance بيفضل ثابت (باگ حقيقي)
> 3. الأدمن كان يقدر يتلاعب بين Confirmed/Cancelled/Refunded من غير أي قيود منطقية (زي تأكيد حجز فات معاده)

**التشخيص:** `AdminController.ChangeBookingStatus` كان بينادي البنك بس لما الحالة الجديدة = `"Refunded"`. أي انتقال تاني (خصوصًا الرجوع لـ `Confirmed`) كان بيغيّر النص في الداتابيز المحلية بس من غير أي نداء بنك فعلي — من هنا جات مشكلة الـ Wallet Balance الثابت. كمان لُقي باگ منفصل في `main.py`: نداء `/payments/refund` مرتين على نفس الحجز كان بيرجّع الفلوس مرتين (مفيش تحقق من إن العملية اتعمللها Refund قبل كده).

**الحل: State Machine مركزي (`BookingStatusService.cs` — ملف جديد)**
كل تغيير حالة حجز (من الأدمن أو اليوزر) بقى لازم يمر من الكلاس ده، وهو اللي بيقرر يكلم البنك ولا لأ:

| من | لـ | بيحصل إيه |
|---|---|---|
| `Confirmed` | `Cancelled` | تسجيل `CancelledAt` بس — **مفيش نداء بنك** (الفلوس لسه معلقة) |
| `Cancelled` | `Refunded` | تلقائي بعد 24 ساعة (Background Job) **أو** يدوي فوري من الأدمن — بينادي `/payments/refund` فعليًا |
| `Confirmed` | `Refunded` | مباشر (تخطي Cancelled) — بينادي البنك فورًا |
| `Cancelled` | `Confirmed` | "تراجع عن الإلغاء" — **مفيش نداء بنك** (الفلوس أصلاً ما رجعتش) |
| `Refunded` | أي حاجة | ❌ ممنوع تمامًا — حالة نهائية (Terminal State) |
| أي حاجة | `Confirmed` (غير من Cancelled) | ❌ ممنوع، وممنوع كمان لو `VisitDate` فات |

**الملفات اللي اتعدّلت/اتضافت:**
```
Models/
└── Booking.cs                          ← 🆕 حقل CancelledAt (DateTime?) + Migration AddCancelledAtToBooking

Services/                               ← 🆕 (لو المجلد مكانش موجود اتعمل جديد)
├── BookingStatusService.cs             ← 🆕 المصدر الوحيد لأي تغيير حالة حجز
└── BookingRefundBackgroundService.cs   ← 🆕 IHostedService، كل 10 دقايق يفحص Cancelled من 24 ساعة ويحولها Refunded

Controllers/
├── AdminController.cs                  ← ChangeBookingStatus بقى بينادي BookingStatusService
└── BookingController.cs                ← Cancel بقى بس يسجل CancelledAt (مفيش refund فوري)

Views/User/
└── Dashboard.cshtml                    ← زرار الـ Cancel اتصلح: كان بيتحقق من 48 ساعة بس، مش من VisitDate.
                                            دلوقتي بيتطابق تمامًا مع شرط BookingStatusService (منع إلغاء حجز فات معاده)

Program.cs                              ← 🆕 تسجيل BookingRefundBackgroundService كـ HostedService
                                            (جنب BookingStatusUpdater و PendingBookingCleanupService الموجودين أصلاً — اتأكدنا مفيش تضارب بينهم، كل واحد بيلمس حالة مختلفة)

bank_service/
└── main.py                             ← /payments/refund: 🆕 حماية ضد الاسترجاع المزدوج
                                            (رفض لو نفس related_type+related_id اتعمله Refund ناجح قبل كده)
```

**✅ تم اختبار الآتي فعليًا ونجح:**
- Cancel من اليوزر → الفلوس متتخصمش فورًا، بترجع أوتوماتيك بعد المدة المحددة (اتعمل تست بـ `RefundAfter = 1 minute` مؤقتًا، ورجّعناها 24 ساعة بعد كده)
- Admin: Cancelled → Confirmed ("تراجع عن الإلغاء") → Wallet Balance ما بيتغيرش، صح
- Admin: Confirmed → Refunded مباشر → الفلوس بترجع فورًا، Wallet Balance بيزيد صح
- Admin: محاولة الرجوع من Refunded → بترفض برسالة واضحة (حالة نهائية)
- Admin: محاولة تأكيد حجز `VisitDate` بتاعه فات → بترفض
- البنك: محاولة Refund مرتين على نفس الحجز → التانية بترفض

**⚠️ ملحوظة مهمة لأي شات جديد:** فيه Background Services تانية شغالة أصلاً في المشروع (`BookingStatusUpdater` بيحول Confirmed→Visited كل ساعة لما VisitDate يعدي، و`PendingBookingCleanupService` بيمسح الحجوزات PendingPayment القديمة كل 5 دقايق). اتفحصوا الاتنين ومفيش تضارب مع `BookingRefundBackgroundService` — كل واحد بيلمس حالة/حقل مختلف تمامًا.

---

### ✅ اللي خلص فعليًا (Create.cshtml — إصلاح شامل للترجمة + الـ OTP + الـ Validation)

**السياق:** بعد ما اتأكد إن فيلدز الكارت في `Create.cshtml` و`HttpClient("BankService")` في `Program.cs` كانوا موجودين فعلاً (خطوة 3.5 كانت خلصت)، صاحب المشروع رجع بـ 3 مشاكل حقيقية طلعت من التجربة الفعلية لصفحة الحجز:

1. طلب الـ OTP بالعربي كان بيطلع نص مبعثر شكل `&#x627;&#x62E;...` بدل الحروف العربية.
2. نفس الرسالة ("لا يوجد حساب بنكي لهذا المستخدم") كانت بتظهر عربي حتى واليوزر شغّال بالإنجليزي.
3. رسالة "Please select a visit date first" كانت بتفضل عالقة على الشاشة حتى بعد ما اليوزر يعدّل التاريخ فعلاً، من غير أي تفسير ليه زرار "تأكيد الحجز" لسه معطل.

**🐛 السبب الجذري (حاجة واحدة وراها المشكلتين 1 و2):**
- الـ `HtmlEncoder` الافتراضي بتاع ASP.NET Core بيحول أي حرف مش Basic Latin (زي العربي) لـ HTML numeric entity (`&#x627;`) كإجراء أمان افتراضي. ده شغال عادي لو النص اتحط في HTML markup عادي (المتصفح بيفكه صح)، لكن لو نفس القيمة اتحطت جوه `<script>` كـ JS string (زي `'@Html.L("...")'`)، بتفضل زي ما هي حرفيًا لأنها مش HTML text node.
- الرسالة التانية ("لا يوجد حساب بنكي...") مكانتش أصلاً بتعدي على `LocalizationService` — كانت نص Hardcoded عربي جاي مباشرة من خدمة البنك (Python) وبيتمرر زي ما هو في `BookingController.cs` (`error?.detail`) من غير أي ترجمة حسب لغة الجلسة.

**🆕 باگ لوجيك إضافي اتلاقى أثناء التفصيص (مش في الرسالة الأصلية):** لو اليوزر داس "ابعت كود تحقق" وبعدين غيّر التاريخ أو عدد التذاكر **قبل** ما يأكد الحجز، `RequestOtp` (مسار `existingBookingId`) كان بيتجاهل التغيير تمامًا ومبيحدّثش `VisitDate`/`NumberOfTickets`/`TotalPrice` — يعني ممكن يتحجز بتاريخ أو سعر مختلف عن اللي ظاهر على الشاشة.

**✅ الإصلاحات اللي اتعملت:**
- `Program.cs`: تسجيل `HtmlEncoder` بيسمح بنطاقات اليونيكود العربي (Basic Latin + Arabic + Arabic Supplement + Extended-A + Presentation Forms A/B) — ده الحل الجذري لمشكلة الـ `&#x627;`.
- `LocalizationService.cs`: إضافة `GetFormatted(key, lang, params args)` للرسائل اللي فيها قيمة متغيرة (زي مبلغ الخصم بالكوبون).
- `BookingController.cs`: كل الرسائل (`RequestOtp`, `ValidateCoupon`, `Confirm`, `Cancel`) بقت بتيجي من `LocalizationService` حسب لغة الجلسة — وبطلنا نعرض نص الخطأ الخام من خدمة البنك؛ بدل كده بنفرّق حسب الـ **HTTP status code بس** (404 = مفيش حساب بنكي، 400 = بيانات دفع غلط) ونعرض رسالتنا احنا. كمان اتصلح باگ عدم تحديث الحجز عند تغيير التاريخ/التذاكر بعد أول طلب OTP، واتنضّفت رسائل الـ Debug اللي كانت في `Cancel` (`❌ NO SESSION` وغيرها) واتحولت لرسائل مترجمة حقيقية.
- `Create.cshtml`:
  - كل نص بيتحط جوه `<script>` بقى بيعدي من خلال `Func<string, IHtmlContent> js = key => Html.Raw(JsonSerializer.Serialize(Html.L(key)))` بدل ما يتحط مباشرة بين `' '` — طبقة حماية إضافية مستقلة عن إعداد الـ Encoder (بتحل مشكلة الـ escaping وكسر الـ JS syntax مع بعض).
  - إضافة `novalidate` على الفورم + مسح تلقائي لرسالة الـ OTP القديمة لما اليوزر يعدّل التاريخ أو عدد التذاكر + تلميح ثابت (`confirmHint`) تحت زرار "تأكيد الحجز" يوضح ليه هو معطل.
  - تصحيح `maxlength="4"` في فيلد الـ CVV لـ `maxlength="3"` عشان يتماشى مع الـ Validation الحقيقي في السيرفر (كان بيقبل اليوزر يكتب 4 أرقام وترفض بعدين من غير سبب واضح).
- `NEW_LOCALIZATION_KEYS.md`: ملف مرجعي فيه كل الـ Keys الجديدة (عربي/إنجليزي) اللي لازم تتضاف في `wwwroot/lang/ar.json` و `en.json`.

**🐛 غلطة Razor صغيرة اتصححت بعد أول build:** `@js("Booking_Coupon_Applied").replace('{0}', ...)` كانت بتخلي الـ Razor يفتكر إن `.replace(...)` كمان جزء من كود C# (مش JS)، فطلعت أخطاء `CS0103`/`CS1012`. الحل: تطويق الاستدعاء بقوسين صريحين `@(js("..."))` عشان الـ Razor يعرف بالظبط فين الكود بينتهي. كمان اتضاف `@using Microsoft.AspNetCore.Html` أعلى الملف (كان ناقص عشان `IHtmlContent`).

**✅ الخطوة اليدوية اتعملت:** الـ JSON اللي كان في `NEW_LOCALIZATION_KEYS.md` اتلزق فعليًا جوه `wwwroot/lang/ar.json` و `en.json`، والمشروع اتعمله Build والتستنج اليدوي خلص بنجاح.

**✅ `HtmlHelperExtensions.cs`:** اتفحص ومحتاجش أي تعديل — الميثودز فيه (`L`, `D`, `Digits`, `DateLoc`, `Num`) بتستخدم الـ `HtmlEncoder` المسجّل في الـ DI، فاستفادت أوتوماتيك من إصلاح `Program.cs` من غير ما تتلمس.

---

### ⏳ اللي لسه هيتعمل (بالترتيب)

```
[x] 1. Python Bank Service (Models + API + Dashboard) ✅ خلص ومتستنج
[x] 2. Card-Validated Payments (/payments/charge) + Refund (/payments/refund) ✅ خلص ومتستنج
[x] 3. تعديل BookingController.cs الحقيقي (Confirm + Cancel + ValidateCoupon) ✅ خلص
[x] 3.5. فيلدز Create.cshtml + Program.cs HttpClient ✅ خلص (كانت اتعملت فعلاً)
[x] 3.6. Cancel/Refund State Machine + إصلاحات لوجيك الأدمن (BookingStatusService + Background Job + منع Refund مزدوج) ✅ خلص ومتستنج يدويًا
[x] 3.7. إصلاح شامل لصفحة Create.cshtml: مشكلة ترميز العربي (&#x627;) + ترجمة رسائل OTP/البنك + Validation/UX ✅ خلص بالكامل (الكود + دمج JSON keys في ar.json/en.json + تستنج يدوي بعد الـ Build)
[x] 4. بناء نظام المتجر (Shop) — خلص بالكامل ✅ (الكود + اللصق + Navigation + Admin Panel Tab)،
        تفاصيل كاملة تحت في "🛍️ Shop System — الكود جاهز (قيد اللصق)":
        - [x] Model: Product (اسم، صورة، سعر، وصف، كمية متاحة) ✅
        - [x] Model: ShopOrder (بيستخدم /payments/charge بنفس منطق الحجز بالظبط) ✅
        - [x] Admin CRUD للمنتجات (نفس باترن Gods) ✅
        - [x] صفحة عرض المنتجات + صفحة تفاصيل + نفس فورم الدفع ✅
        - [x] كل خطوات اللصق (1→9): Models + DbSets + Migration + Controllers + ViewModel +
              Views + Navigation + Localization + Bank Service (مفيهاش تعديل) ✅
        - [x] تاب "Shop" في Admin Panel (Views/Admin/Index.cshtml) بنفس شكل تاب Gods ✅
[x] 5. بناء الكويز (Quiz) ✅ خلص بالكامل ومتستنج — تفاصيل كاملة تحت في
        "🧠 Quiz Engine — Grade + Streak + خصم متغير (مكتمل)":
        - [x] Model (Session-only): QuizModels.cs (QuizAttempt/QuizQuestion/QuizChoice) ✅
        - [x] Model (دائم في الداتابيز): QuizHistory.cs — تسجيل كل كويز خلص (Grade/Streak/Discount) ✅
        - [x] QuizController.cs: Index/Start/Answer + Anti-Cheat (Timing Analysis) ✅
        - [x] QuizQuestionGeneratorService.cs — 13 نوع سؤال من كل جداول الداتابيز ✅
        - [x] Grade System (A+/A/B+/B) بخصم متغير مش ثابت ✅
        - [x] Streak System (يومي، منفصل عن الكوبون) + حد كويز واحد في اليوم ✅
        - [x] عرض النتيجة (Grade Badge + Streak + Coupon) في Index.cshtml ✅
```

---

### ⚠️ Key Rules — Bank/Shop/Quiz Ecosystem
- **الـ Bank Service لازم يكون شغال (uvicorn) في نفس وقت تشغيل الموقع** — لو مقفول، أي عملية دفع/إلغاء هترجع Connection Error.
- **الموقع مفتوح للتسجيل لأي حد — لكن الحجز مقفول** إلا لو اليوزر عنده حساب بنكي حقيقي اتعمله يدويًا. ده تصميم مقصود مش نقص.
- رقم الكارت الكامل **ماينفعش يترجع من أي Endpoint أبدًا** (Masked بس دايمًا) — لو محتاج تجيبه وقت التست، افتح `bank.db` مباشرة بأداة زي DB Browser for SQLite.
- الأرقام والكروت كلها وهمية 100% — مفيش أي بوابة دفع حقيقية أو بيانات حقيقية بتتخزن.
- الكوبون مربوط بـ `user_email` — يعني كود اليوزر A مينفعش يستخدمه اليوزر B حتى لو عنده الكود.
- الـ Refund بيدور على آخر عملية Purchase ناجحة بنفس `related_type` + `related_id` ويرجع نفس قيمتها بالظبط — مفيش حاجة بتتحسب يدويًا.
- أي تعديل مستقبلي في نسبة الخصم الافتراضية أو مدة الصلاحية يتم من `CouponCreate` schema (`discount_percent`, `valid_days`) — مش Hardcoded جوه المنطق.
- **حد الكوبون (70%) وحد الاستمرار في الـ Streak (50%) منفصلين تمامًا عن بعض** — ميتلخبطوش في بعض في أي تعديل مستقبلي (تفاصيل كاملة تحت في قسم Quiz Engine).
- **الـ Quiz History بيتسجل دايمًا في الداتابيز** (نجح أو فشل) — مش Session بس زي الـ Attempt نفسه. ده أساس فحص "لعب النهاردة؟" وحساب الـ Streak، فمينفعش يتشال أو يتاجل حفظه.

---

## 🧠 Quiz Engine — Grade + Streak + خصم متغير (✅ مكتمل)

> ⚠️ **القسم ده موجود عشان في أي شات جديد، تبعتلي الملف وأنا أبقى فاهم إحنا واقفين فين بالظبط.**

### 🎯 الفكرة الأساسية
كوبون الكويز مبقاش نسبة ثابتة (كان 20% تابت في البداية) — بقى **متغير** حسب أداء اليوزر، وبقى فيه نظام Streak يومي يشجع الدخول كل يوم، منفصل تمامًا عن نظام الكوبون.

### ✅ قرارات اتاخدت فعليًا (خلاص متفق عليها، متتراجعش عنها)

| القرار | التفاصيل |
|---|---|
| **عدد الأسئلة** | 10 أسئلة لكل كويز (`QuestionsPerQuiz` في `QuizController.cs`) |
| **الوقت لكل سؤال** | 20 ثانية ثابتة (بغض النظر عن الصعوبة) — التايمر الإجمالي اتقرر إنه مش لازم، لأنه أصلاً محدد ضمنيًا (عدد الأسئلة × الوقت) |
| **حد الكوبون (Coupon Eligibility)** | 70% صح فأكتر (`PassScorePercent`) — لازم يكون فوقه عشان ياخد كوبون خالص |
| **حد الـ Streak (Streak Eligibility)** | 🆕 **50%** صح فأكتر (`StreakScorePercent`) — **منفصل تمامًا عن حد الكوبون**. اليوزر ممكن يحافظ على الـ Streak من غير ما ياخد كوبون في نفس اليوم |
| **ليه فيه فصل بين الحدين** | هدف الـ Streak إنه يبني عادة دخول يومي (زي Duolingo)، مش يقيس تفوق. لو ربطناه بنفس حد الكوبون (70%)، هيبقى صعب جدًا إن اليوزر يحافظ على Streak طويل، وهيضرب الهدف الأساسي (تشجيع الدخول اليومي) |
| **Grade Tiers (حسب نسبة الكويز نفسه)** | A+ (95-100%) → 25% \| A (85-94%) → 20% \| B+ (75-84%) → 15% \| B (70-74%) → 10% \| أقل من 70% → "Fail" (مفيش كوبون، لكن ممكن الـ Streak يفضل مستمر لو فوق 50%) |
| **Streak Bonus Tiers** | يوم 1 → +0% \| 3 أيام → +5% \| 5 أيام → +8% \| 7 أيام → +12% \| 14 يوم → +16% \| 30 يوم → +20% (سقف الـ Bonus نفسه) |
| **سقف الخصم الإجمالي الأقصى** | **35%** (`MaxTotalDiscountPercent`) — Grade Discount + Streak Bonus مع بعض، مهما زادت الأرقام، محدش يعدي الـ 35% (حماية تجارية) |
| **حد الكويز اليومي** | كويز واحد بس في اليوم لكل يوزر (حسب `PlayedAt.Date`) — بيتحقق منه في `Start()` من آخر سجل في `QuizHistories`، مش من الـ Session (عشان ميتلفش بمسح الكوكيز) |
| **قطع الـ Streak** | لو اليوزر فوّت يوم كامل (مش النهاردة ولا إمبارح)، أو آخر كويز كان تحت الـ 50%، الـ Streak بيرجع لـ 1 من الأول (زي Duolingo بالظبط — صفر سماح) |
| **فشل نداء البنك (Bank Service down)** | لو `/coupons/create` فشل، الكوبون بيتلغي (`discountPercent = 0`) بس **الـ Streak مبيتلمسش** — فشل تقني من عندنا مش لازم اليوزر يتعاقب عليه |
| **مدة صلاحية كوبون الكويز** | 10 أيام (`QuizCouponValidDays`) — أقصر من كوبون المتجر العادي، عشان يشجع الاستخدام بسرعة |
| **Anti-Cheat** | موجود من الأول (مش جديد): حد أدنى لسرعة الإجابة (0.35 ثانية) + تحليل نمط الأوقات (سرعة/ثبات غير طبيعي مع نتيجة شبه كاملة = مشبوه، الكوبون مبيتديش) |

### ✅ اللي خلص فعليًا

```
Models/
├── QuizModels.cs      ← QuizAttempt/QuizQuestion/QuizChoice — Session-only (JSON)، IsCorrect مبيوصلش للـ Client أبدًا
└── QuizHistory.cs      ← 🆕 جدول دائم في الداتابيز (مش Session) — بيسجل كل كويز خلص:
                           Score, Grade, StreakEligible, StreakDays, DiscountPercent, CouponCode, PlayedAt
                           ده أساس فحص "لعب النهاردة؟" وحساب الـ Streak صح حتى لو الـ Session انتهت

Services/
└── QuizQuestionGeneratorService.cs   ← 13 نوع سؤال مختلف بيتولدوا من: Pharaohs, Temples, Museums,
                                          Gods, Dynasties, HistoricalEvents, Artifacts (True/False كمان)
                                          🐛 اتصلح فيه باگ: كان بيستخدم a.MuseumName (مش موجود في
                                          Artifact.cs الحقيقي) بدل a.Museum — اتصحح مع دعم Pick(ar/en)

Controllers/
└── QuizController.cs
    ├── Index() (GET)   ← 🆕 بيحسب الـ Streak الحالي + هل لعب النهاردة، وبيبعتهم للـ View (ViewBag)
    ├── Start() (POST)  ← 🆕 بيرفض كويز جديد لو اليوزر لعب النهاردة خلاص (بيتحقق من QuizHistories، مش Session)
    └── Answer() (POST) ← فيها كل منطق الـ Grade + Streak + الخصم المتغير + نداء /coupons/create
                            + حفظ QuizHistory دايمًا (نجح أو فشل)

Views/Quiz/
└── Index.cshtml        ← شاشة البداية بتعرض رسالة الـ Streak الحالي (🔥 أو تشجيع يبدأ واحد جديد)
                            + تعطيل زرار البدء لو لعب النهاردة خلاص
                            شاشة النتيجة بتعرض: Grade Badge + Streak Line (مستقل) + Coupon Box (لو استاهل)
```

**✅ Migrations المطلوبة (اتعملت):**
```bash
dotnet ef migrations add AddQuizHistory
dotnet ef migrations add AddStreakEligibleToQuizHistory
dotnet ef database update
```

**مفاتيح ترجمة جديدة اتضافت في `ar.json` / `en.json`:**
```
Quiz_AlreadyPlayedToday, Quiz_NoCouponMessage, Quiz_DiscountEarned,
Quiz_StreakDays, Quiz_CurrentStreakActive, Quiz_CurrentStreakNone
```

---

## 🛍️ Shop System — الكود جاهز (قيد اللصق)

> ده تفصيل بند "4. بناء نظام المتجر" فوق. الكود اتبنى بالكامل بناءً على `BookingController.cs`،
> `AdminController.cs`، `Booking.cs`، `Payment.cs`، و`Create.cshtml` الحقيقيين (مش تخمين) — بس
> **لسه محتاج لصق يدوي** في المشروع + شوية حاجات ملقيتش الملفات بتاعتها (تحت في "لسه محتاج يدويًا").

### ✅ اللي خلص فعليًا (كود جاهز للصق)

```
Models/
├── Product.cs         ← Id, Name, NameAr, Description, DescriptionAr, Price, ImageUrl, StockQuantity
├── ShopOrder.cs        ← نفس شكل Booking.cs بالظبط (UserEmail, ProductId, Quantity, TotalPrice,
│                          Status, CreatedAt, CancelledAt, [NotMapped] ProductName/ProductImage)
└── ShopPayment.cs      ← 🆕 جدول مستقل عن Payment.cs الأصلي (مش استخدمناه) لأنه مربوط بـ
                           BookingId كـ FK إجباري + navigation property Booking، مفيش مكان فيه
                           لـ ShopOrderId من غير ما نكسره أو نضيفله حقل. نفس شكل Payment.cs بالظبط
                           بس لـ ShopOrderId بدل BookingId.

Controllers/
├── ShopController.cs   ← مطابق لـ BookingController.cs حرفيًا في المنطق:
│                          - Index() → عرض المنتجات
│                          - Details(id) → صفحة المنتج + فورم الدفع (بديل Booking/Create)
│                          - ValidateCoupon(code) → نفس endpoint البنك، بدون أي تغيير
│                          - RequestOtp(productId, quantity, existingOrderId) → بينشئ/يحدّث
│                            ShopOrder كـ PendingPayment وبيتحقق من الـ Stock مرتين (وقت الطلب
│                            الأول ووقت أي إعادة طلب كود)
│                          - Confirm(...) → نفس الـ Validation بتاع الحجز بالظبط (Card/CVV/OTP) +
│                            نفس منطق قراءة أخطاء البنك (Coupon/OTP/NoAccount) + فحص Stock تالت
│                            مرة قبل الخصم مباشرة (Race condition safety) + خصم StockQuantity
│                            فعليًا بعد نجاح الدفع بس
│                          - MyOrders() → سجل الطلبات (بديل MyBookings)
└── AdminController.cs  ← نفس الملف الأصلي + إضافات:
                           - AddProduct / EditProduct / DeleteProduct (نفس باترن AddGod/EditGod/
                             DeleteGod بالظبط) — DeleteProduct بيرفض الحذف لو فيه ShopOrders
                             مرتبطة (زي حماية FK) بدل ما يكسر بيانات تاريخية
                           - Index(): إضافة TotalProducts, Products, TotalShopOrders,
                             TotalShopRevenue لبيانات الـ Dashboard

Views/Shop/
├── Index.cshtml        ← Grid عرض المنتجات (صورة + اسم + سعر + الكمية المتاحة)
├── Details.cshtml       ← نفس هيكل Booking/Create.cshtml بالظبط (نفس الـ CSS classes: book-field,
│                           card-row, coupon-row, total-price, btn-gold) — بس Quantity counter
│                           بدل Date/Tickets، ومربوط بـ /Shop/RequestOtp و /Shop/Confirm
└── MyOrders.cshtml      ← سجل طلبات بسيط (صورة + اسم المنتج + الكمية + السعر + الحالة)

NEW_LOCALIZATION_KEYS_SHOP.md   ← كل مفاتيح الترجمة الجديدة (Shop_Title, Shop_UnitPrice,
                                    Shop_InStock, Shop_Quantity_Label, Shop_BuyBtn, Shop_OutOfStock,
                                    Shop_InvalidQuantity, Shop_ProductNotFound, Shop_PurchaseSuccess,
                                    Shop_MyOrders, Shop_NoOrders, Shop_NoProducts) — الباقي بيستخدم
                                    مفاتيح الحجز الموجودة أصلاً (Booking_CardDetails_Label، إلخ)
                                    من غير أي تكرار.

INTEGRATION_STEPS_SHOP.md       ← خطوات اللصق كاملة بالترتيب.
```

### 🆕 قرارات تصميم اتاخدت وقت البناء
- **البنك مش محتاج يعرف الفرق بين Booking وShop:** استخدمنا نفس الـ Endpoints
  (`/payments/request-otp`, `/payments/charge`, `/coupons/validate`) بالظبط، وبس غيّرنا
  `related_type` لـ `"ShopOrder"` بدل `"Booking"` — مفيش أي تعديل مطلوب في `bank_service/`.
- **الـ Stock بيتفحص 3 مرات:** أول طلب OTP، أي إعادة طلب OTP (لو اليوزر غيّر الكمية)، وآخر لحظة
  قبل نداء `/payments/charge` مباشرة — عشان نمنع سيناريو إن 2 يوزر يشتروا آخر قطعة في نفس الوقت.
  الخصم الفعلي لـ `StockQuantity` بيحصل بعد نجاح الدفع بس، زي أي متجر حقيقي.
- **الكوبون شغال في المتجر من غير أي تعديل:** لأنه أصلاً مربوط بـ `user_email` مش بنوع العملية.

### ✅ اللصق في المشروع الحقيقي — خلص بالكامل (خطوات 1→8 من INTEGRATION_STEPS_SHOP.md)
- Models, DbSets في `AppDbContext.cs`, Migration (`AddShopSystem`) + `database update`,
  Controllers, حقول `AdminOverviewViewModel.cs`, Views، والـ Localization Keys — كل ده خلص.
- Navigation (خطوة 7) خلصت كمان بعد ما بعتلي `_Layout.cshtml` و`Dashboard.cshtml` الحقيقيين:
  - `_Layout.cshtml`: لينك "🛍️ Shop" في الـ `nav-links` الرئيسي (بعد Translator مباشرة)،
    بيودي على `/Shop/Index` ونفس منطق الـ `active` class المستخدم في باقي اللينكات.
  - `Dashboard.cshtml`: لينك "🛍️ My Orders" جنب تاب Bookings في شريط `db-tabs`.
    **مش تاب داخلي** زي Bookings/Favorites (دول بيحتاجوا بيانات من `UserController.Dashboard()`
    والـ ViewModel بتاعتهم اللي مش متاحين عندي) — عملته لينك مباشر بيودي على صفحة
    `/Shop/MyOrders` المستقلة، وبياخد نفس كلاس `db-tab` عشان يبقى متسق بصريًا مع باقي التابز.
  - مفتاح `Nav_Shop` اتضاف لـ `NEW_LOCALIZATION_KEYS_SHOP.md` (عربي/إنجليزي).
- خطوة 9 (Bank Service) مكانتش محتاجة أي تعديل من الأصل.

### ✅ Admin Panel Tab (خلص فعليًا — بعد ما بعتلي Views/Admin/Index.cshtml الحقيقي)
اتبنى بنفس شكل تاب Gods حرفيًا (نفس الـ classes: `adm-nav-item`, `adm-panel`, `adm-table`,
`adm-overlay`, `adm-modal`...):
- Nav item جديد "🛍️ Shop" في الـ Sidebar بعد Gods مباشرة، بعدد المنتجات (`Model.TotalProducts`)
- Panel جدول (`panel-shop`) بأعمدة Image/Name/Price/Stock/Actions — عمود الـ Stock بيتلوّن أحمر
  لو وصل صفر (`adm-stock-zero`)
- Modal إضافة (`modalAddProduct`) وتعديل (`modalEditProduct`) بنفس هيكل مودالات الـ Gods
  بالظبط (حقول EN + قسم Arabic Translation اختياري) — الفرق الوحيد إن Gods عندها Role/Symbol
  والمنتجات عندها Price/StockQuantity بدلهم
- JS: `openEditProductBtn`/`openEditProduct` (نسخة من `openEditGodBtn`/`openEditGod`) + إضافة
  `shop: '🛍️ Manage Shop'` لخريطة `panelTitles`
- الحذف (`DeleteProduct`) بيستخدم نفس `showDeleteConfirm` الموجود، والبحث بيستخدم نفس
  `searchTable('shopTable', ...)` الجنريك — مفيش أي JS جديد غير اللي اتذكر فوق

### ⚠️ الوحيد المتبقي فعليًا
مفيش — كل بنود "🛍️ Shop System" خلصت (الكود + اللصق في المشروع + الـ Navigation + الـ Admin
Panel Tab)، بما فيها Cancel/Refund + تراك الشحن (تفاصيل تحت في "🆕 Shop — Cancel/Refund +
Shipping Tracking").

### 🕐 مش في النطاق الحالي (اتقال صراحة، مش نسيان)
- ~~مفيش Cancel/Refund للمتجر~~ ✅ اتعمل بالكامل — شوف "🆕 Shop — Cancel/Refund + Shipping
  Tracking" تحت.

---

## 🆕 Shop — Cancel/Refund + Shipping Tracking ✅ خلصت بالكامل

> الطلب: طالما الطلب لسه ما خرجش للشحن، اليوزر يقدر يلغيه والفلوس ترجع للبنك أوتوماتيك. اتبنى
> بنفس فلسفة نظام الحجز بالظبط (48hr rule بتاعة الحجز)، بس بمهلة 24 ساعة للريفند التلقائي.

### الموديل
- `ShopOrder.cs`: حقل جديد `ShippingStatus` (Processing / Shipped / Delivered) — منفصل تمامًا
  عن `Status` (PendingPayment/Confirmed/Cancelled/Refunded) عشان منبوظش المنطق الموجود أصلاً.
  الإلغاء متاح بس طالما `ShippingStatus == "Processing"`.
- Migration: `AddShopOrderShippingStatus` — عمود واحد بس، مع تحديث يدوي للأوردرات القديمة
  (Confirmed القديمة اتحطت Delivered افتراضيًا، والباقي Processing).

### السيرفيسز — نسخة طبق الأصل من بتاعة الحجز
- `ShopOrderStatusService.cs` ← نفس منطق `BookingStatusService.cs` حرفيًا:
  - Confirmed → Cancelled: تسجيل `CancelledAt` بس، بدون نداء بنك، وبشرط إضافي إن الطلب لسه
    Processing (قبل الشحن)
  - Cancelled → Refunded: نداء `/payments/refund` فعليًا + إرجاع المخزون (`StockQuantity`)
  - Cancelled → Confirmed: تراجع عن الإلغاء بدون نداء بنك
  - Refunded: حالة نهائية زي الحجز بالظبط
- `ShopOrderRefundBackgroundService.cs` ← نفس فكرة `BookingRefundBackgroundService.cs`: كل 10
  دقايق بيفحص الأوردرات Cancelled من 24 ساعة أو أكتر ويحولها Refunded تلقائيًا
- `Program.cs`: تسجيل `ShopOrderRefundBackgroundService` كـ Hosted Service جنب نظيره بتاع الحجز

### الكنترولرز
- `ShopController.Cancel(int id)` ← نفس منطق `BookingController.Cancel` بالظبط، بس بيتحقق من
  `ShippingStatus == "Processing"` بدل `VisitDate`
- `AdminController.cs`:
  - `ChangeShopOrderStatus(int id, string status)` ← نفس `ChangeBookingStatus` بالظبط (حالة الدفع)
  - `UpdateShopOrderShipping(int id, string shippingStatus)` ← جديد، تحديث تراك الشحن مباشرة
    (Processing → Shipped → Delivered)، بدون أي نداء بنك، ومتاح بس للأوردرات Confirmed
  - `Index()`: بقى بيجيب `ShopOrders` (مع `Items` وأسماء المنتجات) للعرض في تاب الـ Shop

### الـ Views
- `Views/Shop/MyOrders.cshtml`:
  - تراك بصري (Processing/Shipped/Delivered) بيبان بس لما الأوردر Confirmed
  - زرار "إلغاء الطلب" بيبان بس لو `Status == Confirmed && ShippingStatus == Processing`
  - **مودال إلغاء مخصص** (`cancelOrderOverlay`) بدل الـ `confirm()` الافتراضي بتاع المتصفح —
    بنفس فكرة مودال إلغاء الحجز في الداشبورد (بيعرض رقم الأوردر + المبلغ + تحذير الريفند)
- `Views/Admin/Index.cshtml`: جدول جديد "Shop Orders" تحت تاب الـ Shop (Order#, Customer, Items,
  Total, Payment status, Shipping status) + `<select>` سريع لتحديث تراك الشحن + مودال تغيير حالة
  الدفع (مطابق لمودال حالة الحجز)

### مفاتيح ترجمة جديدة
`Shop_Cancel_NotFound`, `Shop_Cancel_NotConfirmed`, `Shop_Cancel_AlreadyShipped`,
`Shop_Cancel_Order`, `Shop_Cancel_Confirm`, `Shop_Cancel_PendingRefundNote`, `Shop_Cancel_Keep`,
`Shop_Track_Processing`, `Shop_Track_Shipped`, `Shop_Track_Delivered`

### ⚠️ لسه متبقي (مش اتعمل)
- تحديث `AdminOverviewViewModel` بحقل `public List<ShopOrder> ShopOrders { get; set; }` —
  الملف نفسه مكانش متاح فاتعمل عليه الكود بس محتاج لصق يدوي.

---

## 🎨 Shop — UI/UX Redesign ✅ خلصت بالكامل

> الطلب: شكل صفحات الشوب كان حاسس "بلدي" — خط صغير، عناصر مش حاسة إنها بريميوم. اتعمل Redesign
> كامل لخمس صفحات (`Shop/Index.cshtml`, `Cart.cshtml`, `Checkout.cshtml`, `Details.cshtml`,
> `MyOrders.cshtml`) مع الحفاظ على هوية الموقع (دهبي/أسود + خط Cinzel + الطابع الفرعوني).

- Type scale أكبر وأوضح في كل الصفحات (العناوين، الأسعار، اللابلز)
- كروت المنتجات: gradient خفيف + shadow حقيقي عند الـ hover + zoom خفيف للصورة
- البادجز (Sale/New/Best Seller/Featured) بقت pills بتدرج لوني بدل مربعات مسطحة
- فاصل ذهبي زخرفي (𓋹) تحت عنوان كل صفحة
- `Checkout.cshtml`: ترقيم حقيقي للخطوات (01 توصيل → 02 دفع → 03 تأكيد) لأنها فعلاً عملية متسلسلة
- `Details.cshtml`: قسم المواصفات بقى شكله "لوحة أثرية" (provenance plaque) متسق مع طابع المتحف
- **باگ اتصلح خلال الـ Redesign:** صفحة `Cart.cshtml` كانت بتستخدم كلاسات `.shop-price` و
  `.ticket-counter` من غير ما تعرّفهم في الـ `<style>` الخاص بيها (كانوا بس متعرّفين في صفحات
  تانية زي `Details.cshtml`) — ده كان بيخلي السعر يبان بلون عادي والأزرار +/- تبان بشكل المتصفح
  الافتراضي (مربع أبيض). اتصلح بإضافة التعريفات الناقصة محليًا في `Cart.cshtml`.

---

## 🎨 Shop — UI/UX Redesign (Round 2: أيقونات + ألوان) ✅ خلصت بالكامل

> بعد الـ Redesign الأول، الشكل العام كان تمام بس لسه حاسس "بلدي". السبب الحقيقي مكنش الألوان
> الأساسية (الدهبي/الأسود شغالين كويس أصلاً) — كان **الإيموجي الملونة** (🛒🔥⭐✨🗑📍📞✖⚠️) اللي
> بتكسر الهوية البصرية لأن ألوانها عشوائية ومالهاش علاقة بالتيم، وألوان الـ badges/الفلاتر
> (أزرق/أخضر عاديين زي أي موقع Bootstrap) اللي مالهاش علاقة بالطابع الفرعوني.

- كل الإيموجي الملونة في الخمس صفحات (`Index`, `Details`, `Cart`, `Checkout`, `MyOrders`) اتستبدلت
  بأيقونات SVG أحادية اللون (`currentColor`) — سلة، تشيك، سلة مهملات، دبوس موقع، تليفون، إكس
  إغلاق، تحذير، تاج/عرض، نجمة، بريق. الرموز الهيروغليفية (𓋹 𓆃) والقلب (♥/♡) اتسابوا زي ما هم
  لإنهم مناسبين للتصميم أصلاً مش الإيموجي الملونة
- ألوان الـ badges (`badge-sale`/`badge-new`/`badge-bestseller`) وألوان الفلاتر اتغيرت لباليت
  أحجار كريمة مصرية: عقيق (carnelian) أعمق للـ Sale، لازورد (lapis) للـ New، فيروز/فاينس
  (faience) للـ Best Seller — بدل الأزرق/الأخضر الفاقعين اللي كانوا موجودين
- **باگ اتصلح أثناء الاستبدال:** أزرار "Add to Cart"/"Buy Now" كانت بترجّع نفسها لحالتها الأصلية
  بعد نجاح العملية بـ `btn.textContent = originalText` — ده كان هيمسح الأيقونة الـ SVG نهائيًا من
  أول ضغطة (لإن `textContent` بيمسح أي عنصر HTML جواه، مش بس النص). اتصلح باستخدام `innerHTML`
  بدل `textContent` في كل الأماكن دي (`Index.cshtml` → `addToCart()`, `Details.cshtml` →
  handlers بتوع `addToCartBtn`/`buyNowBtn`)

---

## 📍 Shop — Checkout: "استخدم موقعي" (GPS Autofill) ✅ خلصت بالكامل

> زي أمازون — زرار جنب حقل العنوان في `Checkout.cshtml` بياخد موقع المستخدم من المتصفح ويملأ
> العنوان والمحافظة تلقائيًا.

- `navigator.geolocation.getCurrentPosition()` من المتصفح — client-side بالكامل، مفيش أي تعديل
  في أي Controller أو قاعدة بيانات
- Reverse Geocoding عن طريق **Nominatim (OpenStreetMap)** — مجاني بالكامل، بدون API key
- دالة `matchGovernorate()` في الـ JS بتحاول تطابق اسم المحافظة الراجع من الـ GPS مع
  `<select id="governorate">` الموجود (بالعربي والإنجليزي، بتجاهل كلمة "محافظة"/"Governorate"
  واختلافات التشكيل) — عشان كده كل `<option>` بقى فيه `data-name-en`/`data-name-ar` كمان
- حالات الخطأ متغطاة كلها برسائل مختلفة (رفض الإذن، عدم دعم المتصفح، timeout، فشل الـ API) بنفس
  ستايل `coupon-msg` الموجود أصلاً
- ⚠️ **مطلوب يدويًا:** إضافة مفاتيح ترجمة جديدة في `en.json`/`ar.json`: `Shop_Checkout_UseLocation`,
  `Shop_Location_Unsupported`, `Shop_Location_Locating`, `Shop_Location_Success`,
  `Shop_Location_SuccessNoGov`, `Shop_Location_Denied`, `Shop_Location_Timeout`, `Shop_Location_Error`

---

## 🐛 Dashboard — My Orders Badge مكانش بيظهر ✅ اتصلح بالكامل

> الملاحظة: الداشبورد مكنش بيجيب عدد الطلبات جنب تاب "My Orders". السبب: تعليق قديم موجود في
> `Dashboard.cshtml` نفسه بيوضح إن اللينك اتعمل مباشر لصفحة `/Shop/MyOrders` من غير badge **لإن
> الكنترولر أصلاً مكانش بيحسب عدد الطلبات ولا بيبعته للـ View خالص** — يعني كان نقص وظيفي واضح
> ومُتعمّد ساعتها، مش باگ في منطق موجود.

- `UserController.cs` → `Dashboard()`: ضيف query لعدد الأوردرات (`context.ShopOrders`، مستبعد
  `PendingPayment` بنفس منطق الـ Bookings بالظبط) وبعتها عن طريق `ViewBag.TotalOrders` — مش عن
  طريق `DashboardViewModel` (بنفس أسلوب `ViewBag.JourneyCount` الموجود أصلاً في نفس الدالة، عشان
  ملف الـ ViewModel نفسه مش متاح في الشات)
- `Dashboard.cshtml`: تاب "My Orders" بقى بيعرض `tab-badge` بالعدد زي Bookings/Favorites بالظبط،
  وسكشن الـ Profile (`db-profile-stat-row`) بقى فيه عنصر رابع لعدد الطلبات جنب
  Bookings/Favorites/EGP Spent
- ⚠️ **تأكد بنفسك:** استخدمت اسم الـ DbSet `context.ShopOrders` افتراضًا (تسمية EF Core التلقائية
  من اسم الموديل `ShopOrder`) — لو الاسم مختلف في `AppDbContext` بتاعك، غيّره في السطر ده بس

---

## 🎯 خطة احتراف الـ Shop (✅ خلصت بالكامل — المراحل 1، 2، 3، 4)

> الهدف: الشوب يبان زي مواقع تسوق حقيقية (تفاصيل منتج غنية + تصنيفات وفلاتر + عروض وشارات).
> اتفقنا نبدأ بالترتيب ده بالظبط لأن كل مرحلة مبنية على اللي قبلها ومفيش تعارض مع Bank/Coupon الشغالين حاليًا.
> **ترتيب البدء المتفق عليه: 1) Reviews → 2) Categories → 3) صور متعددة → 4) العروض والشارات.**
> ⚠️ **قرار اتاخد بعدين:** الـ **Gallery (صور متعددة) اتلغت خالص من الخطة** — مش هتتعمل. بدالها هنعمل بس **المواصفات + منتجات مشابهة** من باقي المرحلة 1، بعد ما خلصنا Categories.

### المرحلة 1️⃣ — تفاصيل المنتج (Product Page)

**✅ التقييمات والريفيوهات — خلصت بالكامل (100% مؤكدة):**
- `ReviewController.cs` و `AppDbContext.cs`: **مفيش أي تعديل احتاجوه** — الـ `Type` أصلاً string حر مش محكوم بقايمة ثابتة في الكود، فـ `"product"` اشتغل من غير أي لمسة.
- `ShopController.cs` → `Details(int id)`: بقى بيجيب ريفيوهات المنتج (`Type == "product" && ItemId == id`) في `ViewBag.Reviews` + `ViewBag.UserReviewed`.
- `Views/Shop/Details.cshtml`: بقى بيعرض الـ `_Reviews` partial بعد كارت المنتج (كتابة/تعديل/حذف/هيلبفل/ريبورت — كله شغال زي أي صفحة تانية).
- `Review.cs`: تعليق توضيحي بس اتحدث (`pharaoh / temple / museum / god / artifact / product`) — مفيش لوجيك اتغير.
- **My Reviews في البروفايل:** اتأكد بعد ما بعتلي `Dashboard.cshtml` الحقيقي — **مفيش تاب "My Reviews" في البروفايل خالص أصلاً** (الموجود بس: Overview / Bookings / Favorites / Journey / Profile)، فمفيش حاجة كانت محتاجة تتصلح.
- **Admin Dashboard — تبويب Reports:** اتأكد بعد ما بعتلي `Views/Admin/Index.cshtml` الحقيقي — بانل `panel-reports` **مفيهوش أي lookup باسم العنصر حسب الـ Type أصلاً**، بيعرض بس `review.Comment` مباشرة، فمفيش مشكلة هنا.
- **Admin Dashboard — تبويب "All Reviews" (`panel-reviews`):** ده كان فيه المشكلة الحقيقية ✅ **اتصلحت** — الـ `switch` بتاع `r.Type` اللي بيبني لينك للعنصر كان ناقصه `case "product"` (كان بيرجع `#`)، وضفنا الـ `case` + خيار "Product" في dropdown الفلترة (`filterType`). الـ JS (`filterReviews`) مكنش محتاج تعديل لأنه أصلاً جنيريك على `data-type`.
- ⚠️ **ملحوظة مش باج:** صفحة Shop/Details بترفض الـ Guest (Redirect للـ Login) والـ Admin (Redirect للـ Home) قبل حتى ما توصل لجزء الريفيوهات — يعني حالة "سجّل دخول عشان تكتب ريفيو" مش هتظهر أبدًا هناك. لسه مش اتقرر نغيّرها ولا نسيبها.
- **متوسط التقييم ⭐ في `Shop/Index.cshtml`:** اختياري (لمسة شكل بس، مش أساسي وظيفيًا) — لسه ملمسناهوش.

**✅ باقي بنود المرحلة 1 — خلصت بالكامل هي كمان:**
| الإضافة | التفاصيل |
|---|---|
| ~~صور متعددة~~ | ❌ **اتلغت من الخطة نهائيًا** — مش هتتعمل |
| المواصفات | ✅ حقول `Material`/`MaterialAr`، `Dimensions`/`DimensionsAr`، `OriginRegion`/`OriginRegionAr` في `Product.cs` — بتتعرض في قسم "Specifications" بصفحة `Shop/Details.cshtml` (بيظهر بس لو فيه أي قيمة موجودة) |
| منتجات مشابهة | ✅ في `ShopController.Details()`: بياخد أول 4 منتجات من نفس الـ `Category`، ولو مش كفاية بيكمل بأقرب سعر (`Math.Abs(price diff)`) عشان السكشن ميفضلش فاضي |

### المرحلة 2️⃣ — تصنيفات وفلاتر وبحث ✅ خلصت بالكامل
| الإضافة | الحالة |
|---|---|
| جدول Categories | ✅ `Models/Category.cs` (Id, Name, NameAr) + Migration `AddProductCategories` |
| ربط المنتج بالتصنيف | ✅ `CategoryId` (FK, nullable) في `Product.cs` + navigation property `Category` |
| Admin CRUD للتصنيفات | ✅ mini-section جوه تاب Shop الموجود في `Views/Admin/Index.cshtml` (مفيش تاب Sidebar جديد بقرار مقصود) — `AddCategory`/`DeleteCategory` في `AdminController.cs` (الحذف بيفك الربط مش بيرفض زي المنتجات) |
| ربط الـ Category بالمنتج من الأدمن | ✅ `<select>` في مودالي Add/Edit Product + تحديث `openEditProductBtn`/`openEditProduct` JS + `existing.CategoryId = model.CategoryId` في `EditProduct` |
| فلترة | ✅ `ShopController.Index(int? categoryId, string? sort)` + شريط فلتر بالتصنيفات في `Views/Shop/Index.cshtml` |
| بحث | ✅ اتضاف لـ `HomeController.Search()` (بالاسم + اسم التصنيف) + سكشن عرض في `Views/Home/Search.cshtml` (مفتاح ترجمة جديد `Common_ProductsPlural`) |
| ترتيب (Sort) | ✅ الأحدث (بـ `Id` تنازليًا — الموديل مفيهوش `CreatedAt`) / الأقل سعر / الأعلى سعر / الأكثر مبيعًا (بيتحسب من `ShopOrders` الـ Confirmed) — `<select>` في `Views/Shop/Index.cshtml` |
| تصنيف الـ 82 منتج الموجودين | ✅ سكريبت SQL (`INSERT` لـ 12 تصنيف + `UPDATE` بالـ Id لكل منتج) اتشغّل يدويًا في SSMS — كل المنتجات بقى ليها `CategoryId` |
| مفاتيح ترجمة جديدة | `Shop_AllCategories`, `Shop_Sort_Newest`, `Shop_Sort_PriceLow`, `Shop_Sort_PriceHigh`, `Shop_Sort_BestSelling`, `Common_ProductsPlural` |

### المرحلة 3️⃣ — عروض وخصومات وشارات ✅ خلصت بالكامل
| الإضافة | الحالة |
|---|---|
| سعر مخفّض | ✅ `OriginalPrice` (nullable decimal) في `Product.cs` — لو موجود وأكبر من `Price` يبان Strikethrough + نسبة الخصم في `Shop/Index.cshtml` و `Shop/Details.cshtml` |
| حماية من خصم وهمي | ✅ في `AdminController.cs` (`AddProduct`/`EditProduct`) — لو `OriginalPrice` أصغر من أو يساوي `Price` بيتجاهلها ويتسجل `null` |
| شارات | ✅ `IsFeatured`, `IsBestSeller`, `IsNew` (bool) في `Product.cs` — بتتحط يدويًا من الأدمن بس (مفيش تلقائي لأن الموديل مفهوش `CreatedAt`) |
| الكوبون | ✅ شغال أصلاً في المتجر من غير أي تعديل |
| تنبيه المخزون | ✅ "Low stock" لو `StockQuantity` من 1 لـ 5 — نفس منطق الألوان الموجود في Admin |
| Admin CRUD للشارات والعرض | ✅ قسم "🏷️ Offers & Badges" في مودالي Add/Edit Product (`Views/Admin/Index.cshtml`) — Original Price + 3 checkboxes + عمود Badges في جدول المنتجات |
| فلاتر الشوب | ✅ 3 زراير في `Shop/Index.cshtml` (🔥 عروض، ⭐ الأكثر مبيعًا، ✨ جديد) — `ShopController.Index(bool? onSale, bool? bestSeller, bool? isNew)` — بيتجمعوا مع بعض (AND) ومع التصنيف والترتيب من غير ما يفصلوا بعض |
| Migration | `AddProductOffers` |
| مفاتيح ترجمة جديدة | `Shop_Badge_New`, `Shop_Badge_BestSeller`, `Shop_Badge_Featured`, `Shop_LowStock`, `Shop_Filter_OnSale`, `Shop_Filter_BestSeller`, `Shop_Filter_New` |

**باگ مهم اتصلح خلال التنفيذ:** في مودالات Add/Edit Product، الـ `checkbox` بتاع الشارات كان متكتوب بعد الـ `hidden` بترتيب غلط (`<input type="hidden">` قبل `<input type="checkbox">`)، فـ ASP.NET Core كان بياخد أول قيمة مبعوتة بس (`false` من الـ hidden) ويتجاهل حالة الـ checkbox خالص — يعني الشارات ماكنتش بتتسجل أبدًا حتى لو معلّمة. اتصلحت بعكس الترتيب (checkbox الأول وبعدين hidden).

### المرحلة 4️⃣ — لمسات احترافية إضافية ✅ خلصت بالكامل

**✅ Wishlist للمنتجات:**
- استخدمت جدول `Favorites` الموجود بالظبط بـ `Type = "product"` — مفيش جدول جديد اتضاف
- `FavoriteController.cs`: ضيف `"product"` في التحقق من الوجود (`Add`) وفي الـ redirect switch، وضيف بناء `favProducts` في `Index()`
- `FavoriteController.cs` (تحديث تاني): `Add`/`Remove` بقوا بيدعموا AJAX — لو الريكوست جاي بهيدر `X-Requested-With: XMLHttpRequest` بيرجعوا JSON (`{ success, favoriteId }`) بدل Redirect كامل للصفحة؛ أي استخدام قديم بلينكات عادية لسه شغال زي ما هو
- `ShopController.cs`: `Index()` بيجيب `Dictionary<ProductId, FavoriteId>` لليوزر الحالي، و`Details()` بيجيب حالة المفضلة + الـ FavoriteId بتاعها
- `Views/Shop/Index.cshtml` و`Views/Shop/Details.cshtml`: زرار قلب (♡/♥) بيتوجل بـ AJAX من غير أي Refresh للصفحة (فانكشن `toggleWishlist()` في الـ JS)
  - **🐛 باگ اتصلح خلال التنفيذ:** زرار القلب كان أصلاً `<a>` جوه `<a class="shop-card">` (اللي بيغلف الكارت كله كلينك) — الـ `<a>` جوه `<a>` مش HTML صحيح، فالمتصفح كان بيقفل الأنكور الخارجية تلقائيًا وبيقطع الكارت لـ 3 عناصر منفصلة جوه الـ Grid. اتصلح بتحويل زرار القلب لـ `<button type="button">` بدل `<a>`.
- `Views/Favorite/Index.cshtml`: سكشن "Products" جديد بنفس شكل باقي السكاشن (يستخدم `Common_ProductsPlural`)
- `UserController.cs` (Dashboard): **باگ منفصل اتصلح** — دالة `Dashboard()` كانت بتبني قوايم المفضلة يدويًا بـ `else if` لكل نوع (pharaoh/temple/god/museum/artifact) ومفيهاش أي حالة لـ `"product"` خالص، فالمنتجات المفضّلة كانت بتتحسب في `TotalFavorites` بس مش بتتعرض في أي تاب. اتضاف `else if (fav.Type.ToLower() == "product")` + قائمة `favProducts` + `FavoriteProducts` في الـ `DashboardViewModel`
- `Views/User/Dashboard.cshtml`: سكشن "Products" جديد جوه تاب Favorites (بزرار حذف شغال بـ `/Favorite/Remove`) + تحديث شرط الـ Empty State

**✅ Breadcrumbs:**
- `Views/Shop/Index.cshtml`: `Shop` (أو `Shop / اسم الكاتيجوري` لو متفلتر)
- `Views/Shop/Details.cshtml`: `Shop / (الكاتيجوري) / اسم المنتج`
- **قرار اتاخد بعد المراجعة:** بداية الـ Breadcrumb كانت هتبقى `Home / Shop / ...` بس اتشالت خالص بطلب — الـ Breadcrumb بيبدأ من `Shop` مباشرة من غير "Home"

**✅ SKU:**
- `Product.cs`: حقل `SKU` (nullable string) — تنظيمي داخلي بس، مش مفتاح فريد مفروض في الداتا بيز
- `Views/Shop/Details.cshtml`: بيتعرض جوه قسم "Specifications" (لو موجود)
- `Views/Admin/Index.cshtml`: حقل SKU في مودالي Add/Edit Product + عمود جديد في جدول المنتجات + تحديث الـ JS (`openEditProductBtn`/`openEditProduct`)
- `AdminController.cs`: `AddProduct`/`EditProduct` بينضّفوا الـ SKU (`Trim()` + `null` لو فاضي) قبل الحفظ — مفيش باراميتر جديد لازم لأن الدالتين أصلاً بتستقبلوا `Product model` كامل

### ⚠️ حاجات لسه مطلوبة يدويًا (مش كود، حاجات تظبيط بيئة/بيانات):
1. **Migration**: `Add-Migration AddProductSKU` ثم `Update-Database` (عشان عمود `SKU` يتضاف فعليًا في جدول `Products`)
2. **`DashboardViewModel.cs`** (في `ViewModels/`، مش اتبعتت في الشات ده): لازم يتضاف فيها يدويًا:
   ```csharp
   public List<FavoriteCardViewModel> FavoriteProducts { get; set; } = new();
   ```
3. **مفاتيح ترجمة جديدة** في `wwwroot/lang/en.json` و`ar.json`:
   - `Shop_Specs_SKU`: "SKU" / "رقم المنتج (SKU)"
   - `Fav_Add_Title`: "Add to Wishlist" / "إضافة للمفضلة"
   - `Fav_Remove_Title`: "Remove from Wishlist" / "إزالة من المفضلة"

---

## 🛒 Shop — Amazon-style Checkout Flow ✅ خلصت بالكامل

> الهدف: نقرّب تجربة الشراء في الـ Shop من أمازون — صفحة منتج منفصلة عن صفحة الدفع، سلة تجمع أكتر من منتج، عنوان + رقم تليفون، وشحن بيتحسب حسب المحافظة.

### الترتيب اللي اتنفذ
1. [x] السلة (Cart) — أكبر تغيير معماري، اتعمل الأول زي ما اتخطط
2. [x] فصل `Shop/Details.cshtml` (عرض بس) عن صفحة `Checkout.cshtml` جديدة
3. [x] الشحن حسب المحافظة
4. [x] العنوان + رقم التليفون
5. [x] ربط الدفع بالبنك (نفس نظام OTP/Charge المستخدم في Booking)
6. [x] ربطها بـ My Orders في الداشبورد

### 1️⃣ السلة (Cart)
- جدول `CartItem` (Id, UserEmail, ProductId, Quantity, AddedAt) — مرتبط باليوزر مش بالـ Session
- `ShopController`: `AddToCart`, `Cart()` (View), `UpdateCartItem`, `RemoveFromCart`, `CartCount` (endpoint خفيف للـ nav)
- أيقونة سلة 🛒 في `_Layout.cshtml` (تبان في كل صفحات الموقع، مش الشوب بس) + في `Shop/Index` و`Shop/Details`
- تصحيح تلقائي: لو منتج اتحذف أو المخزون قلّ عن الكمية المحفوظة في السلة، بيتصحح أو يتشال تلقائيًا بدل ما يكسر الصفحة

### 2️⃣ فصل صفحة المنتج عن الدفع
- `Shop/Details.cshtml`: بقت عرض بس (صورة + وصف + سعر + مواصفات + ريفيوهات) + زرارين: **"Add to Cart"** و **"Buy Now"** (Buy Now = إضافة للسلة + توجيه مباشر لـ Checkout)
- **قرار مهم:** شيلنا فورم الكارت المباشر اللي كان في `Details.cshtml` بالكامل — الشراء كله بقى بيعدي من مسار واحد (السلة → Checkout) بدل ما يكون فيه مسارين دفع منفصلين (منتج واحد / سلة) بيتصلحوا لوحدهم بعدين
- `Shop/Checkout.cshtml` (صفحة جديدة): ملخص الطلب (عناصر السلة) + فورم العنوان/التليفون/المحافظة + فورم الكارت/الكوبون/OTP + الإجمالي (Subtotal + Shipping)

### 3️⃣ الشحن حسب المحافظة
- `Models/Governorates.cs`: جدول ثابت في الكود لـ 27 محافظة، كل واحدة بسعر شحن مختلف (Key, NameEn, NameAr, ShippingFee)
- الشحن بيتحسب أوتوماتيك في الـ Checkout عند اختيار المحافظة (JS بيحدّث الإجمالي فورًا) وبيتخزن في `ShopOrder.ShippingFee`
- `TotalPrice = مجموع (سعر الوحدة × الكمية لكل عنصر) + ShippingFee`

### 4️⃣ العنوان + رقم التليفون
- اتحطوا كفيلدز مباشرة على `ShopOrder`: `PhoneNumber`, `Address`, `Governorate`
- فاليديشن: رقم التليفون لازم 11 رقم ويبدأ بـ "01"، العنوان مطلوب، المحافظة لازم تكون من الـ 27 المعرّفين في `Governorates.cs`
- *(ملحوظة: لسه مفيش جدول عناوين محفوظة زي أمازون فعليًا — كل أوردر بياخد عنوان جديد وقت الـ Checkout. ده تحسين مستقبلي لو احتجناه)*

### 🔄 التغيير المعماري في ShopOrder
- `ShopOrder` بقت بيانات عامة بس (UserEmail, TotalPrice, ShippingFee, PhoneNumber, Address, Governorate, Status, CreatedAt, CancelledAt) وربطتها بـ `Items` (`List<ShopOrderItem>`)
- جدول جديد `ShopOrderItem` (Id, ShopOrderId, ProductId, Quantity, UnitPrice) — كل عنصر بيسجل سعر الوحدة وقت الشراء (Snapshot) عشان الأوردرات القديمة تفضل صح لو سعر المنتج اتغيّر بعدين
- `ShopController.RequestOtp/Confirm`: بقوا بيشتغلوا على كل عناصر السلة مرة واحدة (مش منتج واحد) — بيتحقق من المخزون لكل عنصر، وبيخصم المخزون لكل عنصر بعد نجاح الدفع، وبيفضّي السلة بالكامل بعد الشراء
- `AdminController.DeleteProduct`: اتصلح عشان يفحص وجود أوردرات مرتبطة عن طريق `ShopOrderItems` بدل `ShopOrder.ProductId` (اللي بقى مش موجود أصلاً)
- `MyOrders.cshtml`: بتعرض كل أوردر كـ Card فيه كل منتجاته + العنوان + رقم التليفون + تفاصيل الشحن والإجمالي

### ⚠️ نقطة مفتوحة
الـ Migration بتاعة التغيير ده (`ShopCheckoutRefactor`) بتشيل عمودين (`ProductId`, `Quantity`) من `ShopOrder` مباشرة. لو كان فيه بيانات أوردرات حقيقية (مش تجربة) قبل التعديل ده، لازم Data Migration لنقلها لـ `ShopOrderItem` قبل ما تتمسح، وإلا هتضيع.

---

## 🚚 Shop — تحديث تلقائي لتراك الشحن (Processing → Shipped → Delivered) ✅ خلصت بالكامل

> الطلب: بدل ما الأدمن يغيّر ShippingStatus يدوي كل مرة، الأوردر يتحرك لوحده بين المراحل الثلاثة —
> زي أمازون بالظبط: بعد 48 ساعة من تأكيد الدفع يطلع للشحن أوتوماتيك، وبعدها يوصل بعد عدد أيام
> بيختلف حسب محافظة العميل.

### الموديل
- `ShopOrder.cs`: 3 حقول جديدة —
  - `ConfirmedAt` (لحظة نجاح الدفع الفعلية، بتتسجل في `ShopController.Confirm` — مش نفس `CreatedAt`
    اللي بيتسجل وقت أول `RequestOtp` وممكن يكون أبكر بوقت طويل)
  - `ShippedAt` (لحظة الخروج للشحن فعليًا، سواء أوتوماتيك أو الأدمن غيّرها يدوي)
  - `DeliveredAt` (لحظة الوصول فعليًا — للعرض بس في `MyOrders`)
- Migration: `AddShopOrderTrackingTimestamps`
- `Governorates.cs`: عمود جديد `DeliveryDays` لكل محافظة (1 يوم القاهرة/الجيزة، لحد 5 أيام
  سيناء/أسوان/الوادي الجديد — نفس تدرّج `ShippingFee` الموجود أصلاً) + `Governorates.GetDeliveryDays(key)`

### السيرفيس الجديد
- `ShopOrderShippingBackgroundService.cs` ← IHostedService جديد، نفس فلسفة
  `ShopOrderRefundBackgroundService` بالظبط، بس بمرحلتين كل 10 دقايق:
  1. `Processing → Shipped`: أي أوردر `Confirmed` عدى عليه 48 ساعة من `ConfirmedAt`
  2. `Shipped → Delivered`: أي أوردر عدى عليه من `ShippedAt` عدد الأيام بتاع محافظته
     (`Governorates.GetDeliveryDays`)
- `Program.cs`: تسجيل السيرفيس الجديد كـ Hosted Service جنب نظيره بتاع الريفاند

### الكنترولرز
- `ShopController.Confirm`: بيسجل `order.ConfirmedAt = DateTime.Now` وقت نجاح الدفع مباشرة
- `AdminController.UpdateShopOrderShipping`: لو الأدمن غيّر الحالة يدويًا، بيسجل `ShippedAt`/`DeliveredAt`
  لو لسه فاضيين — عشان الحساب التلقائي بعد كده (خصوصًا Delivered) يفضل مبني على تاريخ حقيقي
  مش فاضي. الأدمن لسه يقدر يتحكم يدوي في أي وقت زي ما هو، مفيش تعارض مع الأوتوماتيك.

### الـ Views
- `MyOrders.cshtml`: رسالة "متوقع وصوله..." زي أمازون تحت تراك الشحن مباشرة، بتتحسب من جديد في
  كل تحميل صفحة (من غير أي كرون جوب إضافي) — بتفرق حسب الحالة:
  - `Processing` → "سيتم شحنه قريبًا"
  - `Shipped` → "متوقع وصوله اليوم/غدًا/خلال X أيام (تاريخ)" حسب الفرق بين تاريخ النهارده وموعد الوصول
  - `Delivered` → "وصل بتاريخ كذا"
  - *(ملحوظة بسيطة: حالة الـ "يومين بالظبط" بالعربي بتطلع "خلال 2 أيام" مش "يومين" — تبسيط متعمد)*

### مفاتيح ترجمة جديدة (لسه محتاجة تتضاف يدويًا في en.json/ar.json)
`Shop_Track_WillShipSoon`, `Shop_Track_ArrivesToday`, `Shop_Track_ArrivesTomorrow`,
`Shop_Track_ArrivesIn`, `Shop_Track_Days`, `Shop_Track_DeliveredOn`

### ⚠️ نقطة مفتوحة
الأوردرات القديمة الـ `Confirmed` (قبل الـ Migration) هتبقى `ConfirmedAt = null`، والسيرفيس بيتجاهلها
(منقدرش نحسب 48 ساعة من حاجة فاضية) فمش هتتشحن أوتوماتيك. لو محتاجين نضبطها، تنفيذ الـ SQL ده مرة واحدة بعد الـ Migration:
```sql
UPDATE ShopOrders SET ConfirmedAt = CreatedAt WHERE Status = 'Confirmed' AND ConfirmedAt IS NULL
```

---

## 🗂️ Admin Dashboard — فصل Shop Orders في تاب مستقل ✅ خلصت بالكامل

> الطلب: جدول أوردرات الشوب كان جوه تاب "Shop" نفسه (مع المنتجات والكاتيجوريز) — ده كان بيخلي
> التاب مزدحم ومعقد. اتفصل في تاب مستقل بنفس فكرة تاب Bookings.

- تاب جديد "📦 Shop Orders" في الـ Sidebar (تحت "Users & Data"، جنب Bookings مباشرة) بعدد الأوردرات
  (`Model.TotalShopOrders`)
- جدول الأوردرات اتنقل بالكامل من `panel-shop` لبانل مستقل `panel-shop-orders`، وضيف عليه عمود جديد
  **"Est. arrival"** (بنفس منطق الـ ETA بتاع `MyOrders.cshtml`: "Ships within 48h" / تاريخ الوصول
  المتوقع / "Delivered")
- تاب "Shop" الأصلي فضل بس للمنتجات + الكاتيجوريز (أبسط وأوضح)
- `panelTitles` (JS) اتضاف لها مفتاح `'shop-orders'`
- `AdminController.cs`: الـ Redirect بعد `ChangeShopOrderStatus` و `UpdateShopOrderShipping` بقى
  يوجّه لـ `tab = "shop-orders"` بدل `tab = "shop"` (كان باگ صغير هيخلي الأدمن يترمي في تاب
  المنتجات بعد أي تحديث حالة أوردر)
- Overview (أول صفحة بتفتح): كارت إحصائي جديد "📦 Shop Orders" + جدول "Recent Shop Orders"
  (آخر 8 أوردرات) بنفس شكل وهوية "Recent Bookings" الموجود جنبه، مع pill ملوّن لتراك الشحن
  (دهبي Processing / أزرق Shipped / أخضر Delivered) وزرار "View All" بيودي على التاب الجديد
- إصلاح صغير مصاحب: دالة `switchPanel` الخاصة بزرار "View All" (اللي بيتنادى من غير `this`) كانت
  مش هتقدر تعلّم على نav item الصح لأي تاب اسمه فيه شرطة زي `shop-orders` — اتصلحت

---
