# 🏺 Pharaohs Legacy — Project Progress & Ultimate Roadmap

> **الهدف:** تحويل Pharaohs Legacy من مشروع تخرج إلى منصة سياحية/ثقافية/ذكية متكاملة بمستوى Startup أو Enterprise Platform.AI Trip Planner

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
| Artifacts | Id, Name, Origin, Period, Category, Description, ImageUrl, Museum, CurrentLocation (+ نسخ Ar لكل حقل نصي) *(⚠️ الأسماء دي هي الصح الموجودة فعليًا في `Artifact.cs` — كان متكتوب هنا قبل كده MuseumName/OriginLocation/Type بالغلط، وده سبب باگ CS1061 في QuizQuestionGeneratorService.cs، اتصحح. الجدول نص وصفي بس، مش FK حقيقي — أغلب القطع في متاحف عالمية برا نطاق جدول Museums)* |
| QuizHistories | Id, UserEmail, PlayedAt, Score, Total, ScorePercent, Grade, StreakEligible, StreakDays, DiscountPercent, CouponCode *(🆕 جدول دائم — تسجيل كل كويز خلص، أساس فحص "لعب النهاردة؟" وحساب الـ Streak)* |

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
- Bank + Shop + Quiz Ecosystem ✅ (مكتمل بالكامل — تفاصيل كاملة في `BANK_SHOP_QUIZ_DETAILS.md`)
- Email Confirmation + QR Code (E-Ticket) ✅ (مكتمل بالكامل — تفاصيل كاملة تحت في قسم "Email Confirmation + QR Code")
- Smart Search (بند 16) ✅ (مكتمل بالكامل — بحث موحّد + Autocomplete + Fuzzy matching + Search History + Trending — تفاصيل كاملة تحت في قسم "Smart Search")
- Achievements & Badges (بند 17) ✅ (مكتمل بالكامل للنسخة الأساسية + Dynasty Expert/True Historian + الـ 6 Hidden Secret Achievements — كل الكود اتكتب، تفاصيل كاملة تحت في قسم "بند 17 — Achievements & Badges" — باقي بس: Migration جديدة `AddItemViewsAndSecretBadges` + قرار مفتوح واحد بخصوص عرض progress bar للشارات السرية في الداشبورد)
- Notification System (بند 15) ✅ (مكتمل بالكامل — In-app + Polling + 12 Trigger عبر Booking/Review/User/Quiz/Shop/TripPlanner/Admin + التحديث التلقائي للشحن — تفاصيل كاملة تحت في قسم "Notification System")

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

## 🏦🛍️🧠 Bank + Shop + Quiz Ecosystem (✅ مكتمل بالكامل)

> **التفاصيل الكاملة اتنقلت لملف منفصل** (البنك + المتجر + الكويز — كل القرارات، الكود،
> الـ Endpoints، الـ State Machine، خطوات اللصق، كل حاجة) عشان الملف ده كان بقى كبير جدًا:
> 📄 **`BANK_SHOP_QUIZ_DETAILS.md`**
>
> لو في شات جديد وعايز تكمل شغل مرتبط بأي حاجة من التلاتة دول، ابعت الملف ده مع الملف الرئيسي.

**الحالة: كل حاجة خلصت ومتستنجة يدويًا 100%:**
- 🏦 **Bank Service** (Python/FastAPI) — Accounts, Card-Validated Charge, Refund, Coupons, OTP, Dashboard
- 🎫 **ASP.NET Integration** — Booking + Shop Confirm/Cancel/ValidateCoupon، و`BookingStatusService`
  (State Machine مركزي محمي من انتقالات الحالة الغلط والاسترجاع المزدوج)
- 🛍️ **Shop System** بالكامل — منتجات، أوردرات، تراك شحن تلقائي، Admin Panel Tab مستقل، Checkout بستايل Amazon
- 🧠 **Quiz Engine** — 10 أسئلة يومية، Grade Tiers + Streak منفصلين، خصم متغير بسقف 35%

⚠️ **ملاحظة صغيرة اتسجلت أثناء المراجعة الأخيرة (تحسينة مستقبلية، مش لازم تتقفل دلوقتي):**
لو يوزر بدأ خطوة الدفع (طلب OTP) وسابها في النص من غير ما يكمل، الحجز/الأوردر ممكن يفضل
عالق على حالة `PendingPayment` للأبد من غير أي تنظيف تلقائي — ممكن تتراكم "حجوزات أشباح"
في الداتابيز بمرور الوقت. لو حابب تقفلها بعدين، الحل المنطقي Background Job بسيط زي
اللي موجود أصلاً لحالات تانية في المشروع.

---



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
✅ 12. Shop System (متجر) — تفاصيل كاملة في `BANK_SHOP_QUIZ_DETAILS.md`
✅ 13. Shop — Categories (تصنيفات + فلترة + ترتيب + بحث) — تفاصيل في `BANK_SHOP_QUIZ_DETAILS.md` (المرحلة 2)
✅ 14. Shop — Offers & Badges (عروض وخصومات وشارات) — تفاصيل في `BANK_SHOP_QUIZ_DETAILS.md` (المرحلة 3)
✅ 15. Shop — Wishlist + Breadcrumbs + SKU (لمسات احترافية) — تفاصيل في `BANK_SHOP_QUIZ_DETAILS.md` (المرحلة 4) — خطة احتراف الـ Shop خلصت بالكامل دلوقتي (كل المراحل الأربعة)
✅ 16. Quiz تفاعلي (Grade + Streak + خصم متغير) — تفاصيل كاملة في `BANK_SHOP_QUIZ_DETAILS.md`
✅ 17. Payment System (Fake) — Python Bank Service (Accounts/Charge/Refund/Coupons) — تفاصيل كاملة في `BANK_SHOP_QUIZ_DETAILS.md`
```

---

## 🔜 الجاي — واحدة واحدة بالترتيب ده

```
[x] 13. Analytics Dashboard (Admin) — ✅ مكتمل (تفاصيل تحت)
[x] 14. AI Trip Planner — ✅ مكتمل (تفاصيل تحت)
[x] 15. Notification System — ✅ مكتمل (تفاصيل تحت)
[x] 16. Smart Search — ✅ مكتمل (تفاصيل تحت)
[x] 17. Achievements & Badges — ✅ مكتمل (Views + Models + Controllers + Migration + الترجمة + التسجيل في Program.cs + الـ Service) — تفاصيل كاملة تحت في قسم "بند 17 — Achievements & Badges" — باقي بس قرارين مؤجلين (Dynasty Expert/Historian + Hidden Secret Achievements)
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

## 🏆 بند 17 — Achievements & Badges — ✅ مكتمل بالكامل (28 يوليو)

> **القرار:** اتعمل كل الشارات المتاحة دلوقتي مرة واحدة (مش MVP تدريجي). القسم ده كان تخطيط بس قبل كده — دلوقتي بيوثق اللي اتنفذ فعليًا في الكود (Views + Models + Controllers + Service + Migration + الترجمة + التسجيل في Program.cs)، عشان في أي شات جديد تبعتلي الملف وأنا أبقى فاهم إحنا واقفين فين بالظبط.
> ⚠️ لسه باقي قرارين مؤجلين مش جزء من الفيتشر الأساسي: Dynasty Expert/Historian (مصدر بيانات مش مقرر) + Hidden Secret Achievements (شروطها مش محددة) — شوف قسم "لسه Pending" تحت.

### ✅ قرارات اتاخدت وطُبّقت فعليًا
| القرار | الاختيار |
|---|---|
| الـ Visit يتحسب إزاي | `Booking.Status == "Visited"` بس (مش مجرد فتح صفحة Details) |
| Tiers | متدرجة لكل بادج (Bronze/Silver/Gold) — كل Tier بيتسجل كصف منفصل في `UserBadges` (تاريخ كامل لمتى اتفتح كل درجة) |
| عرض الـ Hidden Secret Achievements | باهتة/Grayscale (`filter: grayscale(100%); opacity: 0.45`) قبل ما تتفتح — مش "؟؟؟" نصي ومش اختفاء كامل. الاسم والوصف بيتقنّعوا بـ "؟؟؟" لحد ما تتفك عشان السر يفضل سر |
| شرط Legendary Explorer | لازم Gold في **كل** الشارات التانية المفعّلة حاليًا (Explorer + Pharaoh Expert + Reviewer + Community Helper + Quiz Master) — مش مجرد أي Tier |

### 🗂️ الشارات المتاحة فعليًا دلوقتي (Seed في `BadgeCatalogSeed.cs`)
| المجموعة | الشارة | Tiers (Threshold) | المصدر |
|---|---|---|---|
| Visit | Explorer / Temple Master / Grand Explorer | 3 / 7 / 15 (أماكن Visited) | `Bookings.Status == "Visited"` |
| Knowledge | Pharaoh Expert | 5 / 15 / 30 (مفضلة) | `Favorites.Type == "pharaoh"` |
| Knowledge | Quiz Master | 5 / 20 / 50 (أكبر رقم بين عدد مرات اللعب أو الـ Streak) | `QuizHistories` |
| Community | Reviewer | 3 / 10 / 25 (ريفيوهات) | `Reviews` |
| Community | Community Helper | 5 / 20 / 50 (Helpful votes مستلمة) | `ReviewHelpfuls` |
| Legendary | Legendary Explorer | Gold في الخمس شارات فوق | متعدد |

> ⚠️ **الـ Thresholds دي اقتراح مبدئي قابل للتعديل بسهولة** (مجرد أرقام في `BadgeCatalogSeed.cs`) — لو عايز تغيّرها بعد ما تشوف سلوك المستخدمين الحقيقي، عادي.

### ✅ Dynasty Expert / True Historian + Hidden Secret Achievements — اتعملوا بالكامل (تحديث لاحق)

