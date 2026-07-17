# 🏺 Pharaohs Legacy — Project Progress & Ultimate Roadmap

> **الهدف:** تحويل Pharaohs Legacy من مشروع تخرج إلى منصة سياحية/ثقافية/ذكية متكاملة بمستوى Startup أو Enterprise Platform.

---

## 🛠️ Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core + SQL Server
- Session-based Authentication
- C# / Razor Views / JS / CSS

---

## 🗄️ Database Tables

| Table | Fields |
|---|---|
| Users | Id, Name, Email, Password |
| Pharaohs | Id, Name, Dynasty, Period, Description, ImageUrl |
| Temples | Id, Name, Location, Period, Description, ImageUrl, TicketUrl |
| Museums | Id, Name, Location, Founded, Description, ImageUrl, Category, Latitude, Longitude | *(WebsiteUrl مكتوب هنا قبل كده لكن اتأكد إنه **مش موجود فعليًا** في الداتا بيز الحقيقية — لو ضفته في أي INSERT هيدي `Msg 207: Invalid column name`)*
| Gods | Id, Name, Role, Symbol, Description, ImageUrl |
| Favorites | Id, UserEmail, Type, ItemId |
| Bookings | Id, UserEmail, PlaceType, PlaceId, PlaceName (NotMapped), VisitDate, NumberOfTickets, TotalPrice, Status, CreatedAt |
| Payments | Id, BookingId, Amount, PaymentDate, PaymentMethod, Status |
| Reviews | Id, UserEmail, UserName, Type, ItemId, Rating (1-5), Comment, CreatedAt, IsEdited |
| ReviewHelpfuls | Id, ReviewId, UserEmail |
| ReviewReports | Id, ReviewId, ReporterEmail, Reason, CreatedAt, IsResolved |
| Dynasties | Id, Name, Era, StartYear, EndYear, Description, Achievements, CapitalCity, ImageUrl, **PharaohTag** |
| HistoricalEvents | Id, Title, Year (int — سالب = BC), Category, Description, ImageUrl, DynastyTag (nullable), PharaohTag (nullable) |
| Artifacts | Id, Name, OriginLocation, Period, Type, Description, ImageUrl, MuseumName, MuseumLocation *(نص وصفي بس، مش FK حقيقي — أغلب القطع في متاحف عالمية برا نطاق جدول Museums)* |

> ✅ **جزء تكبير/توسيع الداتا بيز خلص بالكامل (كل الجداول أعلاه).**
> كل الجداول اتراجعت جدول جدول، الأعداد الحقيقية بعد المراجعة: **156 Pharaohs، 29 Temples، 42 Museums، 69 Gods، 58 Artifacts، 35 Dynasties، 56 Historical Events**.
> اتصلحت مشكلتين قبل الـ INSERT النهائي: (1) حذف 16 صف مكرر بالكامل في HistoricalEvents (كان فيه 72 صف، اتصلح لـ 56)، (2) تصحيح `PharaohTag` لحدث "Foundation of Memphis" من "Menes" (مش موجود في جدول Pharaohs) لـ "Narmer" (الاسم الصحيح المطابق). باقي كل الـ tags (Dynasty ↔ PharaohTag، DynastyTag، PharaohTag) اتأكد إنها متطابقة 100% في الاتجاهين، مفيش IDs مكررة ولا حقول فاضية مهمة.
> ملاحظة بسيطة اتأجلت (مش خطأ): 9 متاحف (Postal Museum, Railway Museum...) لسه من غير ImageUrl — هتتحل مع جزء رفع الصور.

---

## 📐 Models — Important Notes

```csharp
// Booking.cs
[NotMapped]
public string PlaceName { get; set; } = "";

[Column(TypeName = "decimal(18,2)")]
public decimal TotalPrice { get; set; }
```

---

## 🎮 Controllers

| Controller | Actions |
|---|---|
| UserController | Login (GET/POST), Register (GET/POST), Guest, Logout, Dashboard(string tab = "overview") — 🆕 Dashboard بيجيب رصيد البنك الحقيقي للعرض بس (قراءة، مش إنشاء حساب) |
| HomeController | Index (shows 3 pharaohs + 3 temples + 3 museums + 3 gods), Search, Timeline |
| PharaohController | Index, Details (with IsFav + Reviews) |
| TempleController | Index, Details (with IsFav + Book button + Reviews) |
| MuseumController | Index (Egyptian split), Details (with Book + Fav buttons + Reviews) |
| GodController | Index, Details (with IsFav + Reviews) |
| FavoriteController | Index, Add, Remove — يدعم: pharaoh / temple / god / museum |
| BookingController | Create (with PlaceImage), 🆕 RequestOtp (POST — بيحفظ الحجز مبدئيًا PendingPayment ويطلب OTP من البنك), Confirm (POST — بياخد bookingId + otpCode كمان دلوقتي), MyBookings (بيستبعد PendingPayment), Cancel (48hr rule + Refund تلقائي فوري)، ValidateCoupon |
| ReviewController | Add, Delete, DeleteAdmin, Edit, ToggleHelpful, GetHelpfulData, Report, ResolveReport |
| DynastyController | Index (grouped by Era), Details (with Pharaohs + Artifacts + Prev/Next nav) — فلترة الفراعنة بقت PharaohTag + مدى سنين الحكم (`ParsePharaohStartYear`) عشان تحل باگ الأسر الفرعية زي Amarna Period |
| HistoricalEventController | Index (filter by Category), Details (with Dynasty + Pharaoh + Related Events) |
| AdminController | Index (بيستبعد PendingPayment من الحجوزات والإحصائيات), AddPharaoh, EditPharaoh, DeletePharaoh, AddTemple, EditTemple, DeleteTemple, AddMuseum, EditMuseum, DeleteMuseum, AddGod, EditGod, DeleteGod, DeleteUser, 🆕 ChangeBookingStatus (بقى بينادي `/payments/refund` فعليًا لما الحالة تبقى Refunded — قيد تحديث لقواعد أكتر، شوف قسم البنك تحت), AddDynasty, EditDynasty, DeleteDynasty, AddHistoricalEvent, EditHistoricalEvent, DeleteHistoricalEvent |

---

## 📁 Views Structure

```
Views/
├── Shared/
│   ├── _Layout.cshtml
│   └── _Reviews.cshtml       ← Partial — يتضاف في كل Details page
├── User/
│   ├── Login.cshtml
│   └── Dashboard.cshtml
├── Home/
│   ├── Index.cshtml
│   ├── Search.cshtml
│   └── Timeline.cshtml
├── Pharaoh/
│   ├── Index.cshtml
│   └── Details.cshtml
├── Temple/
│   ├── Index.cshtml
│   └── Details.cshtml
├── Museum/
│   ├── Index.cshtml
│   └── Details.cshtml
├── God/
│   ├── Index.cshtml
│   └── Details.cshtml
├── Dynasty/
│   ├── Index.cshtml          ← Grid grouped by Era + Filter + Mini Timeline
│   └── Details.cshtml        ← Info + Pharaohs + Artifacts + Prev/Next nav + Historical Events
├── HistoricalEvent/
│   ├── Index.cshtml          ← Vertical Timeline + Category Filter
│   └── Details.cshtml        ← Hero + Description + Related Dynasty/Pharaoh + Related Events
├── Favorite/
│   └── Index.cshtml
├── Booking/
│   ├── Create.cshtml
│   └── MyBookings.cshtml
└── Admin/
    └── Index.cshtml
```

---

## ✅ Features Done

