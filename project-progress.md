  # Pharaohs Legacy — Project Progress

## Tech Stack
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core + SQL Server
- Session-based Authentication
- C# / Razor Views / JS / CSS

---

## Database Tables

| Table | Fields |
|---|---|
| Users | Id, Name, Email, Password |
| Pharaohs | Id, Name, Dynasty, Period, Description, ImageUrl |
| Temples | Id, Name, Location, Period, Description, ImageUrl, TicketUrl |
| Museums | Id, Name, Location, Founded, Description, ImageUrl, WebsiteUrl, Category |
| Gods | Id, Name, Role, Symbol, Description, ImageUrl |
| Favorites | Id, UserEmail, Type, ItemId |
| Bookings | Id, UserEmail, PlaceType, PlaceId, PlaceName (NotMapped), VisitDate, NumberOfTickets, TotalPrice, Status, CreatedAt |
| Payments | Id, BookingId, Amount, PaymentDate, PaymentMethod, Status |
| Reviews | Id, UserEmail, UserName, Type, ItemId, Rating (1-5), Comment, CreatedAt, IsEdited |
| ReviewHelpfuls | Id, ReviewId, UserEmail |
| ReviewReports | Id, ReviewId, ReporterEmail, Reason, CreatedAt, IsResolved |
| Dynasties | Id, Name, Era, StartYear, EndYear, Description, Achievements, CapitalCity, ImageUrl, **PharaohTag** |

---

## Models — Important Notes

```csharp
// Booking.cs
[NotMapped]
public string PlaceName { get; set; } = "";

[Column(TypeName = "decimal(18,2)")]
public decimal TotalPrice { get; set; }
```

---

## Controllers

| Controller | Actions |
|---|---|
| UserController | Login (GET/POST), Register (GET/POST), Guest, Logout, Dashboard(string tab = "overview") |
| HomeController | Index (shows 3 pharaohs + 3 temples + 3 museums + 3 gods), Search, Timeline |
| PharaohController | Index, Details (with IsFav + Reviews) |
| TempleController | Index, Details (with IsFav + Book button + Reviews) |
| MuseumController | Index (Egyptian split), Details (with Book + Fav buttons + Reviews) |
| GodController | Index, Details (with IsFav + Reviews) |
| FavoriteController | Index, Add, Remove — يدعم: pharaoh / temple / god / museum |
| BookingController | Create (with PlaceImage), Confirm (POST), MyBookings, Cancel (48hr rule) |
| ReviewController | Add (POST), Delete (POST), DeleteAdmin (POST), Edit (POST), ToggleHelpful (POST), GetHelpfulData (GET), Report (POST), ResolveReport (POST) |
| DynastyController | Index (grouped by Era), Details (with Pharaohs + Artifacts + Prev/Next nav) |
| AdminController | Index, AddPharaoh, EditPharaoh, DeletePharaoh, AddTemple, EditTemple, DeleteTemple, AddMuseum, EditMuseum, DeleteMuseum, AddGod, EditGod, DeleteGod, DeleteUser, ChangeBookingStatus, **AddDynasty, EditDynasty, DeleteDynasty** |

---

## Views Structure

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
│   └── Details.cshtml        ← Info + Pharaohs + Artifacts + Prev/Next nav
├── Favorite/
│   └── Index.cshtml
├── Booking/
│   ├── Create.cshtml
│   └── MyBookings.cshtml
└── Admin/
    └── Index.cshtml
