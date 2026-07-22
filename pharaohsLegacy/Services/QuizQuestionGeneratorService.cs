using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    // ============================================================================
    // مفيش جدول Questions ولا نص أسئلة Hardcoded — كل سؤال بيتولد من صف حقيقي
    // في الداتابيز وقت الطلب (Runtime)، ومعاه Distractors من نفس الجدول.
    // أي إضافة جديدة في Pharaohs/Gods/Temples/... بتدخل تلقائيًا هنا من غير
    // أي تعديل في الكود ده — ده اللي بيخلي الكويز "Dynamic".
    // ============================================================================
    public class QuizQuestionGeneratorService
    {
        private readonly AppDbContext _db;
        private readonly Random _rng = new();

        public QuizQuestionGeneratorService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<QuizQuestion>> GenerateQuizAsync(QuizDifficulty difficulty, int count, string lang = "en")
        {
            var questions = new List<QuizQuestion>();

            // بنسحب Generator عشوائي كل مرة عشان النوع يتنوع (مش كل الأسئلة عن نفس الجدول)
            var generators = new List<Func<string, Task<QuizQuestion?>>>
            {
                l => GeneratePharaohDynastyQuestion(l),
                l => GenerateGodSymbolQuestion(l),
                l => GenerateTempleLocationQuestion(l),
                l => GenerateMuseumLocationQuestion(l),
                l => GenerateDynastyEraQuestion(l),
                l => GenerateHistoricalEventYearQuestion(l, difficulty),
            };

            int safetyLimit = count * 8; // عشان لو جدول فاضي أو مفيهوش داتا كفاية مش هيلف للأبد
            int attempts = 0;

            while (questions.Count < count && attempts < safetyLimit)
            {
                attempts++;
                var gen = generators[_rng.Next(generators.Count)];
                var q = await gen(lang);
                if (q == null) continue;

                q.Difficulty = difficulty;
                if (questions.Any(x => x.Text == q.Text)) continue; // منع تكرار نفس السؤال جوه المحاولة

                questions.Add(q);
            }

            return questions;
        }

        private static string Pick(string en, string? ar, string lang) =>
            lang == "ar" && !string.IsNullOrWhiteSpace(ar) ? ar! : en;

        private async Task<QuizQuestion?> GeneratePharaohDynastyQuestion(string lang)
        {
            var pharaohs = await _db.Pharaohs.ToListAsync();
            if (pharaohs.Count < 4) return null;

            var correct = pharaohs[_rng.Next(pharaohs.Count)];
            var correctDynasty = Pick(correct.Dynasty, correct.DynastyAr, lang);

            var pool = pharaohs.Where(p => p.Id != correct.Id).ToList();
            var wrongDynasties = pool
                .Select(p => Pick(p.Dynasty, p.DynastyAr, lang))
                .Where(d => d != correctDynasty)
                .Distinct()
                .OrderBy(_ => _rng.Next())
                .Take(3)
                .ToList();

            if (wrongDynasties.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = lang == "ar" ? $"الفرعون \"{name}\" حكم في أي أسرة؟" : $"Which dynasty did \"{name}\" belong to?";

            return new QuizQuestion { Category = "Pharaoh", Text = text, Choices = BuildChoices(correctDynasty, wrongDynasties) };
        }

        private async Task<QuizQuestion?> GenerateGodSymbolQuestion(string lang)
        {
            var gods = await _db.Gods.Where(g => !string.IsNullOrWhiteSpace(g.Symbol)).ToListAsync();
            if (gods.Count < 4) return null;

            var correct = gods[_rng.Next(gods.Count)];
            var distractors = gods.Where(g => g.Id != correct.Id).OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = lang == "ar" ? $"إيه رمز الإله \"{name}\"؟" : $"What is the symbol of the god \"{name}\"?";

            var choices = BuildChoices(
                Pick(correct.Symbol, correct.SymbolAr, lang),
                distractors.Select(g => Pick(g.Symbol, g.SymbolAr, lang)));

            return new QuizQuestion { Category = "God", Text = text, Choices = choices };
        }

        private async Task<QuizQuestion?> GenerateTempleLocationQuestion(string lang)
        {
            var temples = await _db.Temples.ToListAsync();
            if (temples.Count < 4) return null;

            var correct = temples[_rng.Next(temples.Count)];
            var distractors = temples.Where(t => t.Id != correct.Id && t.Location != correct.Location)
                .OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = lang == "ar" ? $"معبد \"{name}\" موجود فين؟" : $"Where is \"{name}\" located?";

            var choices = BuildChoices(
                Pick(correct.Location, correct.LocationAr, lang),
                distractors.Select(t => Pick(t.Location, t.LocationAr, lang)));

            return new QuizQuestion { Category = "Temple", Text = text, Choices = choices };
        }

        private async Task<QuizQuestion?> GenerateMuseumLocationQuestion(string lang)
        {
            var museums = await _db.Museums.ToListAsync();
            if (museums.Count < 4) return null;

            var correct = museums[_rng.Next(museums.Count)];
            var distractors = museums.Where(m => m.Id != correct.Id && m.Location != correct.Location)
                .OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = lang == "ar" ? $"متحف \"{name}\" موجود فين؟" : $"Where is \"{name}\" located?";

            var choices = BuildChoices(
                Pick(correct.Location, correct.LocationAr, lang),
                distractors.Select(m => Pick(m.Location, m.LocationAr, lang)));

            return new QuizQuestion { Category = "Museum", Text = text, Choices = choices };
        }

        private async Task<QuizQuestion?> GenerateDynastyEraQuestion(string lang)
        {
            var dynasties = await _db.Dynasties.ToListAsync();
            if (dynasties.Count < 4) return null;

            var correct = dynasties[_rng.Next(dynasties.Count)];
            var distractors = dynasties.Where(d => d.Id != correct.Id && d.Era != correct.Era)
                .OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = lang == "ar" ? $"\"{name}\" كانت جزء من أنهي عصر؟" : $"\"{name}\" belonged to which era?";

            var choices = BuildChoices(
                Pick(correct.Era, correct.EraAr, lang),
                distractors.Select(d => Pick(d.Era, d.EraAr, lang)));

            return new QuizQuestion { Category = "Dynasty", Text = text, Choices = choices };
        }

        private async Task<QuizQuestion?> GenerateHistoricalEventYearQuestion(string lang, QuizDifficulty difficulty)
        {
            var events = await _db.HistoricalEvents.ToListAsync();
            if (events.Count < 1) return null;

            var correct = events[_rng.Next(events.Count)];
            var title = Pick(correct.Title, correct.TitleAr, lang);
            var text = lang == "ar" ? $"\"{title}\" حصل في أنهي سنة تقريبًا؟" : $"Around which year did \"{title}\" happen?";

            // فرق السنين بين الاختيارات بيصغر مع زيادة الصعوبة (يعني الاختيارات القريبة أصعب تخمين)
            int gap = difficulty switch
            {
                QuizDifficulty.Easy => 200,
                QuizDifficulty.Medium => 80,
                _ => 30
            };

            var wrongYears = new HashSet<int>();
            int safety = 0;
            while (wrongYears.Count < 3 && safety < 20)
            {
                safety++;
                int offset = (_rng.Next(2) == 0 ? 1 : -1) * (gap + _rng.Next(gap));
                int candidate = correct.Year + offset;
                if (candidate != correct.Year) wrongYears.Add(candidate);
            }
            if (wrongYears.Count < 3) return null;

            static string YearLabel(int y) => y < 0 ? $"{Math.Abs(y)} BC" : $"{y} AD";

            var choices = BuildChoices(YearLabel(correct.Year), wrongYears.Select(YearLabel));

            return new QuizQuestion { Category = "HistoricalEvent", Text = text, Choices = choices };
        }

        private List<QuizChoice> BuildChoices(string correctText, IEnumerable<string> wrongTexts)
        {
            var choices = new List<QuizChoice> { new QuizChoice { Text = correctText, IsCorrect = true } };
            foreach (var w in wrongTexts.Take(3))
                choices.Add(new QuizChoice { Text = w, IsCorrect = false });

            return choices.OrderBy(_ => _rng.Next()).ToList(); // الإجابة الصح مش دايمًا في نفس المكان
        }
    }
}