- Login / Register / Guest access
- Session-based auth
- Form validation (JS + C#)
- Password strength bar + show/hide + confirm
- Egyptian-themed UI (dark gold theme)
- Responsive design + hamburger menu
- Scroll reveal + back to top + stats counter animation
- Broken image fallback
- 156 Pharaohs + 29 Temples + 42 Museums + 69 Gods + 58 Artifacts + 56 Historical Events (الأعداد اتحدثت بعد مراجعة كاملة للداتا بيز — تفاصيل تحت في "Database Enrichment"، "Artifacts Cleanup + Enrichment"، "Gods Enrichment"، و"Historical Events Enrichment")
- Search across pharaohs + temples
- Favorites system (4 أنواع)
- Booking system + 48hr cancel
- Payment records
- MyBookings (countdown timer + tracker)
- User Dashboard (4 tabs)
- Admin Dashboard (CRUD كامل)
- Interactive Map (Temples + Museums) + Admin Map Picker
- Timeline page
- ERD
- Hieroglyphics Translator page (Unicode font + Canvas download)
- AI Tour Guide Chatbot — floating widget في كل الصفحات (Groq + LLaMA 3.1)
- Timeline — Dynasty grouping + Filter buttons
- Artifacts — Model + Controller + Views (Index + Details) + Admin CRUD + 15 artifact في الـ DB
- Rating + Comments ✅ (مكتمل بالكامل)
- Dynasties Page ✅ (مكتمل بالكامل)
- Historical Events ✅ (مكتمل بالكامل)
- Multi-language (عربي/إنجليزي) ✅ (مكتمل بالكامل)

---

## ⚠️ Key Rules (مهم جداً)

- `@@keyframes` مش `@keyframes` في Razor CSS
- مش ممكن توصل للداتا بيز من الـ View مباشرة
- Session key للـ email = `"UserEmail"`
- Session key للـ role = `"UserRole"` (قيمته `"Admin"` أو `"User"`)
- مفيش session key للاسم — بيتجيب من الـ DB بـ `_context.Users.FirstOrDefault(u => u.Email == email)`
- `[NotMapped]` = field مش في الداتا بيز
- `[Column(TypeName = "decimal(18,2)")]` = لازم على كل decimal
- لو Migration موجودة بنفس الاسم — غير الاسم
- Namespace المشروع = `pharaohsLegacy` (p صغيرة)
- `AppDbContext` موجود في namespace `pharaohsLegacy.Models` مش `pharaohsLegacy.Data`
- `fav.Type.ToLower()` عشان الـ Favorites تشتغل صح
- الأدمن بيتعرف بـ email ثابت في AdminController و UserController
- Admin buttons في الـ sidebar بيستخدموا `class="adm-nav-item"` + `onclick="switchPanel('tab',this)"` مش `data-tab`
- الـ `PharaohTag` في Dynasty لازم يطابق بالظبط الـ `Dynasty` field في جدول Pharaohs (مثال: `"18th Dynasty"`)

---

## 🔑 Admin Email

```
kamalabdlbast89@gmail.com
```

---

## ⚙️ Program.cs Setup

```csharp
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");
```

---

---

# 📋 Completed Features — تفاصيل كاملة

---

## ✅ Rating + Comments (مكتمل بالكامل)

### اللي خلص
- Model: `Review.cs` — Id, UserEmail, UserName, Type, ItemId, Rating, Comment, CreatedAt, **IsEdited**
- Model: `ReviewHelpful.cs` — Id, ReviewId, UserEmail
- Model: `ReviewReport.cs` — Id, ReviewId, ReporterEmail, Reason, CreatedAt, IsResolved
- Migration: `AddReviews` + `AddReviewExtensions` ✅
- `ReviewController` →
  - `Add` (POST)
  - `Delete` (POST)
  - `DeleteAdmin` (POST) — بيحذف الـ Helpfuls والـ Reports المرتبطة
  - `Edit` (POST) — يعدل Rating + Comment + يحط IsEdited = true
  - `ToggleHelpful` (POST) — toggle voted/unvoted
  - `GetHelpfulData` (GET) — بيرجع counts + userVotes بـ AJAX
  - `Report` (POST) — بيحفظ البلاغ في ReviewReports
  - `ResolveReport` (POST) — الأدمن يحل البلاغ
- `_Reviews.cshtml` — Partial View بـ:
  - Star Rating bar (average + distribution)
  - Star Picker تفاعلي
  - Character counter (500 حرف)
  - Reviews list مع avatar + date + stars
  - **(edited)** badge لو الـ review اتعدلت
  - Filter by Rating — أزرار فوق الـ reviews list
  - **Edit Review** — inline form بـ star picker + textarea
  - **Helpful Button** 👍 — toggle + count بيتحدث لحظياً
  - **Report Button** 🚩 — modal بـ 4 أسباب جاهزة + حقل حر
  - بعد Edit بيعمل reload عشان الـ Summary Bar يتحدث
- ضُيّف في Details pages: Pharaoh / Temple / Museum / God / Artifact
- Admin Dashboard:
  - Reviews tab في الـ Sidebar
  - **Reports tab** — بيعرض كل البلاغات مع Reporter / Review / Reason / Date / Status
  - TotalReviews في الـ Overview stats
  - جدول كل الـ Reviews مع Delete بـ confirm modal
  - Filter by Rating + Type + Date في الـ Admin Reviews panel
  - Resolve Report — الأدمن يضغط ✅ Resolve بدون reload
  - Delete Review من الـ Reports tab
- **Verified Visitor Badge** ✅ — يظهر "✅ Visited" جنب اسم اليوزر لو حجز المكان
- القواعد المطبّقة:
  - Guest مش يقدر يكتب/يعدل/يعمل helpful/يعمل report
  - Admin مش يقدر يكتب review — بيشوف "Admins cannot write reviews"
  - كل يوزر يكتب review واحدة بس على كل item
  - كل يوزر يبلّغ مرة واحدة بس على كل review
  - الاسم بيتجيب من الـ DB مش من الـ Session
  - `isGuest = string.IsNullOrEmpty(email) || email == "guest"`

---

## ✅ Dynasties Page (مكتمل بالكامل)

### اللي خلص
- Model: `Dynasty.cs` — Id, Name, Era, StartYear, EndYear, Description, Achievements, CapitalCity, ImageUrl, **PharaohTag**
- Migration: `AddDynasties` + `AddDynastyPharaohTag` ✅
- `DynastyController` →
  - `Index` — بيجيب كل الـ dynasties مرتبة بـ StartYear + grouped by Era → `Dictionary<string, List<Dynasty>>`
  - `Details` — بيجيب الـ dynasty + الفراعنة المرتبطين (عن طريق `PharaohTag`) + الآثار + Prev/Next dynasty
- Views:
  - `Index.cshtml` — Grid Cards grouped by Era + Filter buttons + Mini Timeline proportional + Era Legend
  - `Details.cshtml` — Hero + Key Facts + Achievements + Pharaohs grid + Artifacts grid + Prev/Next nav
- Static files: `wwwroot/css/dynasty.css` + `wwwroot/js/dynasty.js`
- Admin Dashboard: Dynasties tab + `panel-dynasties` + `modalAddDynasty` + `modalEditDynasty` + `openEditDynastyBtn(btn)`
- AdminOverviewViewModel: TotalDynasties + Dynasties
- Navbar: `<li><a asp-controller="Dynasty" asp-action="Index">𓂀 Dynasties</a></li>`
- Data: 14 dynasty في الـ DB تغطي Early Dynastic → Ptolemaic

### Key Rules — Dynasties
- `PharaohTag` لازم يطابق بالظبط الـ `Dynasty` field في جدول Pharaohs
  - مثال: Dynasty Name = `"Eighteenth Dynasty"` → PharaohTag = `"18th Dynasty"`
- StartYear و EndYear: سالب = BC (مثال: `-3100` = 3100 BC)
- الفراعنة والآثار بيظهروا تلقائي — مفيش حاجة تعملها

---

## ✅ Historical Events (مكتمل بالكامل)

### اللي خلص
- Model: `HistoricalEvent.cs` — Id, Title, Year (int — سالب = BC), Category, Description, ImageUrl, DynastyTag (nullable), PharaohTag (nullable)
- Helper property: `YearLabel` → بيرجع `"3100 BC"` أو `"30 AD"` تلقائي
- Migration: `AddHistoricalEvents` ✅
- `HistoricalEventController` →
  - `Index` — بيجيب كل الـ events مرتبة بـ Year + filter by Category
  - `Details` — بيجيب الـ event + Dynasty المرتبطة + Pharaoh المرتبط + Related Events
- Views:
  - `Index.cshtml` — Vertical Timeline + Category Filter buttons + Scroll reveal animation
  - `Details.cshtml` — Hero + Description + Related Dynasty card + Related Pharaoh card + Related Events grid
- Static files: `wwwroot/css/events.css` + `wwwroot/js/events.js`
- Integration: `Dynasty/Details.cshtml` + `Pharaoh/Details.cshtml` عبر `ViewBag.HistoricalEvents`
- Admin Dashboard: Events tab + `panel-events` + `modalAddEvent` + `modalEditEvent` + `openEditEvent(btn)`
- Data: 20 حدث في الـ DB تغطي 3200 BC → 30 BC
- Navbar: `<li><a asp-controller="HistoricalEvent" asp-action="Index">𓋹 Events</a></li>`

### Key Rules — Historical Events
- `DynastyTag` لازم يطابق بالظبط الـ `Name` field في جدول Dynasties
- `PharaohTag` لازم يطابق بالظبط الـ `Name` field في جدول Pharaohs
- Year: سالب = BC (مثال: `-1274` = 1274 BC)
- Categories المتاحة: `Political` / `Military` / `Religious` / `Cultural` / `Scientific`
- الـ `events.css` بيتلينك في `Pharaoh/Details.cshtml` — الـ `Dynasty/Details.cshtml` بيستخدم `<style>` tag مباشرة بسبب CSS conflict مع `dynasty.css`

---

## ✅ My Journey Tab

- Tab جديد في User Dashboard
- Map بيعرض الـ temples والـ museums (Booked / Favourite / Both / Visited)
- Pins بألوان مختلفة (Gold / Red / Purple / Green)
- Cards تحت الـ Map بتفاصيل كل مكان
- Empty state لو مفيش حاجة

---

## ✅ Visited Status

- `BookingStatusUpdater` — Background Service كل ساعة يغير Confirmed لـ Visited أوتوماتيك
- فلتر Visited في User Dashboard Bookings tab
- فلتر Visited في Admin Dashboard Bookings
- Places Visited counter في الـ Overview stats (5 stats دلوقتي)
- Explorer Badge في الـ Profile (Explorer / Temple Master / Grand Explorer)
- Visited row في Account Details
- Visited pin أخضر على الـ Journey Map

---

## ✅ Maps & Location (مكتمل بالكامل)

- Interactive Map page (Temples + Museums) — Leaflet.js dark theme
- Filter buttons (All / Temples / Museums)
- Popup cards بصورة + وصف + View Details button
- Map Picker في Admin Dashboard:
  - Add Temple → اضغط على الخريطة يتملي Lat/Lng أوتوماتيك ✅
  - Edit Temple → بيفتح على الموقع الحالي + Lat/Lng متملية ✅
  - Add Museum → اضغط على الخريطة يتملي Lat/Lng أوتوماتيك ✅
  - Edit Museum → بيفتح على الموقع الحالي + Lat/Lng متملية ✅

---

## ✅ Hieroglyphics Translator

- Unicode Egyptian Hieroglyphs — Noto Sans Egyptian Hieroglyphs font
- Input مع character counter (20 حرف max)
- Output بـ animation لكل رمز
- Download — Canvas API مباشرة (مش html2canvas) عشان الـ font يتحمّل صح
- Copy Text للـ clipboard
- Alphabet Reference تفاعلي — اضغط على أي حرف يتضاف للـ input
- Toast notifications
- Controller: `HieroglyphicsController` → `Translator()`

---

## ✅ AI Tour Guide Chatbot

- Floating widget في كل الصفحات عبر `_Layout.cshtml`
- Powered by **Groq API** + **LLaMA 3.1 8B Instant** (مجاني)
- System Prompt قوي — بيلعب دور AI Tour Guide متخصص في الحضارة المصرية
- مش بيكشف إنه Groq/LLaMA — شخصية الموقع بس
- Suggestion chips تختفي بعد أول رسالة
- Typing animation (3 dots) + Auto-resize textarea
- Expand button لتكبير الـ window + Pulse animation
- AI GUIDE label + Responsive على الموبايل
- Controller: `ChatbotController` → `Ask()` (POST)
- Key Rules:
  - `builder.Services.AddHttpClient()` في `Program.cs`
  - API Key في `appsettings.json` تحت `"GroqApiKey"`
  - Model: `llama-3.1-8b-instant`

---

## ✅ Timeline

- Dynasty grouping — header لكل أسرة
- Filter buttons — All + كل dynasty
- Controller: `HomeController` → `Timeline()` يرجع `Dictionary<string, List<Pharaoh>>`

---

## ✅ Artifacts

- Model: Id, Name, Origin, Period, Category, Description, ImageUrl, Museum, CurrentLocation
- Migration: `AddArtifacts` ✅
- Controller: `ArtifactController` → Index + Details
- Views: Index (filter by category) + Details (meta grid + favorites)
- Admin CRUD: AddArtifact / EditArtifact / DeleteArtifact
- AdminOverviewViewModel: TotalArtifacts + Artifacts
- Navbar: رابط Artifacts مضاف
- Data: 58 artifact في الـ DB (بدأت بـ15، اتنضّفت من تكرار دفعة إضافة سابقة، واتضاف عليها 20 قطعة حقيقية جديدة — تفاصيل في "Artifacts Cleanup + Enrichment")
- Favorites: يدعم type = "artifact"
- Key: الـ modals بتستخدم `adm-overlay` مش `adm-modal` كـ wrapper

---

## ✅ Multi-language (عربي/إنجليزي) (مكتمل بالكامل)

### اللي خلص
- Session key للغة = `"Lang"` (قيمته `"ar"` أو `"en"`, default = `"ar"`)
- `Services/LocalizationService.cs` — بيقرا `wwwroot/lang/ar.json` + `en.json` ويرجع الترجمة حسب اللغة
- `Html.L("Key")` Helper — لكل نصوص الـ UI الثابتة (labels/buttons/nav). لو المفتاح مش موجود بيرجع نفسه كـ fallback
- `Html.D(arabicValue, englishValue)` Helper — لعرض بيانات الداتا بيز، بيرجع العربي لو موجود وإلا الإنجليزي كـ fallback
- **كل صفحات الـ Views اتترجمت** (Navbar, Footer, Home, User, Pharaoh, Temple, Museum, God, Dynasty, HistoricalEvent, Favorite, Booking, Reviews, Translator, Chatbot, Artifact, Map, Search, Timeline, Register... كل الصفحات)
- **7 جداول في الداتا بيز** (`Pharaohs`, `Temples`, `Museums`, `Gods`, `Dynasties`, `HistoricalEvents`, `Artifacts`) عندها أعمدة عربي جديدة (`NameAr`, `DescriptionAr`... إلخ) ومتعبية بالكامل بالترجمة الفعلية عن طريق SQL UPDATE
- **كل الـ Views (Index + Details) للجداول السبعة** بتعرض الترجمة العربية فعليًا (Gods, Artifacts, Temples, Museums, Pharaohs, Dynasties, HistoricalEvents)
- **Admin CRUD Forms** — قسم "🇪🇬 Arabic Translation" في كل فورم Add/Edit للجداول السبعة (View + Controller Actions بتستقبل وتحفظ الحقول ✅)
- **الأرقام العربية-الهندية (٠-٩)** — خلصت في كل الصفحات (Countdown Timer, Summary Bar, تواريخ الحجوزات...)

### Key Rules — Multi-language
- الترجمة UI بس — البيانات القادمة من الداتا بيز بتتعرض عن طريق `Html.D` مش `Html.L`
- حالات ديناميكية (زي `Status` بتاع Booking) بتترجم بـ `@Html.L("Status_" + b.Status)` — لازم قيم الـ DB تطابق أسماء المفاتيح بالظبط
- كل عمود عربي في الداتا بيز nullable (`string?`) — لو فاضي، الـ Fallback يرجع النسخة الإنجليزية
- مفيش `_ViewImports.cshtml` في المشروع — أي View جديدة لازم `@using pharaohsLegacy.Extensions` في أولها
- الـ `data-category`/`data-era`/`data-name` attributes المستخدمة في الـ JS للفلترة والبحث اتسابت إنجليزي عمدًا عشان الفلترة تفضل شغالة
- تنسيق التاريخ لسه إنجليزي (أسماء الشهور) — Culture-aware dates برة نطاق مرحلة الترجمة الحالية، بند مستقبلي لو احتجناه

---

## ✅ Database Enrichment — توسيع قاعدة البيانات (مكتمل)

### اللي خلص

**جدول Pharaohs — من 249 (فيه تكرار) لـ 137 فرعون فريد**
- الداتا بيز كانت فيها **249 صف لكن 112 منهم تكرار فعلي** (92 اسم فرعون متكرر 2 أو 3 مرات، بسبب دفعات استيراد سابقة اتضافت فوق بعضها). اتعمل تنظيف شامل:
  - **Dedup:** لكل مجموعة تكرار، اتسابت نسخة واحدة بس (بالأولوية: فترة حكم دقيقة "من-إلى" مش تقريبية "c. ..."، وصورة `https://` حقيقية بدل مسار محلي)
  - **Narmer تحديدًا:** كان عنده نسخة تحت "Pre-Dynastic" ونسختين تحت "1st Dynasty" — اتحسم لصالح "1st Dynasty" (التصنيف الأشهر والأدق)
  - **قبل أي حذف:** أي `Favorites`/`Reviews` (Type='pharaoh') على النسخة المحذوفة اتعاد ربطها للنسخة الباقية أولًا، عشان محدش يخسر مفضلة أو تقييم
  - توحيد تسمية `"Ptolemaic Dynasty"` → `"Ptolemaic"` (يطابق الـ `PharaohTag` الحقيقي في Dynasties — كان فيه نص الفراعنة البطالمة أصلاً مش بيظهروا في صفحة أسرتهم بسبب الفرق ده)
  - تصحيح خطأ مطبعي قديم في فترة حكم Ahmose I (`1550–1525 BC222` → `1550–1525 BC`) كان لسه موجود في النسخة الأصلية رغم إنه اتصلح قبل كده في نسخة مكررة اتحذفت

**جدول Dynasties — من 30 صف (فيهم تكرار) لـ 30 سجل نضيف**
- حذف سجل "28th, 29th & 30th Dynasties" (Id=20) المدمج القديم اللي كان المفروض يتحذف وقت ما اتقسم لـ 3 سجلات منفصلة، لكنه فضل موجود بيكرر نفس الأسر
- **إضافة سجل "Pre-Dynastic" جديد بالكامل** — كان مفقود تمامًا من Dynasties رغم وجود فراعنة (`Scorpion II`, `Ka (Sekhen)`) بـ `Dynasty = "Pre-Dynastic"` بيشاوروا على أسرة مش موجودة أصلاً

**جدول Museums — من 10 لـ 33 متحف**
- 23 متحف حقيقي وموثّق اتضافوا (Mummification Museum, Gayer-Anderson, Manial Palace, Royal Jewelry, Agricultural, Egyptian Military, Bibliotheca Alexandrina Antiquities, Mohamed Mahmoud Khalil, Sharm El Sheikh, Hurghada, Aswan, Ismailia, Mallawi, Tal Basta, Kharga, El-Alamein War, Alexandria Fine Arts, Egyptian Geological, Abdeen Palace, Gamal Abdel Nasser, Karanis Site, Port Said Modern Art, Islamic Ceramics)
- ⚠️ لسه فيه ~10-15 متحف صغير/تخصصي جدًا (متحف البريد، السكة الحديد، بيت السحيمي، قصر بشتاك...) متأجّلين — مش موثّقين بثقة كافية لسه

**جدول Temples — من 14 لـ 29 معلم**
- 15 معلم اتضافوا (Great Sphinx of Giza, Pyramid of Khafre, Pyramid of Menkaure, Ramesseum, Bent Pyramid, Red Pyramid, Pyramid of Unas, Seti I Temple at Qurna, Deir el-Medina, Kalabsha, Beit el-Wali, Siwa Oracle Temple, Wadi es-Sebua, Amada, Hibis Temple)
- ملحوظة: جدول "Temples" فعليًا بيضم معالم أوسع من المعابد بس (أهرامات، تماثيل) — مش خطأ، مقصود كده من الأول

### 🐛 باگ Amarna Period — **اتحل ✅**
كان `DynastyController.Details` بيجيب الفراعنة بمطابقة `PharaohTag` بس، فصفحة "Amarna Period" كانت بتعرض كل الـ35 فرعون من الأسرة 18 مش بس الأربعة بتوع العمارنة. الحل: ضيف method `ParsePharaohStartYear(string period)` بيستخرج أول سنة من نص `Period` (بيتعامل مع `"c."` و BC/AD)، وبعد فلترة الـ `PharaohTag` بيتعمل فلترة تانية بمقارنة السنة دي بمدى `dynasty.StartYear`/`EndYear`. الحل عام مش مخصوص لـ Amarna بس — أي أسرة فرعية تانية تتضاف بنفس المبدأ هتتفلتر صح تلقائيًا.

### Key Rules — Database Enrichment
- أي إضافة فراعنة/أسر/متاحف/معابد جديدة بـ SQL **من غير تحديد Id** — سايبينها لـ IDENTITY
- خطوة الأمان الأساسية قبل أي `INSERT` نهائي: `SELECT Name FROM <الجدول> WHERE Name IN (...)` (لازم يطلع 0 صفوف) — **لازم تتشاف نتيجتها فعليًا قبل ما تكمل**، مش مجرد موجودة في الاسكريبت
- **الصور:** أغلب الفراعنة/الأسر/المتاحف/المعابد الأقل شهرة لسه `ImageUrl` = مسار محلي (`/images/.../name.jpg`) **مش موجود فعليًا في `wwwroot/`** — الـ Broken Image Fallback بيغطي عليها لحد ما تتحمل صور حقيقية
- بعض أسر الفراعنة (زي 9th-10th Dynasty، Macedonian، Roman Period) عندها سجلات في Dynasties لكن مفيش فراعنة مربوطين بيها لسه — جاهزة لإضافات مستقبلية

### ⚠️ درس مهم جدًا عن تنفيذ SQL — اتعلمناه بالطريقة الصعبة
- **الـ Transaction لازم يتعمله `COMMIT TRANSACTION;` فعلي (السطر متعلّق بـ `--` أو لأ)** — لو سبته من غير Commit وقفلت الـ Query Window، SQL Server بيعمل Rollback تلقائي وكل حاجة ترجع زي ما كانت، حتى لو شفت نتايج الـ SELECT شكلها تمام
- الشوفان جوه نفس الـ Connection بتاعتك بيوريك التعديلات حتى لو لسه مش متعمولها Commit (Uncommitted) — ده اللي بيلخبط، لأنه بيدّيك إحساس كاذب إن كل حاجة اتسجّلت
- **متشغّلش نفس اسكريبت الإضافة مرتين** — لو مش متأكد اتنفذ قبل كده، شغّل `SELECT COUNT(*) FROM <الجدول>` الأول قبل أي حاجة
- مشروع Visual Studio (Rebuild/Run) **مالوش أي علاقة** بتنفيذ ملفات .sql — الاسكريبتات لازم تتنفذ من SSMS (أو أي أداة SQL) بشكل منفصل تمامًا
- **أي نص عربي في `INSERT`/`UPDATE` لازم يتكتب `N'نص عربي'` مش `'نص عربي'` عادي** — لو نسيت الـ`N`، SQL Server بيحول الحروف العربية لعلامات استفهام `؟؟؟؟؟` في أعمدة `NVARCHAR` (حصلت فعليًا مع دفعة إضافة أول 14 إله جديد، اتصلحت بـ`DELETE` + إعادة `INSERT` بصيغة `N'...'` صح)

---

## ✅ Artifacts Cleanup + Enrichment (مكتمل)

### اللي خلص

**تنظيف التكرار — من 40 صف لـ 38 قطعة فريدة**
- اكتُشف إن دفعة إضافة سابقة اتضافت مرتين بالكامل، فكل الـ20 قطعة اللي كانت مضافة وقتها كان ليها نسخة مطابقة 100% (نفس Period وImageUrl وDescription)
- اتعمل فحص شامل بسكريبت SELECT-only الأول (عدد كلي + تكرار بالاسم + تكرار بالكتابة المتقاربة + الصفوف كاملة + فحص Favorites/Reviews المرتبطة)
- النتيجة: **مفيش أي Favorites/Reviews مرتبطة** بأي نسخة مكررة (`FavoritesLinked = ReviewsLinked = 0`)، فمكانش محتاج خطوة إعادة ربط
- اتحذفت الـ20 نسخة صاحبة الـ Id الأكبر في كل زوج (جوه Transaction + COMMIT فعلي)، وفضلت الـ38 نسخة الأصلية

**إضافة 20 قطعة أثرية حقيقية جديدة — من 38 لـ 58 قطعة**
- اتضافت 20 أثر موثّق (مش وهمي): Narmer Palette, Golden Death Mask of Tutankhamun, Statue of Khafre Enthroned, Meidum Geese, Golden Throne of Tutankhamun, Canopic Chest of Tutankhamun, Fowling in the Marshes (Tomb of Nebamun), Statue of Amenhotep Son of Hapu, Merneptah Stele, Inner Coffin of Henettawy, Golden Mask of Psusennes I, Statue of Djoser, Seated Statue of Hatshepsut, Great Papyrus Harris I, Ka Statue of King Hor, Turin Satirical Papyrus, Sphinx of Amenemhat III, Fayum Mummy Portrait of a Young Woman, Two Dogs Palette, Ivory Statuette of Khufu
- كل قطعة بتفاصيلها الإنجليزي والعربي كاملة (NameAr, OriginAr, PeriodAr, CategoryAr, DescriptionAr, MuseumAr, CurrentLocationAr)
- **فئات (Category) جديدة اتضافت** للتنوع: `Palette`, `Mask`, `Painting`, `Furniture`, `Coffin` (زيادة على الموجود: Statue, Papyrus, Obelisk, Jewelry, Sarcophagus, Stele, Relief, Temple, Ceremonial Object, Cuneiform Tablets)
- خطوة أمان قبل الـ INSERT: `SELECT Name FROM Artifacts WHERE Name IN (...)` رجّع 0 صف زي المتفق عليه

### Key Rules — Artifacts
- **عمود `ImageUrl` في جدول Artifacts مش NULLABLE** — لازم string فاضي `''` مش `NULL` لو مفيش صورة حقيقية لسه (اتحسبت غلطة `Msg 515` بسبب استخدام NULL غلط)
- قناع/عرش/مقصورة توت عنخ آمون الـ`CurrentLocation` بتاعهم اتسجل **"Grand Egyptian Museum, Giza"** مش "Egyptian Museum, Cairo" — لأنها اتنقلت فعليًا للمتحف المصري الكبير مؤخرًا
- الـ20 قطعة الجديدة الـ`ImageUrl` بتاعهم فاضي عمدًا — لسه محتاجين صور حقيقية تتضاف بعدين (زي باقي القطع الأقل شهرة في الجداول التانية)

---

## ✅ Gods Enrichment — من 55 لـ69 إله (مكتمل)

### اللي خلص
- اتضاف **14 إله حقيقي موثّق** مش موجودين قبل كده، اتراجعوا اسم اسم ضد `SELECT Name FROM Gods` قبل الإضافة (مفيش تكرار): `Ammit`, `Seshat`, `Anuket`, `Satet`, `Sopdu`, `Pakhet`, `Babi`, `Banebdjedet`, `Qebhet`, `Menhit`, `Aker`, `Ihy`, `Nehebkau`, `Shai`
- كل إله بتفاصيله الكاملة إنجليزي وعربي (Role, Description, Symbol + النسخ العربي)
- **حادثة الترميز:** الدفعة الأولى اتضافت من غير بادئة `N` قبل النصوص العربية، فاتسجلت الأعمدة العربية كـ`؟؟؟؟؟` بدل النص الصحيح. اتصلحت بـ`DELETE` للـ14 صف المتضررة (Id 156-169) وإعادة `INSERT` بصيغة `N'...'` صح (Id الجديدة بقت 170-183)
- **ملحوظة تستاهل مراجعة لاحقًا:** فيه فرعونين بإملاء مختلف لنفس الشخصية في جدول Pharaohs — `Id=19 "Merenptah"` و`Id=171 "Merneptah"` (نفس الأسرة 19th Dynasty) — يمكن يكونوا تكرار قديم زي حالة Narmer، لسه ماتصلحش

### Key Rules — Gods Enrichment
- نفس خطوة الأمان المعتادة: `SELECT Name FROM Gods WHERE Name IN (...)` لازم يرجع 0 صفوف قبل أي `INSERT`
- الصور (`ImageUrl`) للـ14 إله الجدد سايبينها `/images/gods/xxx.jpg` (مش موجودة فعليًا لسه) — الـ Broken Image Fallback بيغطيها

---

## ✅ Historical Events Enrichment — من 20 لـ40 حدث (مكتمل)

### اللي خلص
- اتضاف **20 حدث تاريخي جديد** بيغطوا فجوات كانت واضحة في التغطية (الدولة الوسطى، عصر الانتقال التاني، العصر المتأخر، العصر البطلمي، العصر الروماني) — المدى الزمني اتوسع من 3200 ق.م لحد 130 م
- كل `DynastyTag`/`PharaohTag` اتراجع حرفيًا ضد `SELECT Name FROM Dynasties` و`SELECT Name, Dynasty FROM Pharaohs` الفعليين قبل الإضافة، مش افتراض
- **تأكيد مهم:** اتأكدنا إن `DynastyTag` فعليًا بيطابق عمود `Name` في جدول `Dynasties` بصيغته الرقمية (`"18th Dynasty"`, `"Ptolemaic Dynasty"`...) — المثال القديم في الـ Key Rules (`"Eighteenth Dynasty"`) كان غير دقيق ومحدش يعتمد عليه، البيانات الفعلية والـ20 حدث الأصليين سليمين

### Key Rules — Historical Events Enrichment
- نفس خطوة الأمان: `SELECT Title FROM HistoricalEvents WHERE Title IN (...)` لازم يرجع 0 صفوف قبل أي `INSERT`
- الصور (`ImageUrl`) للـ20 حدث الجدد سايبينها `/images/events/xxx.jpg` (مش موجودة فعليًا لسه)

---

## 🌗 Dark / Light Mode System (✅ خلص بالكامل)

### الهدف
مش "إضافة داك مود" — الموقع أصلاً داكن دايمًا (Dark Gold Theme هو الافتراضي). الهدف الحقيقي: **إضافة Light Mode كخيار بديل** مع زرار Toggle يبدّل بينهم، من غير ما يبوظ حاجة شغالة.

### ⚠️ السبب اللي كان بيبوظ التصميم قبل كده (اتصلح)
كان فيه `:root { --gold: ...; --dark: ...; }` متعرّف في `_Layout.cshtml`، بس **0 مكان** في الملف كان بيستخدم `var(--gold)` فعليًا — كل الألوان كانت مكتوبة مباشرة كـ hex (`#c9a227`, `#0d0702`...) في كل قاعدة CSS. يعني أي تعديل على الـ variables مكانش بيغيّر أي حاجة على الشاشة. **الدرس:** أي لون تضيفه بعد كده لازم يتكتب `var(--اسم-المتغير)` مش hex مباشر، وإلا الـ Toggle مش هيشتغل عليه.

### 🏗️ الـ Architecture
- **CSS Variables** في `:root` (الوضع الدهبي الافتراضي) + `html[data-theme="light"] { ... }` (override كامل للفاتح) — الاتنين متعرّفين في `_Layout.cshtml` بس، وأي View تاني بيستخدم نفس المتغيرات دي مباشرة (مش محتاج يعرّفها تاني)
- المتغيرات: `--gold`, `--gold-rgb`, `--dark`, `--dark-rgb`, `--dark2`, `--dark3`, `--border`, `--border-rgb`, `--text`, `--muted`, `--surface-tint`, `--surface-tint-strong`, `--well-bg`, `--card-shadow`, `--card-shadow-hover`, `--gold-light` ⚠️, `--gold-dark` ⚠️
- الزرار (☀️/🌙) بيحط/يشيل `data-theme="light"` على `<html>` + بيحفظ الاختيار في `localStorage` بمفتاح اسمه **`plTheme`**
- فيه `<script>` صغير في أول `<head>` (قبل أي CSS) بيقرأ `localStorage` ويحط الـ attribute بدري، عشان الصفحة متفتحش بلون غلط لحظة واحدة قبل ما JS يلحق (مشكلة اسمها الـ "Flash")
- الـ Header **مش ثابت لونه** — بيتغير مع باقي الموقع لما تدوس الزرار (ده قرار اتاخد بالتحديد، مش هيتفاجئ حد إنه بيتغير)
- **قاعدة مهمة لأي View جديد هيتحول:** لو لقيت لون بيتحط بشفافية (`rgba(hex, x)`) ومحتاج نسخة `--*-rgb` مش موجودة، ضيفها في **الاتنين** (`:root` والـ `light` override) جوه `_Layout.cshtml` — زي ما حصل مع `--border-rgb` (`61,42,21` دهبي / `220,199,143` فاتح) لما احتجناها في `.det-info-row` بتاعة صفحة الـ Artifacts
- **⚠️ لو صفحة قديمة عندها `:root` محلي خاص بيها (زي ما كان في `MyBookings.cshtml`):** افحص أسامي المتغيرات كويس قبل ما تمسحه. لو فيه اسم بيتصادم مع اسم موجود بالفعل في النظام العام بس بقيمة مختلفة (مثال: `--border` المحلي كان `rgba(201,168,76,0.18)` بردي شفاف، لكن `--border` العام هو `#3d2a15` بني صلب) — **متسبوش الاسم يتصادم**. استبدله بقيمة صريحة (`rgba(var(--gold-rgb), x)`) بدل ما تعتمد على تعريف عام تاني. أما لو الاسم متطابق فعليًا في الغرض والدور (زي `--gold`) سيبه يوصل للتعريف العام عادي.
- **🐛 قاعدة مهمة جدًا اتكشفت متأخر (لازم تتراعى من الأول في أي صفحة جديدة):** فيه فرق بين نوعين من المتغيرات:
  - **متغيرات سطح** (`--dark`, `--dark2`, `--dark3`) — **بتتقلب** بين الوضعين (غامق في الدارك، فاتح تمامًا في اللايت — مثلاً `--dark` بيتحول من `#0d0702` لـ `#f3e8d0`)
  - **متغيرات تمييز** (`--gold`, `--gold-light`, `--gold-dark`) — بتفضل دهبي في الاتنين، بس بدرجة مختلفة، **مش بتتحول لفاتح تمامًا**

  **المشكلة:** أي نص وظيفته إنه يبان فوق خلفية دهبي دايمًا (زرار Send، فقاعة شات المستخدم، `.filter-btn.active`, `.btn-explore`) — لو استخدمت `var(--dark)` أو أي متغير من عيلة الـ surface كلون للنص، هيبقى شغال في الدارك مود بس في اللايت مود النص هيتحول لفاتح فوق خلفية دهبي وهيقل وضوحه بشكل واضح.
  **الحل:** النص في الحالة دي لازم يفضل **hex ثابت غير متغير** (استخدمنا `#150f05`)، بالظبط زي معاملة `#e74c3c` (أحمر Favorite) كلون دلالي مش جزء من نظام السطح المتقلب.
  **الأماكن اللي كانت متأثرة واتصلحت:** `.filter-btn.active` و`.btn-explore` في `MyBookings.cshtml` (كانوا بـ `var(--dark)` غلط، اتصلحوا بعد ما اتبعتوا أول مرة)، وفي `Chatbot/Index.cshtml` (`.message.user .msg-bubble`, `.send-btn`) اتعملوا صح من الأول.
  **لازم تتفحص من دلوقتي فصاعدًا في أي صفحة جديدة:** أي مكان فيه خلفية دهبي (gradient أو solid) — دور على النص فوقها واتأكد إنه مش بيستخدم `var(--dark)`/`var(--dark2)`/`var(--dark3)`.

- **🐛 قاعدة تانية اتكشفت متأخر (لازم تتراعى من الأول في أي صفحة جديدة):** أي صفحة ليها CSS منفصل (زي `dynasty.css`) لازم الألوان فيها تتطابق مع الـ **tokens** المستخدمة فعليًا في `_Layout.cshtml` نفسه (`.card`, `.details-card`, `.stats-bar`, footer)، مش يتم استنتاجها من وصف الباليتة بس. تحديدًا:
  - خلفية أي عنصر "كارت" (card/panel مرفوع) = `var(--dark3)` — **مش** `var(--dark2)`. `--dark2` مستوى أخف (بيتستخدم بس لحاجات زي صورة كارت فاضية `.card-img` background).
  - أي بريط أفقي كامل العرض بعد الهيرو مباشرة (زي `.stats-bar`) = `var(--dark3)` + `border-top` **و** `border-bottom` بـ `var(--border)` — مش `var(--dark2)` ببوردر واحد بس، عشان كده كان بيظهر "فاصل" حاد بين الهيرو وشريط الفلتر في اللايت مود تحديدًا (الفرق بين `--dark` و`--dark2` كبير في اللايت، فلازم نفس مستوى الكارت `--dark3` + إطار كامل).
  - كل بوردرات الكروت/الأقسام = `1px solid var(--border)` **صريح**، مش `rgba(var(--gold-rgb), x)` شفاف.
  - أي نص ثانوي/باهت (وصف، تاريخ، معلومة فرعية) = `var(--muted)` **ثابت**، مش `rgba(var(--text-rgb), x)` بدرجات شفافية متفاوتة — الموقع كله بيستخدم لون واحد مضبوط للـ muted بدل الشفافية المتدرجة.
  - أزرار الفلتر (زي `.art-filter-btn` في صفحة Artifacts) الحالة الـ Active/Hover بتاعتها **outline style** (`border-color: var(--gold); color: var(--gold); background: rgba(var(--gold-rgb),0.08)`) — **مش** تعبئة دهبي solid كاملة بنص غامق.
  - نص فوق خلفية دهبي solid (زرار مليان، badge...) = لون ثابت `#0d0702` بالظبط (نفس اللون المستخدم في `.btn-gold` و`#back-to-top:hover` في كل الموقع)، مش لون مخترع جديد.
  > **الدرس:** لو هتحول صفحة جديدة، افتح ملف CSS بتاع صفحة اتحولت خلاص فعليًا (زي `pharaoh.css`/Artifacts inline styles) وقارن الـ selectors المتشابهة (`.card`, filter buttons, بريطات أفقية) بدل ما تستنتج من وصف الباليتة في الملف ده بس.


```
--gold:  #b8860b   (دهبي أغمق شوية عن الأصلي، عشان يبان فوق خلفية فاتحة)
--dark:  #f3e8d0   (خلفية الصفحة — كريمي بردي دافي، مش أبيض عادي)
--dark2: #fffaf0   (أفتح سطح — كروت/بانلز مرفوعة)
--dark3: #ece0bc   (سطح نص — نافبار/فوتر/كروت عادية)
--border:#dcc78f
--text:  #3b2411   (بني غامق بدل الكريمي الفاتح)
--muted: #8a6f45
```
> جُرّب 3 باليتات وكانت البردي الفاخر دي هي المختارة (مقارنة بـ"رخام كريمي" أبيض عادي، و"دهبي غامق فاتح" قريب جدًا من الأصلي).

### ⚠️ متغيرين جداد اتقرروا (لسه محتاجين يتضافوا يدويًا في `_Layout.cshtml`)
ظهرت الحاجة ليهم لما اتحولت صفحة `MyBookings.cshtml` (كانت بتستخدم `--gold-light`/`--gold-dark` محليين لتدرجات النص والأزرار، زي عنوان الـ Hero والـ Countdown الأرقام الكبيرة). القرار كان بدل ما نلغي التدرج، نرفعهم لمتغيرات عامة زي الباقي:

```css
/* في :root (الدهبي) — تضاف بعد --gold-rgb */
--gold-light: #e8c96a;
--gold-dark:  #9a7a2e;

/* في html[data-theme="light"] — تضاف بعد --gold */
--gold-light: #a9780f;
--gold-dark:  #8b6508;
```
> ملحوظة تصميم: في اللايت مود "اللايت جولد" لازم يبقى فعليًا **أغمق** من الأصلي مش أفتح — عشان يفضل واضح فوق خلفية فاتحة (نفس منطق تغميق `--gold` نفسه في الباليتة التانية فوق).

**✅ اتعمل فعليًا:** الإضافة الفعلية للسطرين دول في `_Layout.cshtml` (اتأكد وجودهم في النسخة اللي اتبعتت في شات لاحق) — القيم زي ما هي فوق بالظبط.

### ✅ اللي خلص فعليًا (كله جوه `_Layout.cshtml` بس)
- `body`, `nav.main-nav` وكل اللي جواه (logo, search, logout, lang-switch, dropdown, nav-toggle)
- `footer.main-footer`
- ودجت الـ AI Tour Guide كامل (الزرار العائم، النافذة، الفقاعات، صندوق الكتابة)
- `.hero`, `.hero-bg`, `.hero-pattern` (الهيروغليفية الشفافة), `.hero-title`, `.hero-subtitle`
- `.stats-bar`, `.section`, `.section-title`
- `.cards-grid`, `.card` (بكل أجزاءه), `.details-card`, `.details-wrap`
- `.btn-gold`, `.btn-outline`, `.btn-back`

### 🎁 لمسات إضافية اتضافت (بناءً على طلب "شكل مواقع المتاحف العالمية")
- **ظل خفيف للكروت** بس في اللايت مود (`--card-shadow`) — في الداكن فاضل `none` زي ما كان بالظبط
- **توهج ذهبي بسيط** عند hover على `.btn-gold` و`.btn-outline` (`box-shadow` خفيف بلون `--gold-rgb`)
- **تكستشر بردي خفيف جدًا** على خلفية الـ body — CSS بس (`repeating-linear-gradient` بشفافية 0.02/0.015)، مفيش صورة عشان الأداء
- **أيقونة عنخ (𓋹) بسيطة** قبل كل `.section-tag` في الموقع كله — لمسة مصرية موحدة، مش مبالغ فيها

### ✅ صفحات (View level) اتحولت بالكامل لحد دلوقتي
- **Pharaoh** — `Index.cshtml` + `Details.cshtml` (بسيطة، كل الألوان اتلاقتلها مقابل جاهز في المتغيرات الموجودة، مفيش إضافة جديدة)
- **Booking** — `Create.cshtml` (بسيطة برضه) + `MyBookings.cshtml` (الأعقد لحد دلوقتي — كان عندها `:root` محلي مستقل بالكامل، اتحل بالتفصيل في قسم "متغيرين جداد اتقرروا" فوق؛ كمان اتصلح فيها باگ CSS قديم كان موجود أصلاً `border-color:;` فاضي في `.btn-cancel:disabled`)
- **Artifacts** — Index + Details (Index/Details) — تمت المراجعة والتحويل (بره الشات ده)
- **Dynasty** — `Index.cshtml` + `Details.cshtml` + `dynasty.css` كامل — كان فيها `:root` محلي به متغيرات متصادمة (`--gold2`, `--card-bg`...) اتشالت خالص. اتصلح فيها الـ Historical Events `<style>` block الداخلي في `Details.cshtml` (ألوان الفئات `cat-political/military/...` اتسابت ثابتة عمدًا لأنها دلالية زي لون الـ Favorite الأحمر، مش جزء من نظام السطح). كمان فيها استثنائين مقصودين فضلوا بألوان ثابتة غير متغيرة: نص الـ Hero في `Details.cshtml` (`.dyn-det-hero-content h1`, `.dyn-det-hero-meta`) لأنه فوق صورة حقيقية + أوفرلاي أسود ثابت مبيتغيرش مع الوضع، و`.dyn-tl-label` (النص فوق قطاعات التايم لاين الملونة بألوان ثابتة `eraColors`).
- **Favorite** — `Index.cshtml` فقط — أسهل صفحة لحد دلوقتي، مفيهاش CSS منفصل خالص وبتعتمد كليًا على كلاسات `_Layout.cshtml` العامة (`.card`, `.cards-grid`, `.section`, `.btn-gold`). التعديل كان بس استبدال 5 inline styles (`#c9a227`→`var(--gold)` × 4، `#8a7055`→`var(--muted)`).
- **HistoricalEvent** — `Index.cshtml` + `Details.cshtml` + `events.css` كامل. أهم حاجتين: (1) لوحظ إن `#8b6914` بيتكرر كموحّد للحدود/الأسهم/التاجات الدهبية الثانوية في كل الملف، فاتقرر توحيدها كلها تحت `var(--gold-dark)` بدل `var(--border)` العادي عشان تفضل "دهبي غامق" بدل ما تتحول لبني حيادي؛ (2) **باگ مكتشف:** آخر `@@media` في الملف كان مكتوب `@@@@media` (مضاعف) زي قاعدة الـ cshtml، لكن `events.css` ملف CSS خام مش بيتعالج بـ Razor، فالـ `@@@@` كانت بتخلي المتصفح يتجاهل كل الـ responsive styles بتاعة الموبايل خالص — اتصلحت لـ `@@media` عادي. ألوان الفئات (`cat-badge-*`, `cat-dot-*`) فضلت ثابتة زي أي مكان تاني. أزرار الفلتر ("All" بس) اتحولت من تعبئة دهبي صلبة لـ outline style زي Artifacts.
- **Home** — `Index.cshtml` (توست الخطأ العائم + شريطي خلفية Temples/Museums الشفافين)، `Search.cshtml` (عناوين الأقسام الستة + حالة اللا نتائج)، `Timeline.cshtml` (كل الألوان كانت مطابقة تمامًا لقيم الدارك مود الحالية فاتحولت مباشرة للمتغيرات المقابلة، عدا لون داخل رابط placeholder image استُثنى لأنه URL مش CSS).
- **Map.cshtml** — كل عناصر التحكم (أزرار الفلتر، الليجند، البوب أب) اتحولت للـ tokens. **إضافة مهمة:** الخريطة نفسها (Leaflet tile layer) كانت هتفضل غامقة دايمًا لأنها صور raster ثابتة مش CSS — اتضاف منطق JS (`getMapTileUrl()` + `MutationObserver` بيراقب `data-theme` على `<html>`) بيبدّل بين tiles `dark_all` و`light_all` من CartoDB تلقائيًا مع كل توجل، من غير ما يتلمس زرار التوجل الأصلي في الـ Layout.
- **Museum** — `Index.cshtml` (نمط كلاسات جديد `pg-*`) + `Details.cshtml` (نمط `det-*`). النمطين دول هما القالب العام اللي اتكرر بعد كده حرفيًا في Temple. استثناء مقصود: نص البانر الرئيسي في Details (`.det-banner-tag`, `.det-banner-name`) فضل لون ثابت زي Dynasty Hero (فوق صورة + أوفرلاي أسود ثابت).
- **Temple** — نفس بنية `pg-*`/`det-*` بتاعة Museum حرفيًا، تحويل سريع. الفرق الوحيد: بتستخدم `.det-btn-fav` بلون أحمر (❤️/🤍) للمفضلة بدل زرار website منفصل، ولون الأحمر ده (`#e74c3c`) فضل ثابت زي أي لون دلالي (Favorite) في باقي الموقع.
- **God** — `Index.cshtml` + `Details.cshtml`، الطالب عملهم بنفسه وكانوا شبه كاملين. حاجتين اتصلحوا: (1) بوردر ولون نص بادج رمز الإله (`.god-card-symbol`) كانوا لسه `#c9a227`/`rgba(201,162,39,.3)` مش متحولين، اتصلحوا لـ `var(--gold)`/`rgba(var(--gold-rgb),.3)`؛ (2) أوفرلاي بانر الـ Details كان بيستخدم `rgba(45,26,8,...)` (قيمة `--dark3`) بدل `rgba(13,7,2,...)` (قيمة `--dark`) اللي باقي صفحات الـ Details بتستخدمها — اتوحّد بقرار من المستخدم.
- **Hieroglyphics Translator** (`Translator.cshtml`) — مكانتش من ضمن خطة الـ Dark/Light الأصلية، بس اتكشف فيها مشكلة وضوح في اللايت مود جوه قسم "🔤 Alphabet Reference" تحديدًا. السبب: (1) `var(--gold-dim)` مستخدم 3 مرات (عنوان القسم، حدود الـ hover، لون الحرف تحت كل رمز) وهو **متغير مش متعرّف خالص** في أي مكان — بيتحول لـ `var(--gold-dark)`؛ (2) خلفية `.alpha-item` كانت `rgba(0,0,0,0.25)` ثابتة بدل `var(--well-bg)` (التوكن المخصص بالظبط لكده)؛ (3) لون الرمز الهيروغليفي نفسه (`.alpha-glyph`) كان `#c9a227` ثابت (قيمة الدارك مود) بدل `var(--gold)`، فكان بيفضل باهت فوق خلفية فاتحة. بالمرة اتصلح كمان `.output-placeholder` اللي كان بيستخدم `var(--border)` كلون نص بالغلط بدل `var(--muted)`. ألوان الـ Canvas بتاعة تحميل الصورة (`downloadGlyphs`) فضلت ثابتة عمدًا لأنها PNG مُصدَّرة مش جزء من رندر الصفحة.
- **User Dashboard** (`Dashboard.cshtml`) — كانت أعقد حالة اتصلحت في المشروع كله: عندها `:root` منفصل تمامًا بأسماء متغيرات مختلفة (`--dark-card`, `--text-dim`, `--gold-dim`...) وحوالي 90 لون hardcoded منتشرة في التابز/الكاردز/الستاتس بادجز/مودالز الـ Cancel Booking و Remove Favorite — يعني زرار التوجل مكانش بيأثر على الصفحة دي **خالص**. الحل: اتشال الـ `:root` المحلي بالكامل واتحول لـ aliases خفيفة بتشاور على متغيرات الـ Layout (`--dark-card: var(--dark3)` مثلاً)، وكل الألوان اتحولت. ضيف كمان `--red`, `--green`, `--blue`, `--purple`, `--on-gold` كـ tokens عامة جداد في `_Layout.cshtml` نفسه (دارك ولايت) عشان تتستخدم هنا وفي أي صفحة تانية بدل ما تتخترع محليًا. **خريطة الـ "My Journey" جوه التاب:** كانت شغالة بـ tile layer دارك ثابت ومركرز/بوب-أب بألوان hex ثابتة — اتحوّلت زي `Map.cshtml` بالظبط (نفس الـ `getJourneyTileUrl()` + `MutationObserver`)، فبقت بتبدّل تلقائي مع التوجل.
- **Map.cshtml** (مراجعة تانية) — كانت شبه مكتملة من الأول (فيها بالفعل منطق تبديل الـ tiles)، لقيت لون واحد بس متبقي hardcoded (`#0d0702` فوق زرار البوب أب الدهبي) اتحول لـ `var(--on-gold)`.
- **`_Reviews.cshtml`** (Partial بيتضاف في كل صفحات الـ Details: Pharaoh/Temple/Museum/God/Artifact) — كانت بتستخدم باليتة دهب مختلفة تمامًا (`#d4af37`) مالهاش أي علاقة بمتغيرات باقي الموقع، رغم إنها بترندر جوه `_Layout.cshtml` وكانت أصلاً قادرة توصل للمتغيرات العامة من غير ما تعرّفها. كل الألوان اتحولت للـ tokens المشتركة، وحتى ألوان رسايل الـ JS (نجح/فشل) بقت `var(--green)`/`var(--red)`.
- **`Login.cshtml`** — الصفحة الوحيدة اللي `Layout = null`، يعني مالهاش وصول لمتغيرات `_Layout.cshtml` خالص. اتضاف فيها: (1) نفس سكريبت الـ pre-paint اللي بيقرأ `localStorage('plTheme')` قبل أي CSS عشان تحترم اختيار اليوزر المحفوظ (مكانتش بتعمل كده خالص قبل كده، فكانت دايمًا بترجع دارك حتى لو اليوزر مختار لايت)، (2) نفس أسامي وقيم التوكنز بالظبط (دارك + لايت) مكرّرة محليًا جوه الصفحة نفسها لأنها مش عندها وصول لـ `_Layout.cshtml`. **قرار مقصود:** الصفحة دي **من غير زرار Toggle خالص** — بتحترم بس القيمة المحفوظة، زي أغلب المواقع الاحترافية (GitHub, Notion, Vercel) بتعمل بالظبط كده في صفحة الـ Login عشان الشاشة تفضل بسيطة ومركزة على الدخول بس.
- **`Register.cshtml`** — اتفحص ولقيناه View افتراضي فاضي (Bootstrap classes عادية بدون أي تنسيق) مش مستخدم فعليًا لأن الـ Register الحقيقي شغال جوه تابز `Login.cshtml` — اتسيب من غير أي تعديل.

### 🚫 مستبعد بقرار (مش "لسه هيتعمل")
- **Admin Dashboard** (`Admin/Index.cshtml`) — اتقرر إنه **مش هيتترجم أصلًا** (مبدأ الـ ROI: بس الأدمن اللي بيشوفه)، فمفيش داعي يتوحّد مع نظام الدارك مود. مستبعد نهائيًا من الخطة، مش بند متأجل.

### 📁 آخر نسخة شغالة
آخر ملف `_Layout.cshtml` كامل (فيه `--gold-light`/`--gold-dark`/`--red`/`--green`/`--blue`/`--purple`/`--on-gold` مطبقين فعليًا) اتبعت في شات لاحق ومتأكد منه. **كل صفحات الموقع (عدا Admin بقرار مقصود) خلصت وموحّدة مع نظام الدارك/لايت مود:** Dynasty (`Index`/`Details`/`dynasty.css`)، Favorite (`Index`)، HistoricalEvent (`Index`/`Details`/`events.css`)، Home (`Index`/`Search`/`Timeline`/`Map`)، Museum (`Index`/`Details`)، Temple (`Index`/`Details`)، God (`Index`/`Details`)، Translator، User (`Dashboard`/`Login`)، `_Reviews.cshtml`. لو محتاج تبعت أي ملف منهم في شات جديد كمرجع، ابعت `_Layout.cshtml` الأول زي العادة.

---

---

## ✅ Daily Fact (Home Page) — مكتمل بالكامل

> **الحالة:** كل الأجزاء خلصت — العرض (Display) + الأدمن (CRUD).

### اللي خلص فعليًا

**الـ Model:**
- `Models/DailyFact.cs` — ملف جديد: `Id`, `FactText` (إنجليزي، required)، `FactTextAr` (عربي، nullable)، `Category` (اختياري: Daily Life / Science / Religion / Political / Architecture...)
- `DbSet<DailyFact> DailyFacts` اتضاف في `AppDbContext.cs`

**الـ Migration:**
- Migration: `AddDailyFacts` ✅ (بعد تصحيح باگ — تفاصيله تحت في "دروس اتعلمناها")

**الداتا:**
- سكريبت `InsertDailyFacts.sql` — **25 حقيقة حقيقية وموثّقة** عن الحضارة المصرية القديمة (إنجليزي + عربي)، مقصود عمدًا إنها **مش** مجرد نسخ من الـ Description بتاع أي فرعون/إله/معبد موجود أصلاً في صفحات التفاصيل — دي حقايق مستقلة عن الحضارة بشكل عام (تقويم، تحنيط، طب، يوميات...)
- كل الأعمدة العربية اتكتبت بصيغة `N'...'` صح من الأول (اتجنبنا مشكلة الـ `؟؟؟؟؟` اللي حصلت قبل كده مع الـ Gods)
- فيها خطوة الأمان المعتادة (`SELECT COUNT(*)` قبل الـ `INSERT`) + `BEGIN TRANSACTION` / `COMMIT TRANSACTION` صريحين

**الـ Controller — `HomeController.cs`:**
- Method جديدة `GetTodaysFact()` (private) — بتختار حقيقة **ثابتة طول اليوم** لكل اليوزرز (مش بتتغير كل refresh)، باستخدام `Random` بـ seed مبني على `DateTime.Now.Year` + `DayOfYear`، وبتتغير أوتوماتيك تاني يوم
- اتضافت `ViewBag.TodaysFact = GetTodaysFact();` جوه `Index()`

**الـ View — `Home/Index.cshtml`:**
- Section جديد `daily-fact-section` اتحط بعد الـ Stats Bar مباشرة وقبل قسم استكشاف الفراعنة
- بيدعم العربي/الإنجليزي بنفس منطق باقي الصفحة (`lang == "ar" && !string.IsNullOrEmpty(...)`)
- لو مفيش أي حقيقة في الداتا بيز (`todaysFact == null`) الـ section مش بيتعرض خالص (مفيش كراش)

**الـ CSS — `_Layout.cshtml`:**
- كارت `.daily-fact-card` جديد مبني بالكامل على الـ tokens الموجودة (`var(--dark3)`, `var(--border)`, `var(--gold)`, `var(--text)`, `var(--card-shadow)`) — شغال تلقائي مع الدارك/لايت مود من غير أي تعديل إضافي
- عملنا override لـ `::before` بتاعة `.section-tag` الافتراضية (𓋹) عشان منكررش الأيقونة، لأن حطينا أيقونة منفصلة `.daily-fact-icon`

### ⚠️ Key Rules — Daily Fact
- الترتيب الصح لعرض النص: `(lang == "ar" && !string.IsNullOrEmpty(fact.FactTextAr)) ? fact.FactTextAr : fact.FactText`
- لازم تضيف المفتاح `"DailyFact_Title"` في `ar.json` و`en.json` (مثلاً: `"حقيقة اليوم"` / `"Fact of the Day"`) — من غيره `Html.L("DailyFact_Title")` هيرجع اسم المفتاح نفسه كـ fallback
- الحقايق اتقصد عمدًا إنها **مستقلة عن أي صف في أي جدول تاني** (مش FK لحاجة)، عشان الجدول يفضل قابل للتوسع بحرية من غير أي ربط معقد

### 🐛 باگ اتصلح أثناء التنفيذ — Migration Column Conflict
لما اتعمل `Add-Migration AddDailyFacts`، الـ EF ولّد Migration فيها **سطر إضافي غير مقصود**: `migrationBuilder.AddColumn<string>(name: "SymbolAr", table: "Gods", ...)` — ده حصل لأن عمود `SymbolAr` في جدول `Gods` كان اتضاف قبل كده **بـ SQL يدوي مباشر** (وقت مرحلة الـ Multi-language) من غير ما يتعمله Migration مقابلة، فتاريخ الـ EF Migrations مكنش عارف إن العمود ده موجود فعليًا. النتيجة: `Update-Database` فشل برسالة `Column names in each table must be unique. Column name 'SymbolAr' in table 'Gods' is specified more than once.`

**الحل:** اتشال الجزء الخاص بـ `SymbolAr`/`Gods` يدويًا من `Up()` و`Down()` بتوع ملف الـ Migration، وسيبنا بس إنشاء جدول `DailyFacts`.

**📌 قاعدة جديدة لازم تتحفظ (زي دروس الـ SQL التانية):** أي عمود بتضيفه يدوي بـ SQL مباشرة على جدول موجود بالفعل (زي ما حصل مع أعمدة الترجمة العربية) — **لازم بعده تعمل `Add-Migration <اسم>` وتسيب محتوى `Up()`/`Down()` فاضي** (Empty Migration)، بس عشان EF يسجّل في تاريخه إن العمود ده "معروف ومتزامن". من غير الخطوة دي، أي Migration جاية تلمس نفس الجدول هتحاول تضيف نفس العمود تاني وهتفشل بنفس الخطأ.

### ✅ الأدمن CRUD — اتعمل بالكامل (نفس باترن Gods بالظبط)

**`AdminController.cs`:**
- `AddFact` / `EditFact` / `DeleteFact` (POST actions)
- `TotalFacts` + `Facts` اتضافوا جوه الـ `Index()` (بترتيب `OrderBy(f => f.Id)`)

**`Admin/Index.cshtml`:**
- Sidebar item جديد `📜 Daily Facts` بعد Events مباشرة (`switchPanel('facts', this)`)
- Panel `panel-facts` — جدول بالحقايق (Fact EN / Category / Actions) + بحث لحظي (`searchTable('factsTable', ...)`)
- مودالين `modalAddFact` (Textarea FactText + Input Category + قسم Arabic Translation بـ Textarea FactTextAr) و`modalEditFact` (نفس الحقول + Hidden Id)
- JS functions: `openEditFactBtn(btn)` (بيقرا الـ `data-*` attributes) + `openEditFact(id, factText, factTextAr, category)` (بيملى المودال ويفتحه)
- إضافة `facts: '📜 Daily Facts'` في كائن `panelTitles` عشان العنوان فوق الصفحة يبان لطيف لما تفتح التاب

### ⚠️ خطوة يدوية واحدة لازم تتعمل (خارج نطاق الشات ده)
لازم تضاف الخاصيتين دول يدويًا في **`ViewModels/AdminOverviewViewModel.cs`** (الملف ده لسه ما اتبعتش في أي شات عشان يتعدل مباشرة):
```csharp
public int TotalFacts { get; set; }
public List<DailyFact> Facts { get; set; }
```
من غيرهم الكود مش هيعمل Build لأن `Model.TotalFacts` و`Model.Facts` مستخدمين في الـ View والـ Controller.

---

---

## 🏦 Bank + Shop + Quiz Ecosystem — الخطة الكاملة (🚧 قيد التنفيذ)

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
[ ] 5. بناء الكويز (Quiz):
        - أسئلة بتتولد من الداتابيز الموجودة (فراعنة/أسر/آلهة/أحداث)
        - مستويات صعوبة (كل ما الداتابيز تكبر، الأسئلة تتنوع)
        - عند نتيجة كويسة → نداء /coupons/create من الموقع
        - عرض الكود للمستخدم + حفظه في صفحته (My Coupons)
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
Panel Tab). لو حابب نضيف Cancel/Refund للمتجر (زي الـ 48hr rule بتاعة الحجز)، ده الحاجة الوحيدة
اللي لسه برا النطاق الحالي عمدًا (شوف "مش في النطاق الحالي" تحت).

