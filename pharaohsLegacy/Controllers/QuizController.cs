using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using pharaohsLegacy.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace pharaohsLegacy.Controllers
{
    // 🆕 Start() بقى مش محتاج أي Body — الصعوبة بقت تلقائية بالكامل جوه الـ
    // Generator (مزيج تصاعدي Easy → Medium → Hard). سايبين الـ class فاضي
    // (بدل ما نمسحه) عشان لو حبينا نضيف حاجة زي "عدد الأسئلة" مستقبلًا نلاقي مكانها جاهز.
    public class QuizStartRequest
    {
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
        private const int StreakScorePercent = 50; // 🆕 حد أقل ومنفصل — عشان يستمر في الـ Streak حتى لو معدّاش حد الكوبون
        private const int NetworkGraceSeconds = 2; // هامش بسيط لفرق الشبكة فوق الـ Timeout
        private const int TimeLimitSeconds = 20; // وقت ثابت لكل سؤال (بغض النظر عن صعوبته)

        // 🆕 خصم الدرجة (Grade) — حسب نسبة الإجابات الصح في نفس الكويز
        private const int GradeDiscountAPlus = 25; // 95-100%
        private const int GradeDiscountA = 20;      // 85-94%
        private const int GradeDiscountBPlus = 15;  // 75-84%
        private const int GradeDiscountB = 10;       // 70-74%

        // 🆕 سقف الخصم الإجمالي الأقصى (Grade + Streak Bonus مع بعض) — حماية تجارية
        private const int MaxTotalDiscountPercent = 35;

        // 🆕 كوبون الكويز صالح لمدة أقصر من كوبون المتجر العادي (10 أيام) — عشان يشجع الاستخدام بسرعة
        private const int QuizCouponValidDays = 10;

        // 🔐 Anti-Cheat: أي إجابة جت أسرع من كده تتحسب غلط تلقائيًا — مفيش إنسان
        // يقدر يقرا سؤال + 4 اختيارات + يدوس في أقل من ثلث ثانية.
        private const double MinHumanAnswerSeconds = 0.35;

        // 🔐 لو متوسط وقت الإجابة أقل من كده مع نتيجة عالية، الكويز يتحط تحت الشك.
        private const double SuspiciousAvgSeconds = 2.5;

        // 🔐 لو التباين بين أوقات الإجابات المختلفة قليل جدًا (كل الإجابات بنفس السرعة تقريبًا)
        // ده نمط بوت/سكريبت مش إنسان (اللي بطبعه بيختلف وقته من سؤال لآخر).
        private const double SuspiciousStdDevSeconds = 0.4;

        public QuizController(AppDbContext db, QuizQuestionGeneratorService generator,
            IHttpClientFactory httpClientFactory, LocalizationService loc)
        {
            _db = db;
            _generator = generator;
            _httpClientFactory = httpClientFactory;
            _loc = loc;
        }

        private string Lang() => HttpContext.Session.GetString("Lang") ?? "en";

        // GET /Quiz — صفحة "ابدأ الكويز" (شاشة واحدة، بلا اختيار صعوبة) + منطقة اللعب (AJAX)
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var lang = Lang();

            // 🆕 آخر سجل (نجح أو فشل) بيحدد هل لعب النهاردة خلاص
            var lastAny = await _db.QuizHistories
                .Where(h => h.UserEmail == userEmail)
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync();

            // 🆕 آخر سجل "مؤهل للاستريك" (50%+) بيحدد الـ Streak الحالي — بس لو لسه "حي" (لعب النهاردة أو إمبارح)
            var lastPassed = await _db.QuizHistories
                .Where(h => h.UserEmail == userEmail && h.StreakEligible)
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync();

            int currentStreak = 0;
            if (lastPassed != null && (DateTime.Now.Date - lastPassed.PlayedAt.Date).Days <= 1)
                currentStreak = lastPassed.StreakDays;

            bool alreadyPlayedToday = lastAny != null && lastAny.PlayedAt.Date == DateTime.Now.Date;

            ViewBag.AlreadyPlayedToday = alreadyPlayedToday;
            ViewBag.AlreadyPlayedMessage = _loc.Get("Quiz_AlreadyPlayedToday", lang);
            ViewBag.StreakMessage = currentStreak > 0
                ? _loc.GetFormatted("Quiz_CurrentStreakActive", lang, currentStreak)
                : _loc.Get("Quiz_CurrentStreakNone", lang);

            return View();
        }

        // POST /Quiz/Start — بيولد كويز بمزيج صعوبة تلقائي ويحفظه في الـ Session، وبيرجع السؤال الأول بس
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start([FromBody] QuizStartRequest? request)
        {
            var lang = Lang();
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return Json(new { success = false, message = _loc.Get("Quiz_LoginRequired", lang) });

            // 🆕 كويز واحد بس مسموح في اليوم لكل يوزر — بيتحقق من آخر سجل مخزّن دايمًا في الداتابيز
            // (مش من الـ Session) عشان اليوزر ميقدرش يلعب تاني بس بمسح الكوكيز أو فتح متصفح جديد.
            var lastHistory = await _db.QuizHistories
                .Where(h => h.UserEmail == userEmail)
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync();

            if (lastHistory != null && lastHistory.PlayedAt.Date == DateTime.Now.Date)
                return Json(new { success = false, message = _loc.Get("Quiz_AlreadyPlayedToday", lang) });

            var questions = await _generator.GenerateQuizAsync(QuestionsPerQuiz, lang);
            if (questions.Count < 4)
                return Json(new { success = false, message = _loc.Get("Quiz_NotEnoughData", lang) });

            var attempt = new QuizAttempt
            {
                UserEmail = userEmail,
                Questions = questions,
                CurrentIndex = 0,
                TimeLimitSeconds = TimeLimitSeconds
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
            double elapsedSeconds = current.ShownAt == null
                ? double.MaxValue
                : (DateTime.Now - current.ShownAt.Value).TotalSeconds;

            bool timedOut = current.ShownAt == null || elapsedSeconds > attempt.TimeLimitSeconds + NetworkGraceSeconds;

            // 🔐 أسرع من حد الإنسان الأدنى = بتتحسب غلط تلقائيًا، حتى لو الاختيار صح فعليًا
            bool tooFastForHuman = elapsedSeconds < MinHumanAnswerSeconds;

            var chosen = current.Choices.FirstOrDefault(c => c.Id == request.ChoiceId);
            bool correct = !timedOut && !tooFastForHuman && chosen != null && chosen.IsCorrect;

            current.Answered = true;
            current.AnsweredCorrectly = correct;
            attempt.CurrentIndex++;

            // بنسجل الوقت الحقيقي (Clamp لأي رقم غريب) عشان نحلل النمط في نهاية الكويز
            attempt.AnswerTimesSeconds.Add(Math.Round(Math.Min(elapsedSeconds, attempt.TimeLimitSeconds + NetworkGraceSeconds), 2));

            int score = attempt.Questions.Count(q => q.AnsweredCorrectly);
            bool finished = attempt.CurrentIndex >= attempt.Questions.Count;

            string? couponCode = null;
            int discountPercent = 0;
            string grade = "Fail";
            int streakDays = 0;

            if (finished)
            {
                attempt.Finished = true;
                int percent = (int)Math.Round(100.0 * score / attempt.Questions.Count);
                bool suspicious = IsSuspiciousTimingPattern(attempt);

                // 🆕 حدّين منفصلين تمامًا: الـ Streak (50%) أسهل من الكوبون (70%) بقصد —
                // الهدف من الـ Streak إنه يشجع الدخول اليومي (عادة)، مش يقيس التفوق.
                bool couponEligible = percent >= PassScorePercent && !suspicious;
                bool streakEligible = percent >= StreakScorePercent && !suspicious;

                grade = GetGrade(percent, couponEligible);

                // 🆕 الـ Streak بيتحسب طالما اليوزر عدّى حد الـ 50%، بغض النظر عن الكوبون خالص
                if (streakEligible)
                    streakDays = await ComputeNewStreakAsync(userEmail);

                if (couponEligible)
                {
                    int gradeDiscount = GetGradeDiscount(grade);
                    int streakBonus = GetStreakBonus(streakDays);
                    // 🆕 الخصم متغير دايمًا (Grade + Streak Bonus)، مع سقف أقصى ثابت لحماية تجارية
                    discountPercent = Math.Min(gradeDiscount + streakBonus, MaxTotalDiscountPercent);

                    var client = _httpClientFactory.CreateClient("BankService");
                    var couponResponse = await client.PostAsJsonAsync("coupons/create", new
                    {
                        user_email = userEmail,
                        discount_percent = discountPercent,
                        valid_days = QuizCouponValidDays,
                        source_type = "Quiz"
                    });

                    if (couponResponse.IsSuccessStatusCode)
                    {
                        var couponResult = await couponResponse.Content.ReadFromJsonAsync<CouponCreateResult>();
                        couponCode = couponResult?.code;
                        if (couponResult?.discount_percent != null)
                            discountPercent = (int)Math.Round(couponResult.discount_percent); // البنك هو مصدر الحقيقة النهائي
                    }
                    else
                    {
                        // 🆕 لو نداء البنك فشل، اليوزر مياخدش كوبون النهاردة، لكن الـ Streak (اللي اتحسب فوق)
                        // مبيتلمسش — فشل البنك مش غلطة اليوزر ومش لازم يتعاقب عليه في الاستمرارية
                        discountPercent = 0;
                    }
                }

                // 🆕 تسجيل النتيجة دايمًا (نجح أو فشل) — أساس فحص "لعب النهاردة؟" والـ Streak القادم
                _db.QuizHistories.Add(new QuizHistory
                {
                    UserEmail = userEmail,
                    PlayedAt = DateTime.Now,
                    Score = score,
                    Total = attempt.Questions.Count,
                    ScorePercent = percent,
                    Grade = grade,
                    StreakEligible = streakEligible,
                    StreakDays = streakDays,
                    DiscountPercent = discountPercent,
                    CouponCode = couponCode
                });
                await _db.SaveChangesAsync();
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
                couponDiscount = finished ? discountPercent : (int?)null,
                grade = finished ? grade : null,
                streakDays = finished ? (int?)streakDays : null
            });
        }

        // ---------------- Grade / Streak helpers ----------------

        // 🆕 الدرجة بتتحدد من نسبة الإجابات الصح في نفس الكويز
        private static string GetGrade(int percent, bool passed)
        {
            if (!passed) return "Fail";
            if (percent >= 95) return "A+";
            if (percent >= 85) return "A";
            if (percent >= 75) return "B+";
            return "B"; // 70-74%، لأن passed = true يعني percent >= PassScorePercent (70) أصلاً
        }

        private static int GetGradeDiscount(string grade) => grade switch
        {
            "A+" => GradeDiscountAPlus,
            "A" => GradeDiscountA,
            "B+" => GradeDiscountBPlus,
            "B" => GradeDiscountB,
            _ => 0
        };

        // 🆕 بونص إضافي حسب عدد الأيام المتتالية اللي اليوزر لعب فيها ونجح (زي Duolingo)
        private static int GetStreakBonus(int streakDays)
        {
            if (streakDays >= 30) return 20;
            if (streakDays >= 14) return 16;
            if (streakDays >= 7) return 12;
            if (streakDays >= 5) return 8;
            if (streakDays >= 3) return 5;
            return 0;
        }

        // 🆕 بيحسب الـ Streak الجديد بعد الكويز الحالي: لو آخر كويز "مؤهل للاستريك" (50%+) كان "إمبارح"
        // بالظبط، الاستمرارية بتكمل (+1). أي فجوة (يوم أو أكتر) أو آخر كويز كان تحت الـ 50% = يرجع لـ 1.
        private async Task<int> ComputeNewStreakAsync(string userEmail)
        {
            var lastEligible = await _db.QuizHistories
                .Where(h => h.UserEmail == userEmail && h.StreakEligible)
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync();

            if (lastEligible != null && lastEligible.PlayedAt.Date == DateTime.Now.Date.AddDays(-1))
                return lastEligible.StreakDays + 1;

            return 1; // النهاردة هو أول يوم في استمرارية جديدة
        }

        // 🔐 بتحلل نمط أوقات الإجابة بتاعة الكويز كله. مش بترجع للـ Client أبدًا —
        // بتتحكم بس في إن الكوبون يتدي ولا لأ، من غير ما اليوزر يعرف إنه اتكشف.
        private bool IsSuspiciousTimingPattern(QuizAttempt attempt)
        {
            var times = attempt.AnswerTimesSeconds;
            if (times.Count < attempt.Questions.Count) return false; // بيانات ناقصة، متحكمش

            double avg = times.Average();
            double variance = times.Sum(t => (t - avg) * (t - avg)) / times.Count;
            double stdDev = Math.Sqrt(variance);

            int total = attempt.Questions.Count;
            int correct = attempt.Questions.Count(q => q.AnsweredCorrectly);
            bool highScore = correct >= total * 0.9;

            bool tooFastOnAverage = avg < SuspiciousAvgSeconds;
            bool tooConsistent = stdDev < SuspiciousStdDevSeconds;

            // نتيجة شبه كاملة + (سرعة غير طبيعية أو ثبات غير طبيعي في التوقيت) = مشبوه
            return highScore && (tooFastOnAverage || tooConsistent);
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
        // 🆕 بنبعت difficulty السؤال ده بس (Easy/Medium/Hard) عشان الـ Badge في الـ UI —
        // ده مجرد Label مالوش أي علاقة بالإجابة الصح فمفيش أي تسريب أمني هنا.
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
                difficulty = q.Difficulty.ToString(),
                choices = q.Choices.Select(c => new { id = c.Id, text = c.Text }),
                timeLimitSeconds = attempt.TimeLimitSeconds
            };
        }
    }
}