```

---

## Features Done ✅

- Login / Register / Guest access
- Session-based auth
- Form validation (JS + C#)
- Password strength bar + show/hide + confirm
- Egyptian-themed UI (dark gold theme)
- Responsive design + hamburger menu
- Scroll reveal + back to top + stats counter animation
- Broken image fallback
- 20 Pharaohs + 15 Temples + 10 Museums + Gods
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
- Rating + Comments ✅ (مكتمل بالكامل — شوف تفاصيل تحت)
- Dynasties Page ✅ (مكتمل بالكامل — شوف تفاصيل تحت)

---

## Key Rules (مهم جداً)

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

## Admin Email
```
kamalabdlbast89@gmail.com
```

---

## Program.cs Setup

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

## Rating + Comments ✅ (مكتمل بالكامل)

### اللي خلص ✅
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
  - **Edit Review** — inline form بـ star picker + textarea (لصاحب الـ review بس)
  - **Helpful Button** 👍 — toggle + count بيتحدث لحظياً
  - **Report Button** 🚩 — modal بـ 4 أسباب جاهزة + حقل حر (للـ users على reviews حد تاني)
  - بعد Edit بيعمل reload عشان الـ Summary Bar يتحدث
- ضُيّف في Details pages: Pharaoh / Temple / Museum / God / Artifact
- Admin Dashboard:
  - Reviews tab في الـ Sidebar
  - **Reports tab** في الـ Sidebar — بيعرض كل البلاغات مع Reporter / Review / Reason / Date / Status
  - TotalReviews في الـ Overview stats
  - جدول كل الـ Reviews مع Delete بـ confirm modal
  - Filter by Rating + Type + Date في الـ Admin Reviews panel
  - Resolve Report — الأدمن يضغط ✅ Resolve بدون reload
  - Delete Review من الـ Reports tab — بيرجع على reports tab
- القواعد المطبّقة:
  - Guest مش يقدر يكتب/يعدل/يعمل helpful/يعمل report
  - Admin مش يقدر يكتب review — بيشوف "Admins cannot write reviews"
  - كل يوزر يكتب review واحدة بس على كل item
  - كل يوزر يبلّغ مرة واحدة بس على كل review
  - الاسم بيتجيب من الـ DB مش من الـ Session
  - `isGuest = string.IsNullOrEmpty(email) || email == "guest"`
  - Admin يحذف بـ `showDeleteConfirm` modal (نفس style باقي الـ Admin panels)

### لسه هيتعمل 🔜
- لا شيء — مكتمل ✅

---

## Dynasties Page ✅ (مكتمل بالكامل)

### اللي خلص ✅
- Model: `Dynasty.cs` — Id, Name, Era, StartYear, EndYear, Description, Achievements, CapitalCity, ImageUrl, **PharaohTag**
- Migration: `AddDynasties` + `AddDynastyPharaohTag` ✅
- `DynastyController` →
  - `Index` — بيجيب كل الـ dynasties مرتبة بـ StartYear + grouped by Era → `Dictionary<string, List<Dynasty>>`
  - `Details` — بيجيب الـ dynasty + الفراعنة المرتبطين (عن طريق `PharaohTag`) + الآثار + Prev/Next dynasty
- Views:
  - `Index.cshtml` — Grid Cards grouped by Era + Filter buttons + Mini Timeline proportional + Era Legend
  - `Details.cshtml` — Hero + Key Facts + Achievements + Pharaohs grid + Artifacts grid + Prev/Next nav
- Static files:
  - `wwwroot/css/dynasty.css` — Egyptian dark gold theme
  - `wwwroot/js/dynasty.js` — Filter by Era + Scroll reveal
- Admin Dashboard:
  - Dynasties tab في الـ Sidebar — `class="adm-nav-item"` + `onclick="switchPanel('dynasties',this)"`
  - `panel-dynasties` — جدول كامل مع Image + Name + Era + Period + Capital + Actions
  - `modalAddDynasty` — Add modal بنفس structure باقي الـ modals
  - `modalEditDynasty` — Edit modal + `openEditDynastyBtn(btn)` JS function
  - TotalDynasties في الـ AdminOverviewViewModel + Overview stats
- Data: 14 dynasty في الـ DB (SQL script) تغطي كل الحقب من Early Dynastic لـ Ptolemaic
- Navbar: `<li><a asp-controller="Dynasty" asp-action="Index">𓂀 Dynasties</a></li>`

### Key Rules — Dynasties
- `PharaohTag` لازم يطابق بالظبط الـ `Dynasty` field في جدول Pharaohs
  - مثال: Dynasty Name = `"Eighteenth Dynasty"` → PharaohTag = `"18th Dynasty"`
- الفراعنة والآثار بيظهروا تلقائي لما تضيفهم في الـ DB — مفيش حاجة تعملها
- StartYear و EndYear: سالب = BC (مثال: `-3100` = 3100 BC)
- الـ Mini Timeline بيتحسب proportionally بناءً على السنين — بيتحدث تلقائي

---

## 🚀 Roadmap — المشروع الكامل

### ✅ المرحلة 1 — Core (منتهية)
كل الـ features الموجودة فوق

---

### 🔜 المرحلة 2 — محتوى أكتر
- [x] **Artifacts** — آثار ومقتنيات (Model + Controller + Views + CRUD)
- [x] **Dynasties** — الأسرات الحاكمة مع Timeline ✅
- [ ] **Historical Events** — أهم الأحداث التاريخية
- [ ] **Quiz** — اختبار معلومات تفاعلي عن الحضارة

---

### 🔜 المرحلة 3 — Analytics & Reports
- [ ] **Revenue Reports** — إيرادات شهرية/سنوية
- [ ] **Most Visited** — أكتر المواقع زيارة وحجزاً
- [ ] **User Statistics** — إحصائيات المستخدمين
- [ ] **Export PDF** — تقارير قابلة للتنزيل
- [ ] **Heatmap** — أكتر الصفحات زيارة

---

### 🔜 المرحلة 4 — تفاعل المستخدم
- [x] **Rating + Comments** ✅ مكتمل بالكامل
- [ ] **Photo Gallery** — يرفع صوره من الزيارة
- [ ] **Achievements & Badges** — شارات للي زار أماكن معينة
- [ ] **Share** على السوشيال ميديا
- [ ] **Forum** — منتدى للنقاش

---

### 🔜 المرحلة 5 — AI Features
- [x] **AI Tour Guide Chatbot** — بيجاوب على أسئلة عن الحضارة المصرية
- [ ] **AI Recommendations** — يقترح أماكن على حسب اهتمامك
- [ ] **Smart Search** — تدور بالوصف مش بالاسم
- [ ] **AI Image Recognition** — ترفع صورة أثر وهو يعرفه

---

### 🔜 المرحلة 6 — Interactive Experience
- [ ] **Interactive Map** — خريطة مصر التفاعلية بكل المواقع
- [ ] **Virtual Tour 360°** — جولة افتراضية جوه المعابد
- [ ] **3D Pyramids** — نموذج ثلاثي الأبعاد
- [x] **Hieroglyphics Translator** — تكتب اسمك بالهيروغليفية

---

### 🔜 المرحلة 7 — Business Features
- [ ] **Subscription Plans** — Basic / Premium / VIP
- [ ] **Gift Cards** — هدايا للزيارة
- [ ] **Group Booking** — حجز جماعي
- [ ] **Tour Packages** — باقات سياحية متكاملة
- [ ] **QR Code** للتذكرة
- [ ] **Email Confirmation** — تأكيد الحجز بالإيميل

---

### 🔜 المرحلة 8 — Advanced Features
- [ ] **Multi-language** — عربي وإنجليزي
- [ ] **Dark/Light Mode**
- [ ] **PWA** — يشتغل زي App على الموبايل
- [ ] **Push Notifications**
- [ ] **Offline Mode**

---

### 🔜 المرحلة 9 — Security & Performance
- [ ] **Two Factor Authentication**
- [ ] **OAuth** — Login بـ Google/Facebook
- [ ] **Rate Limiting**
- [ ] **Audit Logs**
- [ ] **Image Upload** — رفع صور حقيقية

---

### 🔜 المرحلة 10 — Hosting
- [ ] **Azure** أو **Railway**
- [ ] **Domain** حقيقي
- [ ] **SSL Certificate**
- [ ] **CI/CD Pipeline**

---

## Priority Order (ابدأ بالترتيب ده)

### 🟢 المجموعة الأولى — Quick Wins (محتوى + تفاعل بسيط)
```
1.  ✅ Interactive Map
2.  ✅ My Journey Tab + Visited Status
3.  ✅ Hieroglyphics Translator
4.  ✅ AI Tour Guide Chatbot
5.  ✅ Artifacts
6.  ✅ Rating + Comments (مكتمل بالكامل)
7.  ✅ Dynasties Page (مكتمل بالكامل)
8.  [ ] Historical Events
9.  [ ] Quiz تفاعلي 
10. [ ] Dark / Light Mode
11. [ ] Daily Fact — حقيقة يومية عن الحضارة في الـ Home Page
```

### 🟡 المجموعة التانية — Business Logic
```
12. [ ] Email Confirmation + QR Code للتذكرة
13. [ ] Group Booking
14. [ ] Image Upload (رفع صور حقيقية)
15. [ ] Photo Gallery
16. [ ] Share على السوشيال ميديا
17. [ ] Loyalty Points — نقاط على كل حجز قابلة للتحويل لـ discount
18. [ ] Waitlist — تسجيل انتظار لو المكان محجوز بالكامل
```

### 🔵 المجموعة التالتة — Analytics & Admin
```
19. [ ] Revenue Reports
20. [ ] Most Visited Stats
21. [ ] User Statistics
22. [ ] Export PDF
23. [ ] Heatmap
```

### 🟣 المجموعة الرابعة — AI + Smart Features
```
24. [ ] AI Trip Planner — "عندي 3 أيام" → برنامج رحلة كامل من الـ DB
25. [ ] AI Recommendations
26. [ ] Smart Search
27. [ ] AI Image Recognition
```

### 🔴 المجموعة الخامسة — Gamification & Social
```
28. [ ] Achievements & Badges
29. [ ] Leaderboard — أكتر المستخدمين زيارة للأماكن
30. [ ] Forum
31. [ ] Tour Packages + Gift Cards
32. [ ] Subscription Plans
```

### ⚫ المجموعة السادسة — Security & Auth
```
33. [ ] OAuth (Google/Facebook)
34. [ ] Two Factor Authentication
35. [ ] Rate Limiting + Audit Logs
```

### ⚪ المجموعة السابعة — Advanced & Hosting
```
36. [ ] Multi-language (عربي/إنجليزي)
37. [ ] PWA + Push Notifications
38. [ ] Virtual Tour 360° + 3D Pyramids
39. [ ] Offline Mode
40. [ ] Public API — endpoints للـ Pharaohs / Gods / Temples للمطورين
41. [ ] Hosting (Azure/Railway + Domain + SSL + CI/CD)
```

---

## My Journey Tab ✅
- Tab جديد في User Dashboard
- Map بيعرض الـ temples والـ museums (Booked / Favourite / Both / Visited)
- Pins بألوان مختلفة (Gold / Red / Purple / Green)
- Cards تحت الـ Map بتفاصيل كل مكان
- Empty state لو مفيش حاجة

## Visited Status ✅
- `BookingStatusUpdater` — Background Service كل ساعة يغير Confirmed لـ Visited أوتوماتيك
- فلتر Visited في User Dashboard Bookings tab
- فلتر Visited في Admin Dashboard Bookings
- Places Visited counter في الـ Overview stats (5 stats دلوقتي)
- Explorer Badge في الـ Profile (Explorer / Temple Master / Grand Explorer)
- Visited row في Account Details
- Visited pin أخضر على الـ Journey Map

## Maps & Location ✅ (مكتمل بالكامل)
- Interactive Map page (Temples + Museums) — Leaflet.js dark theme
- Filter buttons (All / Temples / Museums)
- Popup cards بصورة + وصف + View Details button
- Map Picker في Admin Dashboard:
  - Add Temple → اضغط على الخريطة يتملي Lat/Lng أوتوماتيك ✅
  - Edit Temple → بيفتح على الموقع الحالي + Lat/Lng متملية ✅
  - Add Museum → اضغط على الخريطة يتملي Lat/Lng أوتوماتيك ✅
  - Edit Museum → بيفتح على الموقع الحالي + Lat/Lng متملية ✅

## Hieroglyphics Translator ✅
- Unicode Egyptian Hieroglyphs — Noto Sans Egyptian Hieroglyphs font
- Input مع character counter (20 حرف max)
- Output بـ animation لكل رمز
- Download — Canvas API مباشرة (مش html2canvas) عشان الـ font يتحمّل صح
- Copy Text للـ clipboard
- Alphabet Reference تفاعلي — اضغط على أي حرف يتضاف للـ input
- Toast notifications
- Controller: `HieroglyphicsController` → `Translator()`

## AI Tour Guide Chatbot ✅
- Floating widget في كل الصفحات عبر `_Layout.cshtml`
- Powered by **Groq API** + **LLaMA 3.1 8B Instant** (مجاني)
- System Prompt قوي — بيلعب دور AI Tour Guide متخصص في الحضارة المصرية
- مش بيكشف إنه Groq/LLaMA — شخصية الموقع بس
- Suggestion chips تختفي بعد أول رسالة
- Typing animation (3 dots)
- Auto-resize textarea
- Expand button لتكبير الـ window
- AI GUIDE label فوق الـ button
- Pulse animation على الـ floating button
- Responsive على الموبايل
- Controller: `ChatbotController` → `Ask()` (POST)
- Key Rules:
  - `builder.Services.AddHttpClient()` في `Program.cs`
  - API Key في `appsettings.json` تحت `"GroqApiKey"`
  - Model: `llama-3.1-8b-instant`

## Timeline ✅ (محدّثة)
- Dynasty grouping — header لكل أسرة
- Filter buttons — All + كل dynasty
- Controller: `HomeController` → `Timeline()` يرجع `Dictionary<string, List<Pharaoh>>`

## Artifacts ✅
- Model: Id, Name, Origin, Period, Category, Description, ImageUrl, Museum, CurrentLocation
- Migration: `AddArtifacts` ✅
- Controller: `ArtifactController` → Index + Details
- Views: Index (filter by category) + Details (meta grid + favorites)
- Admin CRUD: AddArtifact / EditArtifact / DeleteArtifact
- AdminOverviewViewModel: TotalArtifacts + Artifacts
- Navbar: رابط Artifacts مضاف
- Data: 15 artifact في الـ DB (SQL script)
- Favorites: يدعم type = "artifact"
- Key: الـ modals بتستخدم `adm-overlay` مش `adm-modal` كـ wrapper

## Dynasties Page ✅ (مكتمل بالكامل)
- Model: Id, Name, Era, StartYear, EndYear, Description, Achievements, CapitalCity, ImageUrl, **PharaohTag**
- Migration: `AddDynasties` + `AddDynastyPharaohTag` ✅
- Controller: `DynastyController` → Index + Details
- Views: Index (Grid + Era Filter + Mini Timeline) + Details (Info + Pharaohs + Artifacts + Prev/Next)
- Static: `dynasty.css` + `dynasty.js`
- Admin CRUD: AddDynasty / EditDynasty / DeleteDynasty + `openEditDynastyBtn(btn)`
- AdminOverviewViewModel: TotalDynasties + Dynasties
- Navbar: `<li><a asp-controller="Dynasty" asp-action="Index">𓂀 Dynasties</a></li>`
- Data: 14 dynasty في الـ DB تغطي Early Dynastic → Ptolemaic
- **PharaohTag** = exact match للـ `Dynasty` field في جدول Pharaohs (مثال: `"18th Dynasty"`)
- الفراعنة والآثار بيظهروا تلقائي — مفيش حاجة تعملها لما تضيف بيانات جديدة

## Rating + Comments ✅ (مكتمل بالكامل)
- **Verified Visitor Badge** ✅ — يظهر "✅ Visited" جنب اسم اليوزر لو حجز المكان
- **Edit Review** ✅ — inline form بـ star picker + textarea + reload بعد الحفظ
- **Filter by Rating** ✅ — أزرار All / ★★★★★ / ...
- **Helpful Button** ✅ — toggle voted/unvoted + count بـ AJAX
- **Report Review** ✅ — modal بـ 4 أسباب + حقل حر + Admin Reports Tab
- Migration: `AddReviewExtensions` — أضاف IsEdited + ReviewHelpfuls + ReviewReports tables