### 🕐 مش في النطاق الحالي (اتقال صراحة، مش نسيان)
- **مفيش Cancel/Refund للمتجر** في النسخة دي — الطلب كان بس "بناء نظام المتجر" (Model +
  Controller + Admin CRUD + عرض/تفاصيل بنفس فورم الدفع). حقل `CancelledAt` في `ShopOrder.cs`
  جاهز أصلاً عشان لو حبينا نضيف نفس منطق `BookingRefundBackgroundService` بعدين من غير أي
  Migration إضافية.

---

## 🎯 خطة احتراف الـ Shop (🚧 قيد التنفيذ — المرحلة 1 جزئيًا + المرحلة 2 خلصت بالكامل)

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

**⏳ باقي بنود المرحلة 1 (لسه هيتعملوا — بعد Categories):**
| الإضافة | التفاصيل |
|---|---|
| ~~صور متعددة~~ | ❌ **اتلغت من الخطة نهائيًا** — مش هتتعمل |
| المواصفات | حقول إضافية في `Product`: `Material` (زي "فضة/راتنج/قطن")، `Dimensions`، `OriginRegion` |
| منتجات مشابهة | Query بسيط في `Details()`: دلوقتي هيبقى ممكن يعتمد على نفس الـ `Category` (بعد ما خلصت) بدل أقرب سعر بس |

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

