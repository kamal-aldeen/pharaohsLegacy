using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;
using System.Text;
using System.Text.Json;

namespace pharaohsLegacy.Controllers
{
    public class TripPlannerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly LocalizationService _loc;

        public TripPlannerController(AppDbContext context, IConfiguration config, IHttpClientFactory httpFactory, LocalizationService loc)
        {
            _context = context;
            _config = config;
            _http = httpFactory.CreateClient();
            _loc = loc;
        }

        // GET: /TripPlanner
        [HttpGet]
        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null) return RedirectToAction("Login", "User");

            return View();
        }

        // POST: /TripPlanner/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(TripPlannerGenerateRequest request)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null) return RedirectToAction("Login", "User");

            if (request.Days < 1 || request.Days > 14)
            {
                ModelState.AddModelError("", "عدد الأيام لازم يكون بين 1 و 14.");
                return View("Index");
            }

            if (request.Interests == null || !request.Interests.Any())
            {
                // افتراضي لو اليوزر مختارش حاجة
                request.Interests = new List<string> { "Temple", "Museum" };
            }

            // ---------------------------------------------------------
            // 1) هات الأماكن المرشحة من الداتا بيز بس (زي ما اتفقنا)
            //    محدود بـ 30 لكل نوع عشان الـ prompt ميبقاش ضخم جدًا
            // ---------------------------------------------------------
            var candidates = new List<PlaceCandidate>();

            // 🆕 السعر الحقيقي لكل Temple/Museum من جدول Prices — بنجيبه هنا مرة واحدة
            // ونديه للـ AI عشان يستخدمه بدل ما يخمّن رقم عشوائي (كان بيطلع بعيد جدًا
            // عن السعر الحقيقي، زي هرم خوفو 700 جنيه واللي كان بيرجعه الـ AI 150)
            var realPrices = await _context.Prices
                .Where(p => p.PlaceType == "Temple" || p.PlaceType == "Museum")
                .ToDictionaryAsync(p => (p.PlaceType, p.PlaceId), p => p.Amount);

            if (request.Interests.Contains("Temple"))
            {
                candidates.AddRange(await _context.Temples
                    .Select(t => new PlaceCandidate { PlaceType = "Temple", PlaceId = t.Id, Name = t.Name, Location = t.Location, Info = t.Period })
                    .Take(30)
                    .ToListAsync());
            }

            if (request.Interests.Contains("Museum"))
            {
                candidates.AddRange(await _context.Museums
                    .Select(m => new PlaceCandidate { PlaceType = "Museum", PlaceId = m.Id, Name = m.Name, Location = m.Location, Info = m.Category })
                    .Take(30)
                    .ToListAsync());
            }

            // 🆕 بعد ما جبنا المرشحين، نحط عليهم السعر الحقيقي لو موجود ليهم في Prices
            foreach (var c in candidates)
            {
                if (realPrices.TryGetValue((c.PlaceType, c.PlaceId), out var price))
                    c.TicketPrice = price;
            }

            if (!candidates.Any())
            {
                ModelState.AddModelError("", "مفيش أماكن متاحة للاهتمامات دي حاليًا.");
                return View("Index");
            }

            // ---------------------------------------------------------
            // 2) ابني الـ prompt وكلم Groq (نفس النمط بالظبط بتاع ChatbotController)
            // ---------------------------------------------------------
            var candidatesJson = JsonSerializer.Serialize(candidates.Select(c => new { c.PlaceType, c.PlaceId, c.Name, c.Location, c.Info, c.TicketPrice }));

            var systemPrompt = $@"You are the AI Trip Planner engine of Pharaohs Legacy.
Your job is to build a day-by-day Ancient Egypt itinerary using ONLY the candidate places provided below.

STRICT RULES:
- You MUST only use (placeType, placeId) pairs that appear EXACTLY in the candidate list. NEVER invent an id that is not in the list.
- Respond with RAW JSON ONLY. No markdown code fences, no commentary, no text before or after the JSON.
- Exact JSON schema to follow:
{{
  ""days"": [
    {{
      ""day"": 1,
      ""stops"": [
        {{ ""placeType"": ""Temple"", ""placeId"": 3, ""suggestedTime"": ""09:00 AM"", ""estimatedCost"": 150, ""notes"": ""short helpful note"" }}
      ]
    }}
  ]
}}
- Each candidate below may include a ""TicketPrice"" field — this is the REAL ticket price in EGP. If a candidate has a TicketPrice, you MUST use that exact number as ""estimatedCost"" for that stop — do NOT estimate or round it. Only estimate ""estimatedCost"" yourself for the rare candidate with no TicketPrice.
- Spread stops sensibly across {request.Days} day(s). Try to keep the total cost (using the real TicketPrice values above) close to the budget of {request.Budget} EGP, without exceeding it by much.
- Trip mode is '{request.Mode}': Family = relaxed pace, fewer stops per day, kid-friendly notes. Student = budget-conscious, more stops per day. Luxury = fewer stops per day, premium/unhurried notes.
- Write the ""notes"" field in Arabic if the interests/mode context suggests an Arabic-speaking user, otherwise in English. Default to English if unsure.

