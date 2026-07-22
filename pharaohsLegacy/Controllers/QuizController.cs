using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace pharaohsLegacy.Controllers
{
    // 🆕 الـ JS بيبعت الداتا كـ JSON body خام — الـ Model Binding الافتراضي بيدور على
    // الباراميترز في Form/Query بس، مش جوه JSON إلا لو اتلفوا في class واحد وحطينا [FromBody]
    // عليه. من غيرهم كانت الباراميترز بتوصل null كل مرة (وده سبب "Quiz_AttemptExpired" الوهمي).
    public class QuizStartRequest
    {
        public string Difficulty { get; set; } = "";
    }

    public class QuizAnswerRequest
    {
        public string AttemptId { get; set; } = "";
        public string QuestionId { get; set; } = "";
        public string ChoiceId { get; set; } = "";
    }

    public class QuizController : Controller
    {
        private readonly AppDbContext _db;
        private readonly QuizQuestionGeneratorService _generator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalizationService _loc;

        private const string SessionKey = "QuizAttempt";
        private const int QuestionsPerQuiz = 10;
        private const int PassScorePercent = 70; // لازم ياخد 70% أو أكتر عشان ياخد كوبون
        private const int NetworkGraceSeconds = 2; // هامش بسيط لفرق الشبكة فوق الـ Timeout

        public QuizController(AppDbContext db, QuizQuestionGeneratorService generator,
            IHttpClientFactory httpClientFactory, LocalizationService loc)
        {
            _db = db;
            _generator = generator;
            _httpClientFactory = httpClientFactory;
            _loc = loc;
        }

        private string Lang() => HttpContext.Session.GetString("Lang") ?? "en";

        // GET /Quiz  — صفحة اختيار الصعوبة + منطقة اللعب (AJAX)
        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            return View();
        }

        // POST /Quiz/Start — بيولد الأسئلة ويحفظها في الـ Session، وبيرجع السؤال الأول بس (من غير إجابته)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start([FromBody] QuizStartRequest request)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return Json(new { success = false, message = _loc.Get("Quiz_LoginRequired", lang) });

            if (!Enum.TryParse<QuizDifficulty>(request?.Difficulty, true, out var parsedDifficulty))
                parsedDifficulty = QuizDifficulty.Easy;

            var questions = await _generator.GenerateQuizAsync(parsedDifficulty, QuestionsPerQuiz, lang);
            if (questions.Count < 4)
                return Json(new { success = false, message = _loc.Get("Quiz_NotEnoughData", lang) });

            var attempt = new QuizAttempt
            {
                UserEmail = userEmail,
                Difficulty = parsedDifficulty,
                Questions = questions,
                CurrentIndex = 0,
                TimeLimitSeconds = 20
            };

            attempt.Questions[0].ShownAt = DateTime.Now; // 🔐 بداية العد الحقيقي من هنا مش من التوليد
            SaveAttempt(attempt);

            return Json(new { success = true, question = ToPublicDto(attempt, 0) });
        }

        // POST /Quiz/Answer — التحقق من الإجابة + الـ Timeout بيحصلوا هنا في السيرفر بس
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Answer([FromBody] QuizAnswerRequest request)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return Json(new { success = false, message = _loc.Get("Quiz_LoginRequired", lang) });

            var attempt = LoadAttempt();

            // 🔐 لازم تكون نفس المحاولة بتاعة نفس اليوزر — مينفعش حد يبعت attemptId من حتة تانية
            if (attempt == null || attempt.UserEmail != userEmail || attempt.Id != request?.AttemptId || attempt.Finished)
                return Json(new { success = false, message = _loc.Get("Quiz_AttemptExpired", lang) });

            if (attempt.CurrentIndex >= attempt.Questions.Count)
                return Json(new { success = false, message = _loc.Get("Quiz_AttemptExpired", lang) });

            var current = attempt.Questions[attempt.CurrentIndex];

            // 🔐 لازم يجاوب بالظبط على السؤال الحالي — مينفعش يقفز سؤال أو يعيد إجابة سؤال فات
            if (current.Id != request.QuestionId || current.Answered)
                return Json(new { success = false, message = _loc.Get("Quiz_AttemptExpired", lang) });

            // 🔐 Timeout بمقارنة وقت السيرفر بس — الـ Client مش بيبعت أي وقت أصلاً
            bool timedOut = current.ShownAt == null ||
                (DateTime.Now - current.ShownAt.Value).TotalSeconds > attempt.TimeLimitSeconds + NetworkGraceSeconds;

            var chosen = current.Choices.FirstOrDefault(c => c.Id == request.ChoiceId);
            bool correct = !timedOut && chosen != null && chosen.IsCorrect;

            current.Answered = true;
            current.AnsweredCorrectly = correct;
            attempt.CurrentIndex++;

            int score = attempt.Questions.Count(q => q.AnsweredCorrectly);
            bool finished = attempt.CurrentIndex >= attempt.Questions.Count;

            string? couponCode = null;
            double? couponDiscount = null;

            if (finished)
            {
                attempt.Finished = true;
                int percent = (int)Math.Round(100.0 * score / attempt.Questions.Count);

                if (percent >= PassScorePercent)
                {
                    var client = _httpClientFactory.CreateClient("BankService");
                    var couponResponse = await client.PostAsJsonAsync("coupons/create", new
                    {
                        user_email = userEmail,
                        discount_percent = 20,
                        valid_days = 10,
                        source_type = "Quiz"
                    });

                    if (couponResponse.IsSuccessStatusCode)
                    {
                        var couponResult = await couponResponse.Content.ReadFromJsonAsync<CouponCreateResult>();
                        couponCode = couponResult?.code;
                        couponDiscount = couponResult?.discount_percent;
                    }
                    // لو نداء البنك فشل، الكويز مش بيتوقف — بس اليوزر مياخدش كوبون النهاردة
                }
            }
            else
            {
                attempt.Questions[attempt.CurrentIndex].ShownAt = DateTime.Now; // عداد السؤال الجاي بيبدأ دلوقتي
            }

            SaveAttempt(attempt);

            return Json(new
            {
                success = true,
                correct,
                timedOut,
                finished,
                score,
                total = attempt.Questions.Count,
                nextQuestion = finished ? null : ToPublicDto(attempt, attempt.CurrentIndex),
                couponCode,
                couponDiscount
            });
        }

        // ---------------- Session helpers ----------------

        private void SaveAttempt(QuizAttempt attempt) =>
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(attempt));

        private QuizAttempt? LoadAttempt()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonSerializer.Deserialize<QuizAttempt>(json);
        }

        // 🔐 الميثود الوحيدة اللي بتحول سؤال لحاجة بترجع للمتصفح — IsCorrect مش موجود هنا خالص
        private object ToPublicDto(QuizAttempt attempt, int index)
        {
            var q = attempt.Questions[index];
            return new
            {
                attemptId = attempt.Id,
                questionId = q.Id,
                index,
                total = attempt.Questions.Count,
                text = q.Text,
                choices = q.Choices.Select(c => new { id = c.Id, text = c.Text }),
                timeLimitSeconds = attempt.TimeLimitSeconds
            };
        }
    }
}