### المرحلة 3️⃣ — عروض وخصومات وشارات
| الإضافة | التفاصيل |
|---|---|
| سعر مخفّض | `OriginalPrice` (nullable) بجانب `Price` — لو موجود يبان Strikethrough + نسبة الخصم |
| شارات | `IsFeatured`, `IsBestSeller`, `IsNew` (bool) — تتحط يدويًا من الأدمن أو تلقائي (IsNew لو CreatedAt أقل من 30 يوم) |
| الكوبون | شغال أصلاً في المتجر من غير أي تعديل ✅ |
| تنبيه المخزون | "باقي 3 بس!" لو `StockQuantity` أقل من رقم معين — نفس منطق التلوين الأحمر الموجود في Admin |

### المرحلة 4️⃣ — لمسات احترافية إضافية (اختيارية)
- **Wishlist للمنتجات**: تستخدم جدول `Favorites` الموجود، تضيف `Type = "product"`
- **Breadcrumbs**: Home > Shop > Category > Product
- **SKU / رقم منتج**: حقل بسيط للتنظيم الداخلي

---

# 🚀 Roadmap — الترتيب بالأولوية

> خلّص واحدة وروح للتانية — مش محتاج تفكر في أي حاجة تانية.

---

## ✅ اللي خلص

```
✅ 1.  Interactive Map
✅ 2.  My Journey Tab + Visited Status
✅ 3.  Hieroglyphics Translator
✅ 4.  AI Tour Guide Chatbot
✅ 5.  Artifacts
✅ 6.  Rating + Comments
✅ 7.  Dynasties Page
✅ 8.  Historical Events
✅ 9.  Multi-language (عربي/إنجليزي)
✅ 10. Dark / Light Mode (كل الصفحات عدا Admin بقرار مقصود — تفاصيل كاملة في قسم "🌗 Dark / Light Mode System" فوق)
✅ 11. Daily Fact (Home Page) — تفاصيل كاملة في قسم "Daily Fact" فوق
✅ 12. Shop System (متجر) — تفاصيل كاملة في قسم "🛍️ Shop System" فوق
✅ 13. Shop — Categories (تصنيفات + فلترة + ترتيب + بحث) — تفاصيل كاملة في قسم "🎯 خطة احتراف الـ Shop" فوق (المرحلة 2)
```