#### 🏛️ Dynasty Expert / True Historian
- **مصدر البيانات:** تتبع Views فعلي (مش Favorites) — عشان الشارة تعكس معرفة حقيقية مش مجرد حفظ مفضلة.
- **التصميم:** جدول عام (Generic) جديد اسمه `ItemViews` — نفس فلسفة `Favorites` بالظبط (Type + ItemId)، عشان نقدر نوسّعه لاحقًا لأنواع تانية (متاحف/أماكن) من غير ما نعمل جدول جديد لكل نوع. دلوقتي مفعّل بـ `Type == "dynasty"` بس.
- **بنية الجدول:** `ItemViews` → Id, UserEmail, Type, ItemId, ViewedAt (+ Unique Index على UserEmail+Type+ItemId على مستوى الداتا بيز)
- **Dynasty Expert (شارة عادية ظاهرة، Knowledge):** Bronze 5 / Silver 15 / Gold 30 — عدد الأسرات المختلفة اللي اتفتحلها Details (`DISTINCT ItemId`)
- **True Historian (شارة سرية Hidden):** فتح تفاصيل كل الـ 35 أسرة — نفس المصدر، شرط كامل
- **التسجيل:** في `DynastyController.Details()` — مرة واحدة بس لكل يوزر/أسرة (مش كل مرة يفتحها)، وبعدين بيفحص Dynasty Expert + كل الـ Hidden Achievements + Legendary Explorer

#### 🕵️ Hidden Secret Achievements (6 شارات) — كتالوجهم اتضاف + منطق الفحص كامل
| الشارة | الشرط | المصدر | Threshold |
|---|---|---|---|
| Perfect Score | 100% في كويز (Score == Total) ولو مرة | `QuizHistories` | 1 (boolean) |
| Streak Legend | Streak متواصل 30 يوم في الكويز | `QuizHistories.StreakDays` | 30 |
| Museum Completionist | زيارة (Visited) كل المتاحف بدون استثناء | `Bookings` (Distinct museums Visited) | 42 ⚠️ ثابت، لازم يتحدّث لو اتضاف متحف |
| True Historian | فتح تفاصيل كل الـ 35 أسرة | `ItemViews` | 35 ⚠️ ثابت، لازم يتحدّث لو اتضافت أسرة |
| Loyal Explorer | عضو من سنة كاملة | `Users.CreatedAt` | 365 يوم |
| Night Owl | حجز أو كويز اتسجل الساعة 3 بالظبط (3:00–3:59) | `Bookings.CreatedAt` / `QuizHistories.PlayedAt` (فحص الـ Hour، مفيش عمود جديد) | 1 (boolean) |

> منطق الفحص كله في `BadgeEvaluationService.EvaluateHiddenAchievementsAsync()` — دالة واحدة بتفحص الـ 6 مع بعض، بتتنادى Best-effort من `DynastyController.Details()` دلوقتي (ينفع تتنادى من Trigger points تانية زي BookingStatusUpdater/QuizController لاحقًا لو حابب تزود دقة التوقيت).

> ✅ **القرار اتقفل (28 يوليو):** اتلغى تقنيع "؟؟؟" للشارات السرية خالص. دلوقتي الاسم والوصف بيظهروا عادي حتى وهي مقفولة، وباهتة/Grayscale بس (زي أي بادج مقفول تاني) — و`CurrentProgress` بقى بيرجع رقمها الحقيقي (مفيش تصفير عمدي، مفيش داعي طالما مفيش سر بنحميه أصلاً). التعديل ده في `BadgeEvaluationService.GetDashboardBadgesAsync()`.

#### 🎨 إعادة تصميم الداشبورد (Dashboard.cshtml) — بعد أول تجربة فعلية
الشكل الأول كان كل الشارات في Grid واحد مبعثر (earned/locked/hidden مخلوطين مع بعض من غير ترتيب) — اتعمل:
- تجميع الشارات في أقسام حسب الـ Category (زيارات → معرفة → مجتمع → أسطوري → سري)، كل قسم بعنوانه وعداد "X / Y" مكتسب
- شكل الـ Tile اتظبط (Min-height ثابت، حواف ذهبية للمكتسبة، Spacing أوضح)
- **مفتاح ترجمة جديد فعليًا محتاج يتضاف** (عكس الرأي الأول اللي قال مش محتاجين، لأن التصميم اتغيّر لعناوين أقسام):

| Key | AR | EN |
|---|---|---|
| `Badge_Category_Visit` | زيارات | Visits |
| `Badge_Category_Knowledge` | معرفة | Knowledge |
| `Badge_Category_Community` | مجتمع | Community |
| `Badge_Category_Legendary` | أسطوري | Legendary |
| `Badge_Category_Hidden` | إنجازات سرية | Secret Achievements |

#### 🪟 Badge Details Modal — كل كارت بقى قابل للضغط (تحديث لاحق)
بناءً على طلبك، اتضاف Modal احترافي واحد (بيتعاد استخدامه لكل الشارات) بيتفتح لما تدوس على أي كارت في الداشبورد. بيعرض:
- الـ Tier ladder كامل (Bronze/Silver/Gold) مش بس آخر Tier اتكسب
- كل Tier: اسمه ووصفه (إيه المطلوب بالظبط) + تاريخ اكتسابه لو خدته
- الـ Tier الجاي: Progress bar حقيقي (تقدم/المطلوب)

**التعديلات:**
- `Services/BadgeEvaluationService.cs` — `BadgeDisplayItem` بقى فيه `List<BadgeTierInfo> Tiers`، و`GetDashboardBadgesAsync()` بيبنيها لكل شارة (كل Tier + هل اتكسب + تاريخه)
- `Views/User/Dashboard.cshtml` — كل `.badge-tile` بقى قابل للضغط (`onclick="openBadgeModal(...)"`)، بيانات كل الشارات (باللغة الحالية بس) بتتبني Server-side كـ JSON مرة واحدة (`window.__badgesData`) والـ JS بيملى الـ Modal منها من غير Round-trip للسيرفر
#### 🎨 أيقونات SVG مخصّصة بدل Font Awesome (تحديث لاحق)
كل الـ 13 شارة (بما فيها الـ 6 السرية) بقالها أيقونة SVG مرسومة يدويًا بنفس أسلوب الخط (Line-art) بلون واحد (`currentColor`) — بتورّث الذهبي من الـ CSS المحيطة تلقائيًا (كارت مكتسب = ذهبي كامل، مقفول = رمادي باهت مع باقي الكارت، من غير ما نكتب لون منفصل لكل حالة).

**ملف جديد:**
- `Models/BadgeIconSvg.cs` — Dictionary ثابتة (Badge.Key → SVG markup)، مع `Get(key)` وFallback لو مفتاح مش موجود

**اتعدلت:**
- `Views/User/Dashboard.cshtml` — الكارت وModal التفاصيل بقوا بيستخدموا `BadgeIconSvg.Get(b.Key)` بدل `b.IconClass` (Font Awesome). حجم الأيقونة اتظبط في CSS (`.badge-tile-icon svg` / `.badge-modal-icon svg`)

> ملاحظة: أيقونات ✓/🔒 الصغيرة جوه صفوف الـ Tier في الـ Modal (مش أيقونة الشارة نفسها) لسه Font Awesome — مقصود، لأنها أيقونات عامة مش خاصة بكل شارة، قولّي لو عايزها SVG كمان.

#### 🐛 Bug Fix — فجوة في نقاط تفعيل الشارات السرية (تحديث لاحق)
كان `EvaluateHiddenAchievementsAsync()` بيتنادى بس من `DynastyController.Details()`. المشكلة: 4 من الـ 6 شارات السرية أصلاً مالهاش علاقة بزيارة أسرة (Perfect Score/Streak Legend/Night Owl-quiz مرتبطين بالكويز، Museum Completionist/Night Owl-booking مرتبطين بالحجوزات) — فلو يوزر حقق شرطها بس معملش Details لأي أسرة بعد كده، الشارة ماكانتش هتتفتحله خالص.

**الحل:** إضافة نفس الاستدعاء لأماكن تانية:
- `Controllers/QuizController.cs` — بعد `EvaluateQuizMasterAsync()` في نفس الـ Trigger Point اللي بيسجل الكويز
- `Services/BookingStatusUpdater.cs` — بعد `EvaluateVisitAsync()` في نفس الـ Loop اللي بيحوّل الحجوزات لـ Visited

**قرار متعمّد:** مافيش استدعاء في `ReviewController.cs` — مفيش أي شارة من الـ 6 السرية مرتبطة بالريفيوهات، فإضافته هناك هتبقى فحص زيادة من غير فايدة. `Loyal Explorer` (عضو من سنة) مش مرتبطة بأكشن معين أصلاً — بتتفحص عرضًا من أي Trigger من التلاتة دول، اللي كافي كتغطية Best-effort.

#### 🎉 Badge Celebration Toast — إشعار فوري لما شارة تتفتح (تحديث لاحق)
قبل كده كان اليوزر مايعرفش إنه خد شارة جديدة غير لما يدخل الجرس بنفسه (أو الـ Badge count يتحدث بصمت كل 20 ثانية). دلوقتي فيه Toast احتفالي مميز (Gold glow + 🏆) بيظهر تلقائي.

**الطريقة:** بدل ما نضيف "type" جديد للـ Response أو نلمس `NotificationController.cs`، استخدمنا حاجة موجودة فعلاً: عنوان إشعار الشارة (`Badge_EarnedNotifTitle`) فيه رمز 🏆 ثابت مهما كانت اللغة — فالـ JS بيكتشف بيه أي إشعار شارة جديد من نفس الـ Polling الموجود أصلاً (كل 20 ثانية، لحد ما SignalR يتفعّل زي المخطط له من الأول). الإشعارات اللي اتعرضت كـ Toast بالفعل بتتسجل في `localStorage` عشان ميتكررش نفس التوست تاني لو الصفحة اتعمللها Refresh.

**اتعدلت:**
- `Views/Shared/_Layout.cshtml` — CSS جديد (`.pl-badge-toast`) + JS (`navNotifCheckNewBadges()` + `showBadgeCelebrationToast()`) مربوطين بنفس دورة الـ Polling الحالية