Candidate places (choose ONLY from this list):
{candidatesJson}";

            var userPrompt = $"Create a {request.Days}-day Ancient Egypt trip itinerary. Budget: {request.Budget} EGP. Mode: {request.Mode}. Interests: {string.Join(", ", request.Interests)}.";

            var apiKey = _config["GroqApiKey"];
            var url = "https://api.groq.com/openai/v1/chat/completions";

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var body = new
            {
                model = "llama-3.1-8b-instant",
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = Math.Min(300 + request.Days * 250, 4000),
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string aiReplyRaw;
            try
            {
                var response = await _http.PostAsync(url, content);
                var responseStr = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseStr);
                aiReplyRaw = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حصل خطأ في التواصل مع خدمة الـ AI: " + ex.Message);
                return View("Index");
            }

            // ---------------------------------------------------------
            // 3) فك الـ JSON اللي رجع من الـ AI (مع تنظيف احتياطي لو حط ```json``` رغم التعليمات)
            // ---------------------------------------------------------
            var cleanJson = ExtractJson(aiReplyRaw);

            AiItineraryResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<AiItineraryResponse>(cleanJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                ModelState.AddModelError("", "تعذر فهم رد الـ AI، جرب تاني.");
                return View("Index");
            }

            if (parsed?.Days == null || !parsed.Days.Any())
            {
                ModelState.AddModelError("", "الـ AI مرجعش خطة صالحة، جرب تاني.");
                return View("Index");
            }

            // ---------------------------------------------------------
            // 4) تحقق: ارفض أي (placeType, placeId) مش موجود فعليًا في الداتا بيز
            //    (حماية من الـ hallucination) — واستخدم الاسم الحقيقي من الداتا بيز مش اسم الـ AI
            // ---------------------------------------------------------
            var validCandidates = candidates.ToDictionary(c => (c.PlaceType, c.PlaceId), c => c);

            var tripPlan = new TripPlan
            {
                UserEmail = userEmail,
                Days = request.Days,
                Budget = request.Budget,
                Interests = string.Join(",", request.Interests),
                Mode = request.Mode,
                CreatedAt = DateTime.Now
            };

            foreach (var day in parsed.Days)
            {
                foreach (var stop in day.Stops ?? new List<AiStop>())
                {
                    if (!validCandidates.TryGetValue((stop.PlaceType, stop.PlaceId), out var candidate))
                        continue; // تجاهل أي id مخترع من الـ AI

                    tripPlan.Stops.Add(new TripPlanStop
                    {
                        DayNumber = day.Day,
                        PlaceType = stop.PlaceType,
                        PlaceId = stop.PlaceId,
                        PlaceName = candidate.Name,
                        SuggestedTime = stop.SuggestedTime ?? "",
                        EstimatedCost = stop.EstimatedCost,
                        Notes = stop.Notes
                    });
                }
            }

            if (!tripPlan.Stops.Any())
            {
                ModelState.AddModelError("", "الخطة طلعت فاضية بعد التحقق من الأماكن، جرب تاني.");
                return View("Index");
            }

            // ---------------------------------------------------------
            // 4.5) شبكة أمان: حتى لو الـ AI اتجاهل التعليمات وخمّن رقم لوحده،
            //      أي محطة Temple/Museum ليها سعر حقيقي في Prices بنفرضه هنا
            //      بغض النظر عن اللي رجع من الـ AI
            // ---------------------------------------------------------
            foreach (var stop in tripPlan.Stops)
            {
                if (realPrices.TryGetValue((stop.PlaceType, stop.PlaceId), out var realPrice))
                    stop.EstimatedCost = realPrice;
            }

            _context.TripPlans.Add(tripPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { id = tripPlan.Id });
        }

        // GET: /TripPlanner/Result/5
        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null) return RedirectToAction("Login", "User");

            var plan = await _context.TripPlans
                .Include(p => p.Stops)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return NotFound();

            var isAdmin = HttpContext.Session.GetString("UserRole") == "Admin";
            if (plan.UserEmail != userEmail && !isAdmin) return Forbid();

            // ---------------------------------------------------------
            // 🆕 إحداثيات الخريطة — TripPlanStop مفيهوش Lat/Lng، فبنجيبها من
            // Temples/Museums بس (هما الوحيدين اللي عندهم إحداثيات فعليًا).
            // Pharaoh/God مفيهمش إحداثيات فمش هيظهروا على الخريطة (طبيعي).
            // ---------------------------------------------------------
            var templeIds = plan.Stops.Where(s => s.PlaceType == "Temple").Select(s => s.PlaceId).ToList();
            var museumIds = plan.Stops.Where(s => s.PlaceType == "Museum").Select(s => s.PlaceId).ToList();

            var templeCoords = await _context.Temples
                .Where(t => templeIds.Contains(t.Id))
                .Select(t => new { t.Id, t.Latitude, t.Longitude })
                .ToListAsync();

            var museumCoords = await _context.Museums
                .Where(m => museumIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Latitude, m.Longitude })
                .ToListAsync();

            var mapPoints = plan.Stops
                .Select(s =>
                {
                    double? lat = null, lng = null;

                    if (s.PlaceType == "Temple")
                    {
                        var c = templeCoords.FirstOrDefault(t => t.Id == s.PlaceId);
                        if (c != null) { lat = c.Latitude; lng = c.Longitude; }
                    }
                    else if (s.PlaceType == "Museum")
                    {
                        var c = museumCoords.FirstOrDefault(m => m.Id == s.PlaceId);
                        if (c != null) { lat = c.Latitude; lng = c.Longitude; }
                    }

                    return new
                    {
                        day = s.DayNumber,
                        placeType = s.PlaceType,
                        name = s.PlaceName,
                        lat,
                        lng
                    };
                })
                .Where(p => p.lat != null && p.lng != null)
                .ToList();

            ViewBag.MapPointsJson = JsonSerializer.Serialize(mapPoints);

            return View(plan);
        }

        // GET: /TripPlanner/MyTripPlans
        [HttpGet]
        public async Task<IActionResult> MyTripPlans()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null) return RedirectToAction("Login", "User");

            var plans = await _context.TripPlans
                .Include(p => p.Stops)
                .Where(p => p.UserEmail == userEmail)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(plans);
        }

        // POST: /TripPlanner/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null) return RedirectToAction("Login", "User");

            var plan = await _context.TripPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.UserEmail == userEmail);

            if (plan == null) return NotFound();

            _context.TripPlans.Remove(plan); // TripPlanStops بتتحذف تلقائي (FK cascade)
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTripPlans");
        }

        // GET: /TripPlanner/ExportPdf/5?lang=ar|en
        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id, string? lang = null)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null) return RedirectToAction("Login", "User");

            var plan = await _context.TripPlans
                .Include(p => p.Stops)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return NotFound();

            var isAdmin = HttpContext.Session.GetString("UserRole") == "Admin";
            if (plan.UserEmail != userEmail && !isAdmin) return Forbid();

            // 🆕 اليوزر بيختار لغة الـ PDF بنفسه من مودال في Result.cshtml (lang=ar أو lang=en في الـ URL).
            // لو مبعتش قيمة صحيحة (زيارة مباشرة للينك القديم مثلًا)، بنرجع لنفس منطق لغة الموقع القديم كـ fallback
            var validLang = lang == "ar" || lang == "en" ? lang : (HttpContext.Session.GetString("Lang") ?? "ar");

            var pdfBytes = TripPlanPdfBuilder.Build(plan, validLang, _loc);
            var fileName = $"PharaohsLegacy-TripPlan-{plan.Id}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        private static string ExtractJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "{}";
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start >= 0 && end > start) return raw.Substring(start, end - start + 1);
            return raw;
        }
    }

    // ---------------------------------------------------------
    // ViewModels / DTOs — مش Entities، مش هتتحفظ مباشرة في الداتا بيز
    // ---------------------------------------------------------

    public class TripPlannerGenerateRequest
    {
        public int Days { get; set; } = 3;
        public decimal Budget { get; set; }
        public List<string> Interests { get; set; } = new();
        public string Mode { get; set; } = "Student"; // Family / Student / Luxury
    }

    public class PlaceCandidate
    {
        public string PlaceType { get; set; } = "";
        public int PlaceId { get; set; }
        public string Name { get; set; } = "";
        public string? Location { get; set; }
        public string? Info { get; set; }
        public decimal? TicketPrice { get; set; } // 🆕 السعر الحقيقي من جدول Prices لو موجود (Temple/Museum بس)
    }

    public class AiItineraryResponse
    {
        public List<AiDayPlan>? Days { get; set; }
    }

    public class AiDayPlan
    {
        public int Day { get; set; }
        public List<AiStop>? Stops { get; set; }
    }

    public class AiStop
    {
        public string PlaceType { get; set; } = "";
        public int PlaceId { get; set; }
        public string? SuggestedTime { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? Notes { get; set; }
    }
}