---

## 🔜 الجاي — واحدة واحدة بالترتيب ده

```
[x] 10. Daily Fact (Home Page) ✅ — تفاصيل كاملة في قسم "Daily Fact" تحت
[x] Shop System ✅ — خلص بالكامل، تفاصيل في قسم "🛍️ Shop System" فوق

🚧 دلوقتي شغالين على باقي Bank + Quiz Ecosystem (الـ Shop خلص وطلع بره القايمة دي)
    (تفاصيل كاملة في قسم "🏦 Bank + Shop + Quiz Ecosystem" فوق)
    ده بيغطي البنود 11 و18 تحت مع بعض

[ ] 11. Quiz تفاعلي            → جزء من الـ Ecosystem الجديد فوق
[ ] 12. Email Confirmation + QR Code
[ ] 13. Analytics Dashboard (Admin)
[ ] 14. AI Trip Planner
[ ] 15. Notification System
[ ] 16. Smart Search
[ ] 17. Achievements & Badges
[ ] 18. Payment System (Fake) → جزء من الـ Ecosystem الجديد فوق (Python Bank Service)
[ ] 19. Google Login (OAuth)
[ ] 20. Photo Gallery
[ ] 21. User Profile Expansion
[ ] 22. Export PDF Reports
[ ] 23. Group Booking
[ ] 24. Leaderboard
[ ] 25. Share على السوشيال ميديا
[ ] 26. Waitlist System
[ ] 27. AI Recommendations
[ ] 28. PWA (Install as App)
[ ] 29. Public API
```