**⚠️ افتراض محتاج تأكيد:** الكود مبني على افتراض إن `/Notification/GetRecent` بيرجّع `{ id, title, message, isRead }` — زي ما هو مستخدم فعليًا في `navNotifLoadRecent()` الموجودة قبل كده. لو `NotificationController.cs` بيرجّع شكل مختلف (خصوصًا لو `title`/`message` بمفاتيح تانية)، هيحتاج تعديل بسيط. ابعتلي الملف لو عايز تأكيد 100%.


#### ⚠️ اعتماد لازم تتأكد منه في `Program.cs`
`DynastyController` بقى محتاج `BadgeEvaluationService` في الكونستركتور (كان قبل كده `AppDbContext` بس). لو الـ DI مسجلة أصلاً بـ `AddScoped<BadgeEvaluationService>()` من خطوات التفعيل الأساسية، مفيش حاجة إضافية مطلوبة.

#### 📁 الملفات اللي اتعملت/اتعدلت فعليًا
**جديد بالكامل:**
- `Models/ItemView.cs` — Id, UserEmail, Type, ItemId, ViewedAt
- `Models/BadgeIconSvg.cs` — أيقونات SVG مخصّصة لكل شارة

**اتعدلت:**
- `Models/AppDbContext.cs` — `DbSet<ItemView>` + Unique Index (UserEmail+Type+ItemId)
- `Controllers/DynastyController.cs` — Constructor بقى بياخد `BadgeEvaluationService` + `Details()` بقت async وبتسجل الزيارة + تفحص الشارات (بترالي/كاتش، Best-effort)
- `Models/BadgeCatalogSeed.cs` — إضافة `dynasty_expert` (3 Tiers) + الـ 6 Hidden Achievements (`IsHidden = true`) + تحديث تعليق شرط Legendary Explorer
- `Services/BadgeEvaluationService.cs` (namespace فعليًا `pharaohsLegacy.Models`) — إضافة `EvaluateDynastyExpertAsync()` + `EvaluateHiddenAchievementsAsync()` (الـ 6 مع بعض) + تحديث `requiredGoldKeys` في `EvaluateLegendaryAsync()` ليشمل `dynasty_expert` + تحديث `GetDashboardBadgesAsync()` (progress لـ dynasty_expert + إلغاء تقنيع "؟؟؟" للشارات السرية)
- `Views/User/Dashboard.cshtml` — إعادة تصميم تبويب الشارات بالكامل: تجميع بالـ Category + CSS جديد للـ Tiles + Modal تفاصيل + أيقونات SVG
- `Controllers/QuizController.cs` — إضافة `EvaluateHiddenAchievementsAsync()` بعد تسجيل الكويز (Bug fix)
- `Services/BookingStatusUpdater.cs` — إضافة `EvaluateHiddenAchievementsAsync()` بعد تحديث الحجوزات لـ Visited (Bug fix)
- `Views/Shared/_Layout.cshtml` — Badge Celebration Toast (CSS + JS)

**Migration مطلوبة (لسه محتاجة تتعمل يدويًا):** `Add-Migration AddItemViewsAndSecretBadges` ثم `Update-Database`

**ترجمة:** أسماء/وصف الشارات كلها bilingual جوه `BadgeCatalogSeed.cs` مباشرة (مفيش مفاتيح ترجمة ليها). لكن بعد إعادة تصميم الداشبورد بأقسام حسب الـ Category، بقينا محتاجين فعليًا الـ 5 مفاتيح الجديدة اللي فوق (`Badge_Category_*`).

### 📁 الملفات اللي اتعملت/اتعدلت (خريطة كاملة)
**جديد بالكامل:**
- `Models/Badge.cs`، `Models/UserBadge.cs` — الـ Models
- `Models/BadgeCatalogSeed.cs` — كتالوج الشارات (Seed data)
- `Services/BadgeEvaluationService.cs` — منطق الفحص/المنح/الإشعار + `GetDashboardBadgesAsync()` لعرض الداشبورد (بيرجع `BadgeDisplayItem` — DTO فيه حالة كل شارة لليوزر: اتفتحت؟ التقدم؟ الـ Tier الجاي؟)

**اتعدلت:**
- `AppDbContext.cs` — `DbSet<Badge>` + `DbSet<UserBadge>` + الـ Seed جوه `OnModelCreating`
- `ReviewController.cs` — `Add()` بيفحص Reviewer Badge، `ToggleHelpful()` بيفحص Community Helper Badge (لصاحب الريفيو، مش اللي داس الزرار)
- `FavoriteController.cs` — `Add()` بيفحص Pharaoh Expert لما النوع يكون `pharaoh` (اتضاف `BadgeEvaluationService` للـ constructor، الكنترولر مكانش فيه أصلاً)
- `QuizController.cs` — `Answer()` بيفحص Quiz Master بعد ما `QuizHistory` يتسجل
- `BookingStatusUpdater.cs` — بعد ما الحجوزات تتحول Visited، بيفحص Explorer Badge لكل يوزر اتأثر (مرة واحدة لكل إيميل مش لكل حجز) — ⚠️ إشعار إنجليزي ثابت، نفس قرار `ShopOrderShippingBackgroundService` بالظبط (الـ Background Service مالوش Session/LocalizationService)
- `UserController.cs` — `Dashboard()` بقى بيجهز `ViewBag.Badges` (List<BadgeDisplayItem>) و`ViewBag.EarnedBadgeCount`
- `Views/User/Dashboard.cshtml` — 🆕 تاب "Badges" جديد كامل (Grid كروت، فاتحة/باهتة حسب الحالة، progress bar للـ Tier الجاي) + الـ **Explorer Badge الـ hardcoded القديم في تاب Profile اتحول فعليًا** لجزء من النظام الجديد (بياخد الاسم/الأيقونة من `ViewBag.Badges` بدل الـ thresholds الثابتة اللي كانت 1/5/10 — دلوقتي بتتبع الـ Tiers الجديدة 3/7/15)

> 💡 ملاحظة جانبية اتلاحظت وقت التعديل (مش حاجة اتصلحت): فيه باگ موجود من قبل في الـ Hash Routing JS بتاع `Dashboard.cshtml` — `tabIndex` مبني على افتراض إن كل التابات `<button>`، لكن 3 تابات (My Orders/My Coupons/Trip Plans) عبارة عن `<a>` مش أزرار، فترقيم `.db-tab` الحقيقي في الـ DOM مش مطابق للأرقام في `tabIndex`. لسه من غير تصليح.

### ✅ خطوات التفعيل اليدوية — اتعملت بالكامل
1. **`Program.cs`**: ✅ `builder.Services.AddScoped<BadgeEvaluationService>();` اتضافت
2. **Migration**: ✅ `Add-Migration AddAchievementsAndBadges` + `Update-Database` اتعملت
3. **مفاتيح ترجمة جديدة في `ar.json`/`en.json`**: ✅ اتضافت

| Key | AR | EN |
|---|---|---|
| `Badge_EarnedNotifTitle` | مبروك! خدت شارة {0} 🏆 | Congrats! You earned the {0} badge 🏆 |
| `Badge_EarnedNotifMessage` | خدت شارة {0} ({1}). تقدر تشوفها في البروفايل بتاعك. | You earned the {0} badge ({1}). Check it out in your profile. |
| `Dash_Tab_Badges` | الإنجازات | Badges |

---

## 📊 Analytics Dashboard (Admin) — بند 13 — ✅ مكتمل بالكامل

> الفيتشر ده خلص تمامًا واتأكد إن الـ 5 Charts ظاهرة وشغالة صح في الـ Admin panel. القسم ده بقى توثيق لما اتعمل فعليًا — عشان في أي شات جديد تبعتلي الملف وأنا أبقى فاهم إحنا واقفين فين.

### 🎯 النطاق (الـ 5 أجزاء)
1. **Revenue Trend** — Line chart آخر 30 يوم، Bookings + Shop مع بعض (مش Payments — نفس منطق `TotalRevenue` الأصلي اللي بيحسب من `Bookings.TotalPrice` مباشرة)
2. **Most Booked Places** — Bar chart لأعلى 5 أماكن حجزًا
3. **User Growth** — Line chart آخر 30 يوم، تسجيلات جديدة من `Users.CreatedAt` (عمود جديد بالكامل، اتضاف في الخطوة دي)
4. **Reviews Stats** — كروت (متوسط عام + أعلى/أقل عنصر تقييمًا) + Bar chart لمتوسط التقييم لكل Type
5. **Quiz Stats** — كروت (لاعبين، Total Plays، متوسط الدرجة، متوسط الـ Streak) + Bar chart لتوزيع الـ Grades

### ✅ قرارات اتاخدت وطُبّقت فعليًا
| القرار | التفاصيل |
|---|---|
| مصدر الـ Revenue | `Bookings.TotalPrice` + `ShopOrders.TotalPrice` (Status = Confirmed/Visited) — مش جدول `Payments` منفصل، عشان يتوافق مع منطق `TotalRevenue`/`TotalShopRevenue` الموجودين أصلاً |
| `Users.CreatedAt` لليوزرز القدام | `defaultValueSql: "GETDATE()"` — كلهم هيبانوا في نقطة واحدة (يوم الـ Migration) على الـ Growth chart، وأي يوزر جديد بعد كده بتاريخه الحقيقي |
| مكتبة الـ Charts | **Chart.js 4.4.4** عبر CDN (`cdn.jsdelivr.net`) في `<head>` بتاع `Admin/Index.cshtml` |
| Reviews name-lookup | بدل ما نعمل query لكل Review، حوّشنا `Pharaohs/Temples/Museums/Gods` كـ `local var` قبل الـ `vm` initializer واستخدمناهم في الاتنين (تجنب queries زيادة) |
| JSON casing defensive fix | دالة JS `lowerFirstKeys()` بتطبّع مفاتيح الـ JSON (PascalCase أو camelCase) عشان الشارتس تشتغل صح مهما كان إعداد `Program.cs` |
| Charts init timing | `initAnalyticsCharts()` بتتنفذ أول مرة بس اليوزر يفتح تاب Analytics (مش عند تحميل الصفحة) — لأن Chart.js محتاج الـ `<canvas>` يكون ظاهر عشان يحسب المقاسات صح |