---

> كل الـ features التانية الكتيرة (3D / AR / Metaverse / Microservices / ...) موجودة في قسم **المرجع الكامل** تحت — للتوثيق بس، مش للتنفيذ دلوقتي.

---

---

# 📦 المرجع الكامل — تفاصيل كل Feature

---

## 🔐 Authentication System Expansion

### الحالي ✅
- Login / Register / Session Auth / Guest Access

### التطوير القادم
- [ ] JWT Authentication + Refresh Tokens
- [ ] Remember Me
- [ ] Multi-session Management + Device Tracking
- [ ] Login History + Last Seen + Active Sessions Panel
- [ ] Password Reset by Email
- [ ] Email Verification + Magic Link Login
- [ ] OAuth (Google / Facebook / GitHub)
- [ ] Two Factor Authentication
- [ ] Biometric Login Simulation
- [ ] Suspicious Login Detection + Brute Force Protection
- [ ] CAPTCHA

---

## 👤 User System Expansion

### Profile System
- [ ] Full Profile Page (Cover Photo + Avatar Upload + Bio)
- [ ] Favorite Era + Favorite Pharaoh + Travel Preferences
- [ ] Wishlist + Public Profiles
- [ ] Follow Users + User Activity Feed
- [ ] User Reputation Score

---

## 🎫 Booking System — Enterprise Level

### الحالي ✅
- Booking + Cancel Rule (48hr) + Payment Records + Visited Status

### التطوير القادم
- [ ] Real Seat Availability Engine + Dynamic Capacity
- [ ] Time Slot Booking + Real Calendar Availability
- [ ] Seasonal Pricing + Peak Hour Pricing
- [ ] Group Booking + Family Booking + VIP Booking
- [ ] Guided Tours Booking + Tour Bus Booking
- [ ] Hotel Integration Simulation
- [ ] Waitlist System + Reservation Expiry Timer
- [ ] Auto Cancel Unpaid Bookings
- [ ] QR Ticket Generation + Smart Ticket Validation
- [ ] Booking Confirmation Email + SMS Notification Simulation
- [ ] Booking Invoice PDF + Booking Status Timeline
- [ ] Rebooking System + Refund Requests + Partial Refunds
- [ ] Booking Coupons + Promo Codes + Loyalty Discounts → جاري تنفيذها ضمن "🏦 Bank + Shop + Quiz Ecosystem" (Coupon من الكويز)
- [ ] Multi-currency Booking + Booking Analytics
- [ ] Real-time Capacity Counter
- [ ] AI Suggested Visit Times + Weather-aware Booking Suggestions

---

## 💳 Payment & Banking System

> ⚠️ التنفيذ الفعلي جاري دلوقتي — شوف قسم "🏦 Bank + Shop + Quiz Ecosystem" فوق للتفاصيل الكاملة والقرارات النهائية.
> اللي تحت ده مرجع للأفكار الإضافية المستقبلية (بعد ما نخلص النسخة الأساسية).

### Fake Banking Ecosystem (Python API)
- [x] Bank Accounts + Wallets + Balance Management ✅ (خلص في الـ Python Service)
- [x] Transactions + Transaction History ✅ (خلص في الـ Python Service)
- [ ] Transfer Between Accounts (مستقبلي — مش في النطاق الحالي)
- [ ] Payment Gateway + Refund System + Failed Payments + Payment Retry
- [ ] Fraud Detection + Risk Score + AI Fraud Detection
- [ ] Payment Logs + Audit Logs + OTP Simulation
- [ ] Currency Conversion
- [x] Card Validation الأساسي (رقم كارت وهمي + Masking) ✅ — Type/Expiry/CVV المتقدم لسه مستقبلي
- [ ] Spending Limits + Daily Limits
- [ ] Payment Notifications + Transaction Receipts
- [ ] Admin Financial Dashboard + Revenue Tracking + Revenue Forecasting
- [x] Banking Microservice ✅ (FastAPI) — Payment Queue System لسه مستقبلي

---

## 🤖 AI Systems

### AI Tour Guide 2.0

#### الحالي ✅
- Groq + LLaMA 3.1 Chatbot Floating Widget

#### التطوير القادم
- [ ] Voice Responses + Multi-language AI
- [ ] Personality Modes (Storytelling / Historical Narrator / Child-friendly / Scholar)
- [ ] Emotional AI Reactions + Context Memory
- [ ] AI remembers user interests
- [ ] AI explains maps + artifacts visually
- [ ] AI-generated quizzes + tours + summaries + timelines

---

### 🧭 AI Trip Planner

- [ ] User enters days / budget / interests → AI generates full itinerary
- [ ] Route optimization + Smart scheduling + Budget-aware planning
- [ ] Travel time estimation + Food suggestions + Nearby attractions
- [ ] Personalized recommendations
- [ ] Modes: Family / Student / Luxury
- [ ] Offline itinerary export + PDF itinerary generation
- [ ] Interactive trip map + AI trip assistant

---

### 🧠 AI Recommendation Engine

- [ ] Recommendation Scores + Interest Profiling + Behavior Tracking
- [ ] Collaborative Filtering + Content-based Recommendation
- [ ] Similar Pharaohs / Dynasties / Museums
- [ ] Recommended Trips / Events / Articles
- [ ] Smart Homepage Personalization

---

### 🔍 Smart Search Engine

- [ ] Semantic Search + Natural Language Search + Voice Search
- [ ] AI-powered Search + Search Suggestions + Search History
- [ ] Trending Searches + Search Analytics
- [ ] OCR Search from Images + Historical Question Answering

---

### 🖼️ AI Image Systems

- [ ] AI Artifact Recognition + AI Monument Detection
- [ ] Upload image → identify artifact
- [ ] AI Historical Restoration + AI Image Colorization
- [ ] AI Face Reconstruction + AI Pharaoh Portrait Generator
- [ ] AI Scene Recreation + AI-generated Ancient Egypt Wallpapers

---

### 🧠 RAG & Intelligent Knowledge Systems

- [ ] RAG Architecture + PDF Knowledge Ingestion
- [ ] Historical Dataset Retrieval + Smart Knowledge Base
- [ ] Semantic Embeddings Search + AI Historical Reasoning
- [ ] Context-aware Responses + Multi-source Knowledge Fusion

---

### 🤖 Multi-Agent AI Ecosystem

- [ ] Historian Agent + Tourist Guide Agent + Archaeologist Agent
- [ ] Booking Assistant Agent + Recommendation Agent
- [ ] Security Monitoring Agent + Educational Tutor Agent
- [ ] Research Assistant Agent + AI Agents Communication Layer

---

### ⚙️ Autonomous Automation Systems

- [ ] Auto Content Tagging + AI Auto Moderation
- [ ] Auto Recommendation Retraining + Auto Event Categorization
- [ ] Auto Metadata Generation + AI Content Classification
- [ ] Auto Notification Rules + AI Content Prioritization

---

### 🧠 Adaptive Intelligence Systems

- [ ] Adaptive Homepage + Dynamic User Experience + Personalized UI
- [ ] Mood-aware AI Interaction + Learning-based Recommendations
- [ ] Smart User Journey Prediction + Behavioral AI Personalization

---

## 🌍 Interactive Experience

### 🗺️ GIS & Maps — Advanced

#### الحالي ✅
- Interactive Map (Leaflet.js) + Filter Buttons + Admin Map Picker

#### التطوير القادم
- [ ] Full Egypt GIS Layer + Heatmaps + Historical Layers
- [ ] Time-based Map + Route Navigation + Nearby Places
- [ ] GPS Simulation + Satellite Maps + Temple Clusters + Smart Filters
- [ ] Archaeological Layers + Ancient Trade Routes
- [ ] Ancient Egypt Borders by Era + Nile Flood Simulation
- [ ] Ancient Cities Reconstruction

---