### 📁 الملفات اللي اتعدلت (نسخ نهائية اتبعتت)
- `ViewModels/AdminViewModel.cs` — 5 DTOs جديدة (`RevenuePoint`, `PlaceBookingCount`, `UserGrowthPoint`, `ReviewsSummary`, `QuizSummary`, `TypeRatingAvg`, `GradeCount`) + خصائصهم في `AdminOverviewViewModel`
- `Controllers/AdminController.cs` — منطق حساب الـ 5 أجزاء جوه `Index()`
- `Views/Admin/Index.cshtml` — Chart.js CDN + Sidebar item + `panel-analytics` + JS لكل الـ 5 Charts
- `Models/User.cs` — عمود `CreatedAt` جديد (`[Column(TypeName = "datetime")]`)
- `Controllers/UserController.cs` — `Register` (POST) بقى بيحط `CreatedAt = DateTime.Now` صراحة
- `Migrations/20260724074313_AddUserCreatedAt.cs` — ⚠️ **درس مهم:** أول نسخة اتولّدت من `Add-Migration` كانت حاطة `defaultValue: new DateTime(1,1,1,...)` (سنة 1 ميلادي!) مش `GETDATE()` — لازم تتصحح يدويًا كل مرة الـ EF يولّد Default لعمود DateTime جديد، لأنه مش بياخد الـ default من الـ `= DateTime.Now` في الـ Model تلقائيًا وقت الـ Migration نفسها

### 🔗 Overlap مع بند تاني في الروادماب
البند **"Admin Financial Dashboard + Revenue Tracking + Revenue Forecasting"** (تحت قسم البنك) اتقفل جزئيًا مع الجزء ده (Revenue Trend chart بيغطّي جزء كبير منه).

---

## 🎫 Email Confirmation + QR Code (بند 12) — ✅ مكتمل بالكامل

> الفيتشر ده خلص تمامًا. القسم ده بقى توثيق لما اتعمل فعليًا — عشان في أي شات جديد تبعتلي الملف وأنا أبقى فاهم إحنا واقفين فين.

### 🎯 الفكرة الأساسية
- **Email Confirmation:** إيميل تأكيد حقيقي بيتبعت بعد نجاح الدفع (نفس مبدأ إرسال الـ OTP الموجود أصلاً في `email_service.py` بتاع البنك).
- **QR Code:** مش مجرد كود بيتعرض، ده **E-Ticket حقيقي** — زي تذكرة طيران/حفلة. اليوزر بيفتح الكود بالتليفون ويلاقي تذكرته (الاسم، الإيميل، عدد التذاكر، التوتال، حالة الدفع).

### ✅ قرارات اتاخدت وطُبّقت فعليًا
| القرار | التفاصيل |
|---|---|
| **محتوى الـ QR** | **مش** بيانات الحجز خام جوه الكود. الكود بيشيل **لينك فريد بس** (`/Booking/Ticket/{id}?token=...`) |
| **مصدر بيانات التذكرة** | Live من الداتابيز وقت فتح اللينك — لو الحجز اتلغى بعدين، التذكرة تعرض الحالة الصح فورًا |
| **الأمان** | حقل `TicketToken` (Guid?) في `Booking.cs` — بيتولد لما الحجز يبقى `Confirmed` لأول مرة (جوه `Confirm()` في `BookingController.cs`)، وبرضه لو رجع Confirmed من تراجع عن إلغاء (جوه `BookingStatusService.ToConfirmedAsync`) |
| **المكتبة** | `QRCoder` (NuGet, v1.6.0) — بتحول اللينك لصورة PNG مباشرة عن طريق Action منفصل `TicketQr` |
| **مكان الظهور** | زرار "🎫 عرض التذكرة" جنب كل حجز `Confirmed` في **مكانين**: `MyBookings.cshtml` و`Dashboard.cshtml` (الـ Bookings tab) |

### 🗂️ الملفات اللي اتعدلت (خريطة كاملة)

**ASP.NET Core (الموقع الرئيسي):**
- `Models/Booking.cs` — إضافة `public Guid? TicketToken { get; set; }`
- `Services/BookingStatusService.cs` — توليد التوكن لو الحجز رجع Confirmed من إلغاء (`ToConfirmedAsync`)
- `Controllers/BookingController.cs` —
  - توليد التوكن أول مرة في `Confirm()` بعد نجاح الدفع
  - Action جديد `Ticket(int id, string token)` — بيرجع صفحة التذكرة (بيتحقق من تطابق التوكن قبل أي حاجة)
  - Action جديد `TicketQr(int id, string token)` — بيرجع صورة QR كـ PNG مباشرة (لينكها هو نفس رابط `Ticket`)
  - في نهاية `Confirm()` (بعد نجاح الدفع)، نداء best-effort لـ `notifications/booking-confirmation` في خدمة البنك — جوه `try/catch` عشان لو الإيميل فشل الحجز يفضل ناجح عادي
- `Views/Booking/Ticket.cshtml` — 🆕 View جديد بالكامل، صفحة التذكرة (الاسم/الإيميل/التاريخ/عدد التذاكر/التوتال/الحالة + صورة QR)
- `Views/Booking/MyBookings.cshtml` — زرار "🎫 عرض التذكرة" (`.btn-ticket` CSS جديد) جنب أي حجز Confirmed
- `ViewModels/DashboardViewModel.cs` — إضافة `TicketToken` لـ `BookingCardViewModel`
- `Controllers/UserController.cs` — تمرير `TicketToken` من `Booking` لـ `BookingCardViewModel` جوه `Dashboard()`
- `Views/User/Dashboard.cshtml` — زرار "🎫 عرض التذكرة" (`.btn-gold`) جنب زرار الإلغاء في الـ Bookings tab
- `pharaohsLegacy.csproj` — إضافة `<PackageReference Include="QRCoder" Version="1.6.0" />`
- Migration: `AddTicketTokenToBooking` ✅

**Localization (لازم تتضاف يدويًا في `wwwroot/lang/ar.json` و`en.json`):**
- `Booking_ViewTicketBtn` = "عرض التذكرة" / "View Ticket" (لـ `MyBookings.cshtml`)
- `Dash_ViewTicket` = "عرض التذكرة" / "View Ticket" (لـ `Dashboard.cshtml`)

**Bank Service (Python/FastAPI):**
- `email_service.py` — دالة جديدة `send_booking_confirmation_email()`، نفس أسلوب `send_otp_email()` بالظبط (SMTP + نص وHTML بالتصميم الدهبي)
- `schemas.py` — Schema جديد `BookingConfirmationEmailRequest` (section جديد "Notifications")
- `main.py` — Endpoint جديد `POST /notifications/booking-confirmation` (section جديد "Notifications"، بين `/payments/refund` و`/coupons/create`) — مش عملية بنكية، مفيش account ولا Transaction، مجرد إرسال إيميل