### 🛰️ Advanced GIS Intelligence

- [ ] Terrain Simulation + Ancient Nile Flood Simulation
- [ ] Ancient Trade Route Analysis + Archaeological Prediction AI
- [ ] Ancient Population Distribution Maps + Spatial Historical Analytics
- [ ] Historical Border Evolution + Archaeological Site Discovery Engine

---

### 🏛️ Virtual Museum

- [ ] 360° Tours + 3D Museum Navigation + Interactive Artifact Rotation
- [ ] Museum Audio Guide + Ambient Sounds + Guided Virtual Tours
- [ ] Multiplayer Virtual Tours + VR Support
- [ ] Interactive Museum Challenges + Hidden Artifact Hunt

---

### 🧱 3D Systems

- [ ] 3D Pyramids + 3D Temples + 3D Tombs
- [ ] Interactive Tomb Exploration + Ancient City Reconstruction
- [ ] Pyramid Interior Simulation + Build-a-Pyramid Game
- [ ] Ancient Architecture Explorer

---

### 📱 AR Experience

- [ ] AR Artifacts + AR Pharaoh Masks + AR Temple View
- [ ] AR Pyramid at Home + Camera Filters
- [ ] AR Hieroglyphics + AR Guided Tours

---

## 🎮 Gamification System

### XP & Levels
- [ ] XP System + User Levels + Rank Titles
- [ ] Achievement Progression + Skill Trees
- [ ] Reputation Points + Explorer Score

### 🏆 Achievements & Badges
- [ ] Visit Achievements
- [ ] Dynasty Expert / Pharaoh Expert / Historian / Quiz Master Badges
- [ ] Reviewer Badge + Community Helper Badge
- [ ] Legendary Explorer Badge + Hidden Secret Achievements

### 🎯 Missions & Challenges
- [ ] Daily Missions + Weekly Challenges
- [ ] Exploration / Quiz / Event Challenges
- [ ] Treasure Hunt Events + Community Challenges + Seasonal Events

---

## 🧩 Educational Systems

### 📚 Quiz Engine
> ⚠️ النسخة الأساسية (Dynamic Quiz + Difficulty Levels + Coupon Rewards) جاري تنفيذها دلوقتي ضمن قسم "🏦 Bank + Shop + Quiz Ecosystem" فوق.
- [ ] Dynamic Quiz Generator + AI-generated Questions
- [ ] Timed Quizzes + Multiplayer Quiz Battles
- [ ] Quiz Leaderboards + Quiz Rewards
- [ ] Difficulty Levels + Exam Mode + Daily Quiz + Tournament Mode

### 📖 Learning System
- [ ] Learning Paths + Ancient Egypt Courses + Interactive Lessons
- [ ] Certificates + Educational Progress Tracking
- [ ] Student Dashboard + Teacher Dashboard

---

## 👥 Social & Community Systems

### 🌐 Community Platform
- [ ] User Posts + Historical Discussions + Forums
- [ ] Comments on Articles + Community Groups
- [ ] Follow System + Messaging System + Notifications Feed
- [ ] Mention System + Reactions System + Polls
- [ ] User-generated Content

### 🤝 Collaborative & Realtime Experience
- [ ] Shared Trip Planning + Collaborative Museum Tours
- [ ] Watch Together Mode + Live Guided Sessions
- [ ] Shared Annotations + Real-time Collaborative Maps
- [ ] Multi-user Exploration Rooms + Real-time Learning Sessions

---

## 📸 Media Systems

- [ ] Photo Gallery + User Uploads + Travel Albums
- [ ] Video Uploads + Historical Reels
- [ ] AI-generated Slideshows + Image Moderation + Community Voting

---

## 📊 Analytics & Big Data

### 📈 Analytics Dashboard
- [ ] Revenue Reports + Booking Trends + User Growth Analytics
- [ ] Visitor Heatmaps + Engagement Analytics + Session Analytics
- [ ] AI Analytics + Search Analytics + Device Analytics
- [ ] Geographic Analytics + Conversion Rates + Funnel Analytics
- [ ] Admin Insights

---

## 📡 Real-time Systems

- [ ] Real-time Notifications + Live Dashboard Updates
- [ ] Real-time Booking Status + Live Visitor Counters
- [ ] Live Chat + Real-time Maps + Live Events Feed

---

## 🔔 Notification Ecosystem

- [ ] SignalR Notifications + In-app Notifications + Push Notifications
- [ ] Email Notifications + Booking Alerts + Event Reminders
- [ ] AI Suggestions Notifications + Badge Notifications
- [ ] Smart Notification Preferences

---

## 🛡️ Cybersecurity Layer

### Security Systems
- [ ] Audit Logs + Login Logs + Admin Action Logs
- [ ] Security Dashboard + Threat Monitoring
- [ ] Suspicious Activity Detection + Rate Limiting + API Protection
- [ ] IP Tracking + Device Fingerprinting
- [ ] CSRF / SQL Injection / XSS Protection
- [ ] Encryption Layer + Secure Headers + File Upload Scanning
- [ ] Honeypot Inputs + Security Alerts

### SOC Systems
- [ ] SIEM-style Dashboard + Threat Correlation Engine
- [ ] Honeypot Simulation + Attack Detection Dashboard
- [ ] Intrusion Detection Simulation + Security Event Analytics
- [ ] Threat Intelligence Feed + Security Incident Timeline
- [ ] Attack Replay System + Security Awareness Training Module

---

## 🧱 System Architecture — مستوى شركات

### Microservices
- [ ] AI Service + Banking Service + Notification Service
- [ ] Analytics Service + Search Service + Recommendation Service
- [ ] Auth Service + Media Service

---

## ⚡ Performance Systems

- [ ] Redis Cache + Response Caching + Lazy Loading
- [ ] Background Jobs + Queue System
- [ ] Image Compression + CDN Integration
- [ ] Database Optimization + Query Optimization
- [ ] Pagination Everywhere + Infinite Scrolling

---

## ☁️ DevOps & Cloud

### Hosting & Deployment
- [ ] Azure Deployment / Railway Deployment
- [ ] Docker + Nginx + CI/CD Pipeline (GitHub Actions)
- [ ] SSL Certificates + Domain
- [ ] Production Monitoring + Error Logging
- [ ] Auto Backups + Environment Separation

### Distributed Cloud Architecture
- [ ] API Gateway + Service Discovery + Distributed Tracing
- [ ] Centralized Monitoring + Observability Stack
- [ ] Distributed Logging + Auto Scaling Simulation
- [ ] Health Monitoring Services

---

## 📱 Mobile & PWA

- [ ] Progressive Web App + Offline Support + Install as App
- [ ] Push Notifications + Offline Maps + Offline Articles
- [ ] Offline AI Responses Cache + Mobile Gestures

### Native Mobile Ecosystem
- [ ] Flutter Mobile App + Offline Sync + Mobile GPS Tours
- [ ] QR Scanner + Mobile AR Camera + Mobile Push Ecosystem
- [ ] Offline Smart Guides + Gesture Navigation

---

## 🌐 Global Features

- [ ] Arabic Language + English Language + German Language
- [ ] RTL / LTR Support + Currency Switching + Timezone Support
- [ ] Accessibility Support + Screen Reader Compatibility

---

## 📦 Public API Platform

- [ ] Pharaohs API + Dynasties API + Museums API + Temples API + Artifacts API
- [ ] Authentication API + Booking API
- [ ] Documentation (Swagger) + API Keys + Rate Limits
- [ ] Public Developer Portal

---

## 🎬 WOW Factor Features

### Cinematic Systems
- [ ] Ancient Egypt Intro Animation + Dynamic Sand Effects
- [ ] Day / Night Mode + Animated Nile
- [ ] Cinematic Transitions + Historical Battle Animations
- [ ] Interactive Timeline Zoom + Animated Hieroglyphics

### 🤯 Crazy Features
- [ ] Time Travel Mode + Ancient Egypt Simulator
- [ ] Pharaoh Decision Simulator + Historical Battle Simulator
- [ ] Build Your Dynasty Game
- [ ] AI Story Generator + AI Documentary Narrator
- [ ] AI-generated Historical Scenarios
- [ ] Ancient Egypt Metaverse Lite + Multiplayer Exploration
- [ ] AI Companion Character + Smart NPCs

---

## 🌌 Multiplayer & Metaverse Systems

- [ ] Multiplayer Exploration Mode + Shared Historical Missions
- [ ] Guilds & Teams + Multiplayer Treasure Hunts
- [ ] Live Cooperative Quizzes + Social Virtual Museum
- [ ] Multiplayer Historical Battles + Ancient Egypt Metaverse Lite

---

## 🧬 Advanced Research & Data Science

### Data Science & Prediction
- [ ] Predictive Tourism Analytics + Visitor Forecasting
- [ ] Seasonal Prediction Models + Revenue Prediction
- [ ] User Churn Prediction + User Segmentation AI
- [ ] Crowd Density Prediction + Smart Capacity Forecasting
- [ ] AI Tourism Trends Dashboard

---

## 🧠 Knowledge Graph & Semantic Systems

- [ ] Neo4j Integration + Historical Relationship Graph
- [ ] Pharaoh Family Trees + Dynasty Relationship Mapping
- [ ] Interactive Entity Graph + Semantic Historical Explorer
- [ ] AI Relationship Discovery + Historical Dependency Mapping
- [ ] Smart Historical Linking

---

## 🧾 Advanced CMS Platform

- [ ] Rich Text Editor + Draft / Publish Workflow + Scheduled Publishing
- [ ] Version Control for Articles + Content Revision History
- [ ] SEO Metadata Generator + AI Content Assistant
- [ ] Media Management System + Dynamic Content Blocks
- [ ] Content Approval Pipeline + Moderator Roles

---

## 🌐 SEO & Discoverability Engine

- [ ] Dynamic Sitemap + Open Graph Integration + Schema.org Markup
- [ ] AI SEO Optimization + SEO Health Dashboard
- [ ] Search Engine Indexing Tools + Smart URL Structure
- [ ] AI-generated Meta Descriptions + Internal Linking Engine
- [ ] Trending Content Detection

---

## 🎥 Streaming & Media Ecosystem

- [ ] Live Museum Streams + Live Archaeological Events
- [ ] Webinar Platform + Educational Live Sessions
- [ ] Video Archive + Historical Documentary Streaming
- [ ] AI-generated Video Summaries + Media Recommendations

---

## 🏺 Artifact Preservation & Archaeology

- [ ] Artifact Condition Tracking + Restoration History Timeline
- [ ] Preservation Status Monitoring + Environmental Damage Simulation
- [ ] Archaeological Discovery Tracking + Artifact Lifecycle Management
- [ ] Smart Preservation Alerts

### Digital Preservation Initiative
- [ ] 3D Artifact Preservation + Digital Scanning Archive
- [ ] Long-term Cultural Archiving + Historical Data Preservation APIs
- [ ] Ancient Egypt Open Archive + Smart Restoration Simulation

---

## 🏛️ Museum Management Platform

- [ ] Artifact Inventory Management + Employee Management
- [ ] Museum Maintenance Tracking + Visitor Flow Management
- [ ] Smart Museum Capacity Control + Internal Museum Analytics
- [ ] Museum Security Monitoring + Artifact Loan Tracking

---

---

# 🏁 Final Goal

لما المشروع يخلص يكون:

- ✅ منصة متكاملة
- ✅ فيها AI متعدد الوكلاء
- ✅ فيها GIS متقدم
- ✅ فيها Payments & Banking
- ✅ فيها Gamification كامل
- ✅ فيها Analytics & Big Data
- ✅ فيها Security & SOC Systems
- ✅ فيها Enterprise Architecture & Microservices
- ✅ فيها Real-time Features
- ✅ فيها Interactive Experiences (3D / AR / VR)
- ✅ فيها Knowledge Graph & RAG
- ✅ فيها Multiplayer & Metaverse

> **منتج حقيقي قابل للتحول لشركة أو Startup.**