### 🔍 Flow الإيميل بالظبط (عشان الرجوع له بسرعة)
1. الدفع ينجح في `Confirm()` (C#)
2. `Confirm()` بينادي `POST /notifications/booking-confirmation` في خدمة البنك، ببيانات الحجز + لينك التذكرة
3. الـ endpoint بينادي `send_booking_confirmation_email()`
4. الدالة دي بتبعت SMTP مباشر (نفس `.env` بتاع OTP: `SMTP_EMAIL` / `SMTP_PASSWORD`)
5. الإيميل فيه نسخة نص + HTML، وفيه زرار "🎫 عرض التذكرة" بيودّي على `/Booking/Ticket/{id}?token=...`

### ⚠️ حاجات لسه محتاجة قرار (مش من ضمن السكوب الحالي)
- شكل الـ Shop (لو هيتطبق نفس الفكرة على أوردرات المتجر مش الحجوزات بس) — لسه مقرّرناش.
- ✅ تصميم الإيميل اتقرر: HTML Template احترافي (مش نص عادي) — نفس تصميم الـ OTP الدهبي.

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
- [x] Booking Coupons + Promo Codes → ✅ خلصت (Coupon من الكويز) — تفاصيل في `BANK_SHOP_QUIZ_DETAILS.md` | [ ] Loyalty Discounts لسه مستقبلي
- [ ] Multi-currency Booking + Booking Analytics
- [ ] Real-time Capacity Counter
- [ ] AI Suggested Visit Times + Weather-aware Booking Suggestions

---

## 💳 Payment & Banking System

> ✅ النسخة الأساسية خلصت بالكامل ومتستنجة — التفاصيل والقرارات النهائية في `BANK_SHOP_QUIZ_DETAILS.md`.
> اللي تحت ده مرجع للأفكار الإضافية المستقبلية بس (بعد النسخة الأساسية).

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

## 🧭 AI Trip Planner — خطة العمل الكاملة (✅ **قُفل بالكامل** — Model + Controller + الـ 3 Views + الترجمة (عربي/إنجليزي) + Dark/Light Mode + تكامل Dashboard + تصحيح الأسعار الحقيقية + إلغاء Pharaoh/God + PDF Export (مع اختيار اللغة) + مودالات حذف مصممة خلصوا كلهم. الـ checklist النهائي اللي لازم صاحب المشروع يتأكد منه بنفسه تحت في آخر الـ section)

> اتفقنا على كل التفاصيل دي قبل ما نبدأ أي شغل. الجزء ده بيتحدث أول ما بنخلص كل خطوة فعليًا.

### 🎯 الـ Scope النهائي لأول نسخة (v1) — كل حاجة مع بعض
- [ ] Text Itinerary (الأساس) — اليوزر يدخل days / budget / interests / mode → الـ AI يرجع خطة رحلة يوم بيوم
- [ ] Interactive Trip Map (Leaflet.js — نفس المكتبة المستخدمة أصلاً في خريطة المعابد والمتاحف)
- [ ] PDF Export للخطة
- [ ] Modes: Family / Student / Luxury

### 🔒 قرارات اتاخدت (نهائية، مش هترجع فيها من غير سبب)
| القرار | الاختيار | السبب |
|---|---|---|
| مصدر التوصيات | **من الداتا بيز بس** (Temples/Museums/Pharaohs/Gods الموجودين فعلاً) | عشان اللينكات والحجز (Booking) يشتغلوا صح على الأماكن اللي الـ AI يقترحها |
| AI Service | **نفس Groq + LLaMA 3.1 اللي شغال في الـ AI Tour Guide Chatbot حالياً** (مش local model، ومش service منفصل) | مفيش هوستينج إضافي، الـ integration ده مجرب وشغال أصلاً، والمشروع لسه محلي فأولوية السرعة والبساطة |
| حفظ الخطة | **أيوه، بتتحفظ في الداتا بيز** (جدول جديد TripPlans) عشان اليوزر يرجعلها تاني من MyTripPlans | — |
| مكتبة الـ PDF | **QuestPDF** (Free/Community license) — مفيش مكتبة PDF مضافة في المشروع قبل كده | Clean C# API + رخصة مجانية للمشاريع الصغيرة/الطلابية |
| مشكلة الإحداثيات | ~~هنحتاج Geocoding~~ — **✅ اتأكد إن Temple.cs أصلاً فيه Latitude/Longitude، وكل الـ 29 معبد متملية بقيم حقيقية في الداتا بيز فعليًا (اتأكد بالصورة من SSMS بتاريخ 24 يوليو).** الملاحظة القديمة هنا كانت غلط/قديمة. **مفيش داعي لسكريبت Geocoding خالص، الخطوة دي اتشالت من الخطة.** | — |

### 🗄️ الجداول الجديدة المقترحة
| Table | Fields |
|---|---|
| TripPlans | Id, UserEmail, Days, Budget, Interests, Mode (Family/Student/Luxury), CreatedAt |
| TripPlanStops | Id, TripPlanId (FK), DayNumber, PlaceType (Temple/Museum/Pharaoh/God), PlaceId, PlaceName, SuggestedTime, EstimatedCost, Notes |

### 🎮 الـ Controller المقترح: `TripPlannerController`
- Index (GET) — الفورم: days, budget, interests, mode
- Generate (POST) — بيجيب أماكن مرشحة من الداتا بيز حسب الـ interests، يبني الـ prompt، يكلم الـ AI، يحلل الرد (JSON بس، بيرجع IDs حقيقية من الداتا بيز)، يحفظ النتيجة، يودي اليوزر لصفحة النتيجة
- Result/Details/{id} (GET) — عرض الخطة يوم بيوم + خريطة Leaflet + زرار Export PDF
- MyTripPlans (GET) — الخطط المحفوظة السابقة لليوزر
- Delete (POST)
- ExportPdf/{id} (GET) — توليد وتنزيل PDF بالـ QuestPDF

### 📁 Views المقترحة
```
Views/TripPlanner/
├── Index.cshtml       ← فورم الإدخال
├── Result.cshtml       ← الخطة + الخريطة + زرار PDF
└── MyTripPlans.cshtml  ← الخطط المحفوظة
```

### 🪜 ترتيب التنفيذ المتفق عليه
1. ✅ `TripPlan.cs` + `TripPlanStop.cs` + تسجيلهم في `AppDbContext.cs` — **خلص**. محتاج تشغّل `Add-Migration AddTripPlanner` ثم `Update-Database` عندك.
   - `TripPlan`: Id, UserEmail, Days, Budget (decimal 18,2), Interests, Mode, CreatedAt, + navigation `Stops` (متهيأة `= new()` ✅)
   - `TripPlanStop`: Id, TripPlanId (FK), DayNumber, PlaceType, PlaceId, **PlaceName (عمود حقيقي مخزّن، مش NotMapped زي Booking — snapshot وقت التوليد)**, SuggestedTime, EstimatedCost (decimal 18,2), Notes
   - ✅ **راجعت الملفين فعليًا (24 يوليو) — كل الـ types والأسماء متطابقة 100% مع `TripPlannerController.cs`، مفيش أي تعديل مطلوب فيهم.**
2. ~~سكريبت Geocoding للمعابد~~ — **اتشالت، مش لازمة** (الإحداثيات موجودة فعلاً في الداتا بيز)
3. ✅ **خلص:** `TripPlannerController.cs` — كامل (Index, Generate, Result, MyTripPlans, Delete) + منطق الـ AI Prompt (reuse لنفس Groq service بنفس نمط `ChatbotController.cs` بالظبط: نفس الـ endpoint، نفس `GroqApiKey` من الـ config، نفس موديل `llama-3.1-8b-instant`). راجعته وموجود فيه:
   - جلب candidates من الداتا بيز بس (Temple/Museum/Pharaoh/God) حسب الـ interests
   - System prompt بيجبر الـ AI يرجع JSON خام بس + يستخدم IDs من الـ candidates بس
   - Validation بعد رد الـ AI: أي `(PlaceType, PlaceId)` مش موجود فعليًا في candidates بيتم تجاهله (حماية من الـ hallucination) — وبيتخزن اسم المكان الحقيقي من الداتا بيز مش اسم الـ AI
   - ⚠️ **ملاحظات تحسين مؤجلة (مش blocking، نرجعلها بعدين):**
     - مفيش فحص `response.IsSuccessStatusCode` قبل قراءة رد Groq (لو فشل الـ request هيدي رسالة error مش واضحة)
     - مفيش whitelist check على `request.Mode` (`Family/Student/Luxury`)
     - مطابقة `PlaceType` بين رد الـ AI والداتا بيز حساسة لحالة الأحرف (case-sensitive) — ممكن نضيف normalize احتياطي
4. ✅ **خلص:** الـ Views التلاتة (`Index.cshtml`, `Result.cshtml`, `MyTripPlans.cshtml`)
   - ✅ **قرار اتاخد (24 يوليو):** تسمية الـ Interests في كل حتة في المشروع هتكون **بالمفرد** (`Temple`, `Museum`, `Pharaoh`, `God`) — من غير "s" — لأنها هي المستخدمة فعليًا جوه `TripPlannerController.Generate()`. يعني الـ checkbox values في `Index.cshtml` لازم تتكتب بالظبط `value="Temple"` مش `value="Temples"`، وإلا الفلترة بتفشل بصمت ("مفيش أماكن متاحة" حتى لو اليوزر فعلاً اختار).
   - ⚠️ **قرار جديد بيلغي جزء من قرار 24 يوليو (25 يوليو):** `Pharaoh` و `God` اتشالوا نهائيًا من الـ Interests — مش أماكن ليها موقع/إحداثيات تتزار زي المعابد والمتاحف، فمنطقيًا معندهمش مكان في تخطيط رحلة. اتشالوا من:
     - `Index.cshtml` — الشيبتين اتمسحوا من فورم الاهتمامات
     - `MyTripPlans.cshtml` و `Result.cshtml` — اتمسحوا من `interestLabels` / `placeGlyphs` / `placeControllers`
     - `TripPlannerController.cs` — اتمسح الكود اللي بيجيب `Pharaohs`/`Gods` من الداتا بيز كـ candidates، وتعليق الـ system prompt بتاع الـ AI اتعدّل عشان يبطّل يذكرهم
   - ملاحظة صغيرة: التعليق جوه `TripPlan.cs` بيدي مثال بالجمع (`"Temples,Museums,Gods"`) — التعليق بس هو الغلط، مش الكود؛ محتاج يتصحح لاحقًا ليطابق القرار ده (مش عاجل، تعليق بس مش منطق).
   - `Index.cshtml` ✅ — فورم days/budget/mode/interests بالـ Egyptian dark-gold theme (CSS vars بـ prefix `tp-`)، stepper للأيام، mode cards (Family/Student/Luxury)، interest chips
   - `Result.cshtml` ✅ — خريطة Leaflet.js (نفس المكتبة المستخدمة في خريطة المعابد/المتاحف) + خطة يوم بيوم + زرار PDF (✅ بقى شغال، شايف Export.PDF أول ما خلصنا الخطوة 5) + زرار حذف (CSS vars بـ prefix `tpr-`)
   - `MyTripPlans.cshtml` ✅ — Grid من كروت الخطط المحفوظة (أيام/ميزانية/عدد محطات/اهتمامات/تاريخ) + Empty state + زراير عرض/حذف (CSS vars بـ prefix `tpm-`)
   - راجعت الكود الفعلي بتاع `TripPlannerController.cs` (24 يوليو) وطابقت الـ Views عليه سطر بسطر — الموديل والـ actions والـ routes كلهم متطابقين 100%
   - ✅ **باگ اتصحح:** `TripPlannerController.MyTripPlans()` كانت مفيهاش `.Include(p => p.Stops)` (فـ "📍 0 محطة" كان بيظهر غلط). **راجعت نسخة اليوزر (25 يوليو) — `.Include(p => p.Stops)` موجودة فعليًا دلوقتي، الباگ مش موجود.**
5. ✅ **خلص:** الترجمة الكاملة (عربي/إنجليزي) للـ 3 Views بتاعت الـ Trip Planner
   - كل نص ثابت (labels, hints, أزرار, رسائل تأكيد الحذف, نص الخريطة الفاضية) بقى بينادي `Html.L("key")` بدل نص عربي/إنجليزي مكتوب مباشرة
   - الأرقام (أيام، ميزانية، تكلفة) بتتحول بـ `Html.Num(...)` والتاريخ بـ `Html.DateLoc(...)` — نفس الـ helpers الموجودة أصلاً في `HtmlHelperExtensions.cs`
   - 🆕 اتضافت method جديدة `Html.LF(key, args...)` في `HtmlHelperExtensions.cs` (بتنادي `LocalizationService.GetFormatted` اللي كانت موجودة أصلاً بس مش متاحة من الـ View) — لجمل فيها متغيرين زي "رحلة {0} أيام — {1}" أو "≈ {0} جنيه لهذا اليوم"
   - ~55 مفتاح ترجمة جديد اتضافوا لـ `ar.json` و `en.json` (بادئة `trip.*`)
6. ✅ **خلص:** إصلاح Dark/Light Mode
   - الـ 3 Views كانت بتستخدم `:root` مستقل بألوان hex ثابتة (`--tp-*`, `--tpm-*`, `--tpr-*`) من غير ما تسمع لزرار الـ toggle — اتضاف `html[data-theme="light"]` override لكل واحد فيهم بنفس الـ palette المستخدم في باقي الموقع
   - في `pharaoh.css`: `#back-to-top`, `.btn-fav` (+ `.active`), `.fav-guest-msg`, `.hero-stars` كانوا بألوان hex ثابتة تمامًا (مش بتستجيب للـ toggle خالص) — اتبدّلوا بـ `var(--gold)`, `var(--border)`, `var(--muted)`, `var(--dark3)`, `var(--gold-rgb)` اللي أصلاً معرّفة ومربوطة بـ light mode في `_Layout.cshtml`
   - ⚠️ ملاحظة صغيرة مؤجلة: فيه `#toast` (id) قديم لسه موجود في `pharaoh.css` بجانب `.pl-toast` الجديد في الـ Layout — شكله كود ميت مش مستخدم، محتاج تتأكد ولو فعلاً مش مستخدم نقدر نشيله
7. ✅ **خلص:** تكامل مع User Dashboard
   - تاب جديد "خطط رحلتي" 𓊪 اتضاف في `Dashboard.cshtml` جنب "My Orders" و "My Coupons" — لينك مباشر لـ `/TripPlanner/MyTripPlans` (مش تاب داخلي بيبدّل بانل، بنفس أسلوب Orders/Coupons)
   - `UserController.Dashboard()`: اتضاف query بسيط بيحسب `ViewBag.TotalTripPlans` (عدد خطط اليوزر) بيتعرض كـ badge على التاب، بنفس أسلوب `TotalOrders`/`TotalCoupons` الموجودين أصلاً
   - محتاج مفتاح ترجمة جديد `Dash_Tab_TripPlans` في `ar.json`/`en.json`
8. ✅ **خلص:** تصحيح مشكلة الأسعار الوهمية
   - ⚠️ **كان فيه باگ:** الـ AI كان بيتقال له "estimate" السعر بنفسه لكل محطة (حتى Temple/Museum اللي ليهم سعر حقيقي في جدول `Prices`) — طلع مثال حقيقي: هرم خوفو سعره الحقيقي 700 جنيه، والـ AI رجع 150
   - الإصلاح على مستويين: (1) بنجيب الأسعار الحقيقية من `Prices` قبل ما نكلم الـ AI ونحطها في الـ candidates كـ `TicketPrice`، ونقول له صراحة يستخدمها زي ما هي بدل التخمين (وده كمان بيخلي حساب الميزانية بتاعه أدق)، (2) شبكة أمان بعد رد الـ AI: أي Temple/Museum ليه سعر حقيقي بنفرضه فوق أي رقم رجع من الـ AI. الـ Pharaoh/God (مفيهمش تذاكر فعلية) بيفضلوا على تقدير الـ AI
9. ✅ **خلص:** PDF Export بالـ QuestPDF
   - ✅ ملف جديد `Services/TripPlanPdfBuilder.cs` — بيبني الـ PDF كامل بالـ QuestPDF Fluent API، بيدعم عربي (RTL عبر `page.ContentFromRightToLeft()` + فونت Amiri) وإنجليزي (LTR + Arial). فيه Header (اسم الموقع + تاريخ الإنشاء)، Summary pills (أيام/ميزانية/تكلفة تقديرية/mode)، كارت لكل يوم بمحطاته (اسم/وقت/سعر/ملاحظات)، Footer برقم الصفحة. الأرقام بتتحول لهندي-عربي لو اللغة عربي (نسخة مبسطة من فكرة `Html.Num` تشتغل من غير `IHtmlHelper`). بيستخدم مفاتيح ترجمة موجودة أصلاً (`trip.mode.*`, `trip.interest.*`, `trip.currency`, `trip.result.day.label`) + 3 مفاتيح جديدة (`trip.pdf.title`, `trip.pdf.generated`, `trip.pdf.footer`)
   - ✅ `TripPlannerController.cs` — اتضاف `LocalizationService` في الـ constructor، وأكشن `ExportPdf(int id, string? lang = null)` (GET) بنفس فحص ملكية الخطة بتاع `Result()`
   - ✅ `Program.cs` — تسجيل `QuestPDF.Settings.License = LicenseType.Community` + تسجيل فونت Amiri (Regular/Bold) من `wwwroot/font/` عبر `FontManager.RegisterFont` وقت الـ startup
   - ⚠️ **باگ اتكتشف من نسخة PDF فعلية اتبعتت (25 يوليو):** حرف "ي" كان بيختفي من أي كلمة عربية ملزوقة برقم (`"٥ أيام"` طلعت `"٥ أ م"`، `"يوم ١"` طلعت `"١ ام"`). السبب: محرك الـ Arabic shaping بتاع QuestPDF/SkiaSharp بيكسر عند تقاطع حرف عربي + رقم في نفس الـ `Text()` call الواحد، حتى لو اتقسّموا بـ `.Span()` منفصلة
   - ✅ **الفيكس اتأكد إنه شغال (25 يوليو):** `TripPlanPdfBuilder.cs` اتعاد كتابته — أي رقم ولزيقه كلمة عربية بقى في عنصرين `Text()` منفصلين تمامًا جوه `Row()`. **صاحب المشروع عمل rebuild وبعت لقطة فعلية من الـ PDF بعد الفيكس — "اليوم ١" و"٥ أيام" ظهروا صح بالكامل، اتأكد بصريًا**
   - 📌 **قاعدة اتسجلت في تعليق أعلى الملف** عشان متتنساش لو حد ضاف نص جديد بعدين: أي رقم + كلمة عربية = عنصرين `Text()` منفصلين، مش string واحد ولا `.Span()` في نفس الـ `Text()`
   - ⚠️ **مش متأكد بتجربة فعلية:** مسار الإنجليزي (`lang="en"`) — الكود مكتوب بنفس منطق العربي (مُتأكد منه) بس معرفتش أختبره فعليًا (معنديش .NET SDK). صاحب المشروع لازم يجرب الزرار ويتأكد
10. ✅ **خلص:** مودالات حذف مصممة بدل `confirm()` الافتراضي بتاع المتصفح
    - `MyTripPlans.cshtml` (`.tpm-modal-*`) و `Result.cshtml` (`.tpr-modal-*`) — نفس الـ Egyptian dark-gold theme بتاع كل صفحة، بمودال overlay + blur + أنيميشن fade/scale، إغلاق بالدوس بره الصندوق أو Escape
    - `MyTripPlans.cshtml`: مودال واحد للصفحة كلها بيتتبع الفورم اللي اتضغط عليه (`btn.closest('form')`) لأن فيه أكتر من كارت/فورم حذف في نفس الصفحة
    - `Result.cshtml`: مودال ثابت (فورم واحد بس في الصفحة، `id="tprDeleteForm"`)، ودوال الفتح/القفل اتعممت (`tprOpenModal(id)` / `tprCloseModal(id)`) عشان تخدم أي مودال تاني يتضاف بعدين (زي بند 11)
    - مفتاحين ترجمة جديدين: `trip.delete.title`, `trip.delete.cancel` (النص التحذيري نفسه `trip.delete.confirm` كان موجود أصلاً)
11. ✅ **خلص:** اختيار لغة الـ PDF وقت التصدير
    - زرار "تصدير PDF" في `Result.cshtml` بقى بيفتح مودال (`tprPdfLangModal`) بخيارين: 🇪🇬 العربية / 🇬🇧 English — كل واحد لينك مباشر لـ `ExportPdf?lang=ar` أو `?lang=en` (مش تلقائي حسب لغة الموقع زي الأول)
    - `TripPlannerController.ExportPdf` بقى بياخد `lang` كـ query parameter، بيتأكد إنه `"ar"` أو `"en"` بس، ولو مش موجود/غلط بيرجع لمنطق `Session["Lang"]` القديم كـ fallback (اللينكات القديمة تفضل شغالة)
    - مفتاحين ترجمة جديدين: `trip.pdf.lang.title`, `trip.pdf.lang.subtitle` (أسماء اللغتين نفسها "العربية"/"English" اتسابت ثابتة مش عن طريق `Html.L` — أسماء لغات بتتكتب دايمًا بلغتها هي، مش نص UI بيتترجم)

### ✅ Checklist نهائي — لازم صاحب المشروع يتأكد منه بنفسه قبل ما يعتبر الـ feature جاهز للإنتاج فعليًا
(الحاجات دي معملتش عليها اختبار فعلي — معنديش .NET SDK ولا بيئة تشغيل حقيقية طول السيشن ده، غير اللقطة الوحيدة اللي بعتها صاحب المشروع بعد الفيكس)
- [ ] الـ 7 مفاتيح ترجمة الجديدة اتضافوا فعليًا لـ `ar.json` **و** `en.json`: `trip.pdf.title`, `trip.pdf.generated`, `trip.pdf.footer`, `trip.delete.title`, `trip.delete.cancel`, `trip.pdf.lang.title`, `trip.pdf.lang.subtitle`
- [ ] فونت `Amiri-Regular.ttf` / `Amiri-Bold.ttf` موجودين في `wwwroot/font/` (لو اتحركوا/اتمسحوا، المشروع كله مش هيشتغل من الـ startup مش بس الـ PDF)
- [ ] NuGet package `QuestPDF` متضاف فعليًا (`dotnet add package QuestPDF`)
- [ ] تجربة فعلية: تصدير PDF عربي ✅ (اتأكدت بلقطة)، تصدير PDF إنجليزي ⚠️ (لسه محتاج تجربة)، مودال الحذف في الصفحتين
- ⚠️ حاجات قديمة معروفة لسه من غير حل (مش من السيشن ده): مفيش whitelist check على `request.Mode` في `Generate()`، مفيش فحص `response.IsSuccessStatusCode` قبل قراءة رد Groq، تعليق `TripPlan.cs` لسه بمثال `"Temples,Museums,Gods"` القديم
- 💡 ملاحظة اختيارية: نص ملاحظة "Luxor Temple" في اللقطة اللي بعتها طلع عربي مكسور لغويًا ("الحيثات الكلية للاحيثات القديمة") — جاي من رد الـ AI نفسه مش من الـ PDF builder، ممكن يستاهل مراجعة الـ system prompt بتاع `Generate()` بعدين

### 📥 ملفات مطلوبة من صاحب المشروع قبل بداية الكود
- [x] AI Tour Guide Chatbot Service (`ChatbotController.cs`) — **اتراجع، الـ TripPlannerController بيستخدم نفس النمط بالظبط**
- [x] `AppDbContext.cs`
- [x] `Temple.cs`, `Museum.cs`, `Pharaoh.cs`, `God.cs`
- [x] `Program.cs`
- [x] `TripPlan.cs`, `TripPlanStop.cs`
- [x] `TripPlannerController.cs` كامل — **راجعته فعليًا (24 يوليو)، مطابق تمامًا لكل الـ Views**
- [x] `Index.cshtml`, `Result.cshtml` — رفعهم صاحب المشروع، راجعتهم وطابقت عليهم
- [x] `MyTripPlans.cshtml` — اتبنت (24 يوليو) بنفس الستايل والـ naming convention
- [x] `HtmlHelperExtensions.cs`, `LocalizationService.cs` — راجعتهم (25 يوليو) قبل شغل الترجمة، اتضافت `Html.LF(...)` جديدة في `HtmlHelperExtensions.cs`
- [x] `_Layout.cshtml`, `pharaoh.css` — راجعتهم (25 يوليو) لتشخيص وإصلاح مشكلة Dark/Light Mode
- [x] `UserController.cs`, `Dashboard.cshtml` — راجعتهم (25 يوليو) لإضافة تاب "خطط رحلتي" في الـ User Dashboard
- [x] `AppDbContext.cs`, `Price.cs` — راجعتهم (25 يوليو) لتصحيح مشكلة الأسعار الوهمية في `TripPlannerController.Generate()`

---

## 🔍 Smart Search — بند 16 — ✅ مكتمل بالكامل

### 🎯 النطاق المتفق عليه
- بحث موحّد لايف على كل الجداول السبعة: Pharaohs, Temples, Museums, Gods, Dynasties, Artifacts, HistoricalEvents
- Autocomplete أثناء الكتابة (debounce)
- Search History لكل يوزر
- Fuzzy matching — يتحمل غلطة إملائية بسيطة
- Trending Searches — محسوبة live من الـ History، من غير جدول منفصل

### ✅ اللي اتكتب فعليًا (نسخة أولى — ملفات جاهزة في المحادثة)
- **`SearchHistory.cs`** — موديل جديد: `Id, UserEmail (nullable), Query, SearchedAt, ResultType (nullable)`
- **`AppDbContext.cs`** — اتضاف `DbSet<SearchHistory> SearchHistories`
- **`HomeController.cs`** — تعديل كامل على الـ `Search` action + إضافات جديدة:
  - `HistoricalEvents` بقى جزء من نتيجة البحث الموحّد (كان ناقص)
  - كل جدول بيدور أول حاجة بـ `Contains` العادي (زي الأول)، ولو رجّع صفر نتائج بيرجع تاني بـ **Fuzzy fallback** (Levenshtein distance، يدوي من غير مكتبة خارجية)
  - `SearchSuggestions(string term)` — GET endpoint جديد للـ Autocomplete، بيرجع JSON (Take 8) من Pharaohs/Temples/Museums/Gods/Artifacts/Dynasties
  - `TrackSearchClick(query, resultType)` — POST endpoint جديد، بيسجل ResultType لما اليوزر يضغط على نتيجة
  - `LogSearch` — بيسجل كل بحث في `SearchHistories`
  - `GetRecentSearches` / `GetTrendingSearches` — بيرجعوا لـ `Search.cshtml` عن طريق `ViewBag.RecentSearches` / `ViewBag.TrendingSearches`
- **`Search.cshtml`** — اتضاف: قسم "بحثت عنه قبل كده" + "الأكتر بحثاً" فوق النتائج، قسم HistoricalEvents جديد، `onclick="trackSearchClick(...)"` على كل كارت نتيجة، وربط `smart-search.js`
- **`smart-search.js`** جديد — بيدور على `#smartSearchInput` + `#smartSearchSuggestions` في أي صفحة، debounce 300ms

### ⚠️ افتراضات اتحطت — ✅ اتأكدت
- `HistoricalEvent.cs` اتبعت وراجعته: عنده فعلاً `TitleAr`, `CategoryAr`, `DescriptionAr`. الـ `Search` action و`Search.cshtml` اتعدّلوا يستخدموا `TitleAr`/`CategoryAr` زي باقي الجداول (لايف + Fuzzy فالاتنين)
- الـ Fuzzy fallback بيعمل `ToList()` على الجدول كله لما الـ Contains يرجّع صفر بس — مقبول للأحجام الحالية (≤160 صف)، مش الحل لو الجداول كبرت أوي مستقبلاً

### ⏳ خطوات يدوية — كلها اتعملت ✅
- [x] حفظ `SearchHistory.cs` في `Models/`
- [x] Migration `AddSearchHistory` اتعملت وطُبّقت على الداتا بيز
- [x] إضافة `#smartSearchInput` + `#smartSearchSuggestions` + CSS الـ dropdown + تضمين `smart-search.js` في `_Layout.cshtml` — اتعمل مباشرة على الملف اللي اتبعت (مربع البحث في الـ Navbar)
- [x] مفاتيح الترجمة `Search_RecentSearches`, `Search_TrendingSearches`, `Common_HistoricalEventsPlural` اتضافوا في `wwwroot/lang/ar.json` و`en.json`
- [x] تأكيد حقول `HistoricalEvent.cs` — ✅ اتأكدت (`TitleAr`, `CategoryAr` موجودين واتضافوا للكود)

### 📌 مستبعد من السكوب الحالي (موجود في المرجع الكامل تحت بس مش دلوقتي)
- Semantic / Natural Language / Voice Search
- AI-powered Search (Groq)
- OCR Search from Images
- Search Analytics (Dashboard منفصل)

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
> ✅ النسخة الأساسية (Dynamic Quiz + Difficulty Levels + Coupon Rewards) خلصت بالكامل — التفاصيل في `BANK_SHOP_QUIZ_DETAILS.md`.
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

## 🔔 Notification System — بند 15 — ✅ مكتمل بالكامل

> كل الـ Triggers اتدمجت فعليًا (كود حقيقي جوه كنترولرز المشروع + الـ Background Service بتاع الشحن) وخطوات التفعيل اليدوية (Migration + استبدال الملفات + مفاتيح الترجمة في `ar.json`/`en.json`) اتعملت وخلصت. تفاصيل الدمج الكاملة في `INTEGRATION_NOTES.md`.

### 🎯 الفكرة الأساسية
بند 15 = **الأساس** بس (In-app Notification System بسيط وشغال بالكامل) — مش النسخة المتقدمة. الحاجات المتقدمة (SignalR Real-time، Push Notifications، Email Alerts، Smart Preferences) مؤجلة ومكانها قسم "🔔 Notification Ecosystem" تحت في المرجع الكامل، مش دلوقتي.

### ✅ قرارات اتاخدت (سيبها لتقديري)
| القرار | الاختيار |
|---|---|
| نطاق الـ Triggers | 5: Booking Confirmed، Booking Cancelled، حل Report على ريفيو، كوبون Quiz Streak، ترحيب بعد التسجيل — زائد تنبيه أدمن عند تأكيد أي حجز |
| Real-time ولا Polling | Polling بـ JS كل 20 ثانية (SignalR مؤجل لقسم "Notification Ecosystem" تحت) |
| إشعارات الأدمن | نفس جدول `Notifications`، بيتفلتر بـ `UserEmail = kamalabdlbast89@gmail.com` عبر `NotificationHelper.NotifyAdmin()` |
| أرشفة/حذف | من غير حذف تلقائي دلوقتي |

### ✅ اتعمل فعليًا (اتبعت للمشروع الحقيقي، مش demo)
- **`AppDbContext.cs`** — `DbSet<Notification> Notifications` موجود بالفعل ✅
- **`_Layout.cshtml`** — الجرس + الدروب داون + JS Polling اتدمجوا كاملين **جوه الملف نفسه مباشرة** (مش partial منفصل) — بيستخدم `@Html.L(...)` زي باقي الموقع بالظبط، مش نصوص Hardcoded
  - ⚠️ **`_NotificationBell.cshtml`** كان أول اقتراح (partial منفصل) قبل ما أشوف الـ `_Layout.cshtml` الحقيقي — **اتلغى ومش جزء من التسليم النهائي**، الجرس كله جوه `_Layout.cshtml` دلوقتي
- **`BookingController.cs`** — `Confirm()` (تأكيد الحجز لليوزر + تنبيه أدمن) و`Cancel()` (إلغاء، موضّح إن الاسترداد بعد 24 ساعة)
- **`ReviewController.cs`** — `ResolveReport()` (إشعار للمُبلّغ)
- **`UserController.cs`** — `Register()` (ترحيب) — الـ context field اسمها `context` مش `_context`، اتعامل معاها صح
- **`QuizController.cs`** — `Answer()`، بعد `_db.QuizHistories.Add(...)`: إشعار "مبروك! كسبت كوبون خصم" **بس لو `couponCode` اتولّد فعليًا** (مش لو الـ Streak اتحسب بس من غير كوبون)، وبعد الـ `SaveChangesAsync()` الأساسي علشان الـ QuizHistory يتسجل أولًا

كل الـ 5 نداءات جوه `try/catch` أو بعد الحفظ الأساسي مباشرة — Best effort زي إيميل التأكيد بالظبط، فشل الإشعار مبيوقفش أي عملية أساسية.

### 📋 باقي — خطوة يدوية واحدة بس ناقصة (اكتشفناها من فحص `LocalizationService.cs`)
نظام الترجمة بتاع المشروع عبارة عن ملفين JSON مسطحين: `wwwroot/lang/ar.json` و`wwwroot/lang/en.json` (Dictionary key → value، بيتقروا مرة واحدة في constructor بتاع `LocalizationService`). لازم تتضاف فيهم الـ 4 مفاتيح دول:
| Key | EN | AR |
|---|---|---|
| Nav_Notifications | Notifications | الإشعارات |
| Nav_MarkAllRead | Mark all as read | تحديد الكل كمقروء |
| Nav_NoNotifications | No notifications | لا توجد إشعارات |
| Nav_ViewAllNotifications | View all notifications | عرض كل الإشعارات |

بعد كده، خطوات التفعيل اتعملت وخلصت (26 يوليو):
1. ✅ `Add-Migration AddNotifications` + تصحيح `CreatedAt` لـ `GETDATE()` + `Update-Database`
2. ✅ استبدال `_Layout.cshtml` و`BookingController.cs`/`ReviewController.cs`/`UserController.cs`/`QuizController.cs`/`ShopOrderShippingBackgroundService.cs` بالنسخ المدموجة
3. ✅ إضافة كل مفاتيح الترجمة المطلوبة في `ar.json`/`en.json`
4. ✅ اختبار الـ Flow الكامل

### 🔗 Overlap مع بنود تانية في الروادماب
- "Auto Notification Rules + AI Content Prioritization" (تحت 🤖 AI Systems)
- "Real-time Notifications + Live Dashboard Updates" (تحت 📡 Real-time Systems)
- "🔔 Notification Ecosystem" بالكامل تحت — ده المرحلة المتقدمة اللي بند 15 بيبني الأساس ليها

### 🌐 تصليح الترجمة في الإشعارات (25 يوليو — اتعمل فعليًا)
اكتشفنا وقت الاختبار إن الترجمة (EN/AR) مكانتش شغالة صح في جزء الإشعارات، بسببين مختلفين اتصلحوا الاتنين:

1. **`Views/Notification/Index.cshtml`** — الصفحة كانت مبنية بالكامل بنصوص عربي Hardcoded (العنوان، زرار "تحديد الكل كمقروء"، رسالة "مفيش إشعارات لسه") من غير ما تستخدم `Html.L(...)` خالص. ✅ اتصلحت — كل النصوص بقت بتستخدم مفاتيح ترجمة، والتاريخ بقى بينادي `Html.DateLoc(...)` بدل `.ToString(...)` المباشر. محتاج تضيف مفتاحين جداد في `ar.json`/`en.json`: `Notif_PageTitle`، `Notif_Empty` (`Nav_MarkAllRead` أصلاً موجود من قبل).

2. **محتوى الإشعارات نفسه (Title/Message) في الكنترولرز** — كان Hardcoded عربي مباشرة جوه الكود بدل ما يستخدم `_loc.Get(...)`/`_loc.GetFormatted(...)`. ✅ اتصلحت في الأربع كنترولرز:
   - `BookingController.cs` — إشعار الإلغاء + إشعار التأكيد + تنبيه الأدمن عند تأكيد حجز
   - `QuizController.cs` — إشعار كوبون الكويز
   - `UserController.cs` — إشعار الترحيب بعد التسجيل (الكنترولر ده مكانش فيه `LocalizationService` أصلاً، اتضافت في الـ constructor + `Lang()` helper جديد)
   - `ReviewController.cs` — إشعار حل البلاغ (نفس الحالة، اتضافت `LocalizationService` + `Lang()` helper)
   
   ⚠️ **سلوك متعمد ومش باگ:** الإشعار بيتسجل بلغة اليوزر وقت حصول الحدث نفسه، ومبيتحدثش تلقائي لو بدّل اللغة بعد كده (اتأكد بالاختبار الفعلي: إشعار اتعمل إنجليزي فضل إنجليزي حتى بعد تبديل اللغة). الإشعارات القديمة اللي اتسجلت قبل التعديل ده هتفضل عربي زي ما هي (مش هتتحدث رجعيًا) إلا لو اتمسحت يدوي من جدول `Notifications`.
   
   مفاتيح الترجمة الجديدة المطلوب إضافتها في `ar.json`/`en.json`: `Booking_CancelNotifTitle`, `Booking_CancelNotifMessage`, `Booking_ConfirmNotifTitle`, `Booking_ConfirmNotifMessage`, `Booking_AdminNewBookingTitle`, `Booking_AdminNewBookingMessage`, `Quiz_CouponWonNotifTitle`, `Quiz_CouponWonNotifMessage`, `User_WelcomeNotifTitle`, `User_WelcomeNotifMessage`, `Review_ReportResolvedNotifTitle`, `Review_ReportResolvedNotifMessage`.

### 🆕 Triggers جداد اتضافوا (25 يوليو — اتعملوا فعليًا)
بعد ما بعت صاحب المشروع `ShopController.cs`، `AdminController.cs`، `TripPlannerController.cs`، اتضافت 7 Triggers جداد فوق الـ 5 الأساسيين (بند 15):

**`ShopController.cs`** (كان فيه `_loc`/`Lang()` أصلاً، مفيش تعديل في الـ constructor):
- إشعار للمستخدم + تنبيه للأدمن لما أوردر الشوب يتأكد (بعد نجاح الدفع في `Confirm()`)
- إشعار للمستخدم لما يلغي أوردره (`Cancel()`)
- الاتنين بيستخدموا `_loc.Get(...)`/`_loc.GetFormatted(...)` زي باقي الموقع، ومفاتيح الترجمة الجديدة: `Shop_OrderConfirmedNotifTitle/Message`، `Shop_AdminNewOrderTitle/Message`، `Shop_OrderCancelledNotifTitle/Message`

**`TripPlannerController.cs`** (كان فيه `_loc` أصلاً):
- إشعار تأكيد بعد ما اليوزر يعمل خطة رحلة جديدة بنجاح (`Generate()`) — لينك مباشر لـ `/TripPlanner/Result/{id}`
- مفاتيح الترجمة الجديدة: `TripPlanner_CreatedNotifTitle/Message`

**`AdminController.cs`** ⚠️ **بدون نظام ترجمة خالص — قرار سابق إن الأدمن داشبورد مالوش Localization**، فالإشعارات دي اتضافت بنص إنجليزي ثابت (Hardcoded) مباشرة، من غير `_loc`/`Lang()`/`LocalizationService` خالص (الكنترولر رجع لنفس الـ constructor الأصلي بتاعه: `AppDbContext` + `IHttpClientFactory` بس):
- إشعار للمستخدم لما الأدمن يحدّث حالة الشحن يدويًا لـ **Shipped** أو **Delivered** (`UpdateShopOrderShipping()`)
- إشعار للمستخدم لما الأدمن يحوّل حجزه يدويًا لـ **Refunded** (`ChangeBookingStatus()`)
- إشعار للمستخدم لما الأدمن يحوّل أوردر الشوب بتاعه يدويًا لـ **Refunded** (`ChangeShopOrderStatus()`)

مفاتيح الترجمة الجديدة المطلوب إضافتها في `ar.json`/`en.json` (بس بتاعة `ShopController`/`TripPlannerController`، مش الأدمن):

| Key | AR | EN |
|---|---|---|
| `Shop_OrderConfirmedNotifTitle` | تم تأكيد أوردرك 🛍️ | Your order is confirmed 🛍️ |
| `Shop_OrderConfirmedNotifMessage` | أوردرك رقم #{0} ({1} منتج) اتأكد وجاري تجهيزه. | Your order #{0} ({1} item(s)) is confirmed and being prepared. |
| `Shop_AdminNewOrderTitle` | أوردر جديد اتأكد | New order confirmed |
| `Shop_AdminNewOrderMessage` | {0} أكد أوردر رقم #{1} بقيمة {2}. | {0} confirmed order #{1} worth {2}. |
| `Shop_OrderCancelledNotifTitle` | تم إلغاء أوردرك | Your order was cancelled |
| `Shop_OrderCancelledNotifMessage` | طلب إلغاء أوردرك رقم #{0} اتسجل. الفلوس هترجع لحسابك خلال 24 ساعة. | Your cancellation request for order #{0} was recorded. Your money will be refunded within 24 hours. |
| `TripPlanner_CreatedNotifTitle` | خطة رحلتك جاهزة 🗺️ | Your trip plan is ready 🗺️ |
| `TripPlanner_CreatedNotifMessage` | خطة رحلتك لـ {0} يوم اتعملت بنجاح. | Your {0}-day trip plan has been created successfully. |

### ✅ التحديث التلقائي للشحن (26 يوليو — اتعمل فعليًا)
`ShopOrderShippingBackgroundService.cs` اتعدّل — دلوقتي بيبعت نفس الإشعار بالظبط اللي بيبعته المسار اليدوي (`AdminController.UpdateShopOrderShipping`) في الحالتين:
- Processing → Shipped (أوتوماتيك بعد 48 ساعة من `ConfirmedAt`) — جوه `ProcessDueShipmentsAsync`
- Shipped → Delivered (أوتوماتيك حسب عدد أيام المحافظة) — جوه `ProcessDueDeliveriesAsync`

⚠️ نفس القرار بتاع الأدمن بالظبط: نص إنجليزي ثابت (`"Your order has shipped 🚚"` / `"Your order was delivered ✅"`)، مش بيستخدم `_loc`، لأن الـ Background Service ده مالوش وصول لـ `LocalizationService`/Session أصلاً (مفيش Request لنعرف منه لغة اليوزر وقت الحدث) — نفس المنطق اللي خلّى الأدمن داشبورد من غير Localization، بس هنا مفروض تقنيًا مش قرار اختياري.
كل نداء جوه `try/catch` مستقل لكل أوردر — Best effort، فشل الإشعار مبيوقفش تحديث `ShippingStatus` نفسه ولا باقي الأوردرز في نفس الدورة.
النتيجة: سواء الأدمن غيّر الحالة يدويًا أو الـ Background Service غيّرها أوتوماتيك، اليوزر بياخد نفس الإشعار بالظبط — مفيش تعارض ولا تكرار (الفلاتر بتضمن كل أوردر يتلقط مرة واحدة بس).

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
