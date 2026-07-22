using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    // ============================================================================
    // مفيش جدول Questions ولا نص أسئلة Hardcoded — كل سؤال بيتولد من صف حقيقي
    // في الداتابيز وقت الطلب (Runtime)، ومعاه Distractors من نفس الجدول.
    //
    // 🆕 الصعوبة بقت تلقائية بالكامل — اليوزر مش بيختارها. كل كويز فيه مزيج
    // تصاعدي (الأسئلة الأولانية أسهل، وبتصعب تدريجيًا) بدل ما يبقى الكويز كله
    // بنفس المستوى. ده بيتحكم فيه BuildDifficultyPlan تحت.
    //
    // 🆕 بقى فيه 16 شكل سؤال مختلف (مش 6) — Forward + Reverse لكل جدول،
    // Odd-One-Out، True/False، وأسئلة Cross-Reference بين جدولين — وكل نوع
    // ليه أكتر من صياغة بتتسحب عشوائي، عشان مايبقاش نفس الجملة وبس اتغير فيها اسم.
    // ============================================================================
    public class QuizQuestionGeneratorService
    {
        private readonly AppDbContext _db;
        private readonly Random _rng = new();

        // ترتيب الصعوبة رقميًا (مش بنعتمد على قيمة الـ enum الأصلية عشان نتجنب أي افتراض غلط)
        private static readonly Dictionary<QuizDifficulty, int> Rank = new()
        {
            { QuizDifficulty.Easy, 0 },
            { QuizDifficulty.Medium, 1 },
            { QuizDifficulty.Hard, 2 },
        };

        public QuizQuestionGeneratorService(AppDbContext db)
        {
            _db = db;
        }

        // -------------------------------------------------------------------
        // نقطة الدخول: بتبني خطة صعوبة تصاعدية للكويز كله، وبعدين تولد سؤال
        // لكل خانة في الخطة بالصعوبة المحددة ليها.
        // -------------------------------------------------------------------
        public async Task<List<QuizQuestion>> GenerateQuizAsync(int count, string lang = "en")
        {
            var plan = BuildDifficultyPlan(count);
            var questions = new List<QuizQuestion>();
            var descriptors = BuildGeneratorDescriptors();

            foreach (var slotDifficulty in plan)
            {
                var eligible = descriptors.Where(d => Rank[d.MinDifficulty] <= Rank[slotDifficulty]).ToList();

                QuizQuestion? q = null;
                int attempts = 0;
                while (q == null && attempts < eligible.Count * 3)
                {
                    attempts++;
                    var desc = eligible[_rng.Next(eligible.Count)];
                    var candidate = await desc.Generate(lang, slotDifficulty);
                    if (candidate == null) continue;
                    if (questions.Any(x => x.Text == candidate.Text)) continue; // منع تكرار نفس السؤال بالظبط

                    candidate.Difficulty = slotDifficulty;
                    q = candidate;
                }

                if (q != null) questions.Add(q);
            }

            return questions;
        }

        // 40% سهل، 40% متوسط، 20% صعب — بالترتيب ده بالظبط (تصاعدي)، مش Shuffle،
        // عشان الإحساس يبقى "بيصعب مع تقدمي" زي كويزات التلفزيون الحقيقية.
        private static List<QuizDifficulty> BuildDifficultyPlan(int count)
        {
            int hard = Math.Max(1, (int)Math.Floor(count * 0.2));
            int easy = Math.Max(1, (int)Math.Ceiling(count * 0.4));
            int medium = Math.Max(0, count - easy - hard);

            var plan = new List<QuizDifficulty>();
            plan.AddRange(Enumerable.Repeat(QuizDifficulty.Easy, easy));
            plan.AddRange(Enumerable.Repeat(QuizDifficulty.Medium, medium));
            plan.AddRange(Enumerable.Repeat(QuizDifficulty.Hard, hard));

            // لو فيه فرق تقريب بسيط خليه في الآخر (الأصعب)
            while (plan.Count < count) plan.Add(QuizDifficulty.Hard);
            while (plan.Count > count) plan.RemoveAt(plan.Count - 1);

            return plan;
        }

        private record GeneratorDescriptor(
            Func<string, QuizDifficulty, Task<QuizQuestion?>> Generate,
            QuizDifficulty MinDifficulty);

        private List<GeneratorDescriptor> BuildGeneratorDescriptors() => new()
        {
            // ---- أساسيات (متاحة من Easy) ----
            new(GeneratePharaohDynastyQuestion, QuizDifficulty.Easy),
            new(GeneratePharaohByDynastyQuestion, QuizDifficulty.Easy),
            new(GenerateGodSymbolQuestion, QuizDifficulty.Easy),
            new(GenerateTempleLocationQuestion, QuizDifficulty.Easy),
            new(GenerateMuseumLocationQuestion, QuizDifficulty.Easy),
            new(GenerateDynastyEraQuestion, QuizDifficulty.Easy),
            new(GenerateHistoricalEventYearQuestion, QuizDifficulty.Easy),
            new(GenerateArtifactMuseumQuestion, QuizDifficulty.Easy),
            new(GenerateTrueFalsePharaohDynastyQuestion, QuizDifficulty.Easy),

            // ---- أعقد شوية (Medium فما فوق): Reverse-direction + Odd-One-Out ----
            new(GenerateGodByReverseSymbolQuestion, QuizDifficulty.Medium),
            new(GenerateTempleByLocationQuestion, QuizDifficulty.Medium),
            new(GenerateMuseumByLocationQuestion, QuizDifficulty.Medium),
            new(GenerateDynastyByEraQuestion, QuizDifficulty.Medium),
            new(GenerateOddOneOutDynastyQuestion, QuizDifficulty.Medium),

            // ---- الأصعب (Hard بس): Cross-Reference بين جدولين ----
            new(GenerateEventDynastyLinkQuestion, QuizDifficulty.Hard),
            new(GenerateEventPharaohLinkQuestion, QuizDifficulty.Hard),
        };

        // =====================================================================
        // Helpers عامة
        // =====================================================================

        private static string Pick(string en, string? ar, string lang) =>
            lang == "ar" && !string.IsNullOrWhiteSpace(ar) ? ar! : en;

        private static string Fmt(string[] enTemplates, string[] arTemplates, string lang, Random rng, params object[] args)
        {
            int idx = rng.Next(enTemplates.Length);
            var template = lang == "ar" ? arTemplates[Math.Min(idx, arTemplates.Length - 1)] : enTemplates[idx];
            return string.Format(template, args);
        }

        // بيحاول ياخد الرقم اللي في أول النص (مثال: "18th Dynasty" → 18) عشان نقدر
        // نقيس "قرب" الاختيارات الغلط من الصح رقميًا وقت الصعوبة العالية.
        private static int? ExtractLeadingNumber(string text)
        {
            var m = Regex.Match(text, @"\d+");
            return m.Success ? int.Parse(m.Value) : (int?)null;
        }

        // في Hard: الاختيارات الغلط أقرب رقميًا للصح (أصعب تفرقة). في غير كده: عشوائي.
        private List<string> OrderDistractorsByDifficulty(string correctText, List<string> candidates, QuizDifficulty difficulty)
        {
            if (difficulty != QuizDifficulty.Hard) return candidates.OrderBy(_ => _rng.Next()).ToList();

            var correctNum = ExtractLeadingNumber(correctText);
            if (correctNum == null) return candidates.OrderBy(_ => _rng.Next()).ToList();

            return candidates.OrderBy(c => Math.Abs((ExtractLeadingNumber(c) ?? int.MaxValue / 2) - correctNum.Value)).ToList();
        }

        // نفس الفكرة لكن بالاعتماد على فرق سنين (لـ Dynasties اللي معاها StartYear)
        private List<T> OrderByYearProximity<T>(T correct, List<T> candidates, QuizDifficulty difficulty, Func<T, int> yearSelector)
        {
            if (difficulty != QuizDifficulty.Hard) return candidates.OrderBy(_ => _rng.Next()).ToList();
            int correctYear = yearSelector(correct);
            return candidates.OrderBy(c => Math.Abs(yearSelector(c) - correctYear)).ToList();
        }

        private List<QuizChoice> BuildChoices(string correctText, IEnumerable<string> wrongTexts)
        {
            var choices = new List<QuizChoice> { new QuizChoice { Text = correctText, IsCorrect = true } };
            foreach (var w in wrongTexts.Take(3))
                choices.Add(new QuizChoice { Text = w, IsCorrect = false });

            return choices.OrderBy(_ => _rng.Next()).ToList();
        }

        private List<QuizChoice> BuildBinaryChoices(string trueLabel, string falseLabel, bool statementIsTrue)
        {
            var choices = new List<QuizChoice>
            {
                new QuizChoice { Text = trueLabel, IsCorrect = statementIsTrue },
                new QuizChoice { Text = falseLabel, IsCorrect = !statementIsTrue },
            };
            return choices.OrderBy(_ => _rng.Next()).ToList();
        }

        // =====================================================================
        // 1) فرعون → أسرة (Forward)
        // =====================================================================
        private async Task<QuizQuestion?> GeneratePharaohDynastyQuestion(string lang, QuizDifficulty difficulty)
        {
            var pharaohs = await _db.Pharaohs.ToListAsync();
            if (pharaohs.Count < 4) return null;

            var correct = pharaohs[_rng.Next(pharaohs.Count)];
            var correctDynasty = Pick(correct.Dynasty, correct.DynastyAr, lang);

            var pool = pharaohs.Where(p => p.Id != correct.Id).ToList();
            var wrongPool = pool
                .Select(p => Pick(p.Dynasty, p.DynastyAr, lang))
                .Where(d => d != correctDynasty)
                .Distinct()
                .ToList();

            var wrongDynasties = OrderDistractorsByDifficulty(correctDynasty, wrongPool, difficulty).Take(3).ToList();
            if (wrongDynasties.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = Fmt(
                new[] { "Which dynasty did \"{0}\" belong to?", "\"{0}\" was a pharaoh of which dynasty?" },
                new[] { "الفرعون \"{0}\" حكم في أي أسرة؟", "\"{0}\" كان فرعون من أنهي أسرة؟" },
                lang, _rng, name);

            return new QuizQuestion { Category = "Pharaoh", Text = text, Choices = BuildChoices(correctDynasty, wrongDynasties) };
        }

        // =====================================================================
        // 2) أسرة → فرعون (Reverse)
        // =====================================================================
        private async Task<QuizQuestion?> GeneratePharaohByDynastyQuestion(string lang, QuizDifficulty difficulty)
        {
            var pharaohs = await _db.Pharaohs.ToListAsync();
            if (pharaohs.Count < 4) return null;

            var dynastyGroups = pharaohs.GroupBy(p => p.Dynasty).Where(g => g.Count() >= 1).ToList();
            if (dynastyGroups.Count < 2) return null;

            var chosenGroup = dynastyGroups[_rng.Next(dynastyGroups.Count)];
            var correct = chosenGroup.ElementAt(_rng.Next(chosenGroup.Count()));
            var dynastyLabel = Pick(correct.Dynasty, correct.DynastyAr, lang);

            var outsiders = pharaohs.Where(p => p.Dynasty != correct.Dynasty).ToList();
            if (outsiders.Count < 3) return null;
            var wrongNames = outsiders.OrderBy(_ => _rng.Next()).Take(3)
                .Select(p => Pick(p.Name, p.NameAr, lang)).Distinct().ToList();
            if (wrongNames.Count < 3) return null;

            var text = Fmt(
                new[] { "Which pharaoh ruled during the {0}?", "Who was a pharaoh from the {0}?" },
                new[] { "مين حكم في الـ\"{0}\"؟", "مين من الفراعنة اللي حكموا في \"{0}\"؟" },
                lang, _rng, dynastyLabel);

            var correctName = Pick(correct.Name, correct.NameAr, lang);
            return new QuizQuestion { Category = "Pharaoh", Text = text, Choices = BuildChoices(correctName, wrongNames) };
        }

        // =====================================================================
        // 3) إله → رمز (Forward)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateGodSymbolQuestion(string lang, QuizDifficulty difficulty)
        {
            var gods = await _db.Gods.Where(g => !string.IsNullOrWhiteSpace(g.Symbol)).ToListAsync();
            if (gods.Count < 4) return null;

            var correct = gods[_rng.Next(gods.Count)];
            var distractors = gods.Where(g => g.Id != correct.Id).OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = Fmt(
                new[] { "What is the symbol of the god \"{0}\"?", "\"{0}\" is symbolized by what?" },
                new[] { "إيه رمز الإله \"{0}\"؟", "الإله \"{0}\" بيترمز بإيه؟" },
                lang, _rng, name);

            var choices = BuildChoices(
                Pick(correct.Symbol, correct.SymbolAr, lang),
                distractors.Select(g => Pick(g.Symbol, g.SymbolAr, lang)));

            return new QuizQuestion { Category = "God", Text = text, Choices = choices };
        }

        // =====================================================================
        // 4) رمز → إله (Reverse)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateGodByReverseSymbolQuestion(string lang, QuizDifficulty difficulty)
        {
            var gods = await _db.Gods.Where(g => !string.IsNullOrWhiteSpace(g.Symbol)).ToListAsync();
            if (gods.Count < 4) return null;

            var correct = gods[_rng.Next(gods.Count)];
            var symbolLabel = Pick(correct.Symbol, correct.SymbolAr, lang);

            var distractors = gods.Where(g => g.Id != correct.Id).OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var text = Fmt(
                new[] { "Which god is symbolized by \"{0}\"?", "The symbol \"{0}\" belongs to which god?" },
                new[] { "مين الإله اللي بيترمز بـ\"{0}\"؟", "الرمز \"{0}\" بيخص أنهي إله؟" },
                lang, _rng, symbolLabel);

            var choices = BuildChoices(
                Pick(correct.Name, correct.NameAr, lang),
                distractors.Select(g => Pick(g.Name, g.NameAr, lang)));

            return new QuizQuestion { Category = "God", Text = text, Choices = choices };
        }

        // =====================================================================
        // 5) معبد → مكان (Forward)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateTempleLocationQuestion(string lang, QuizDifficulty difficulty)
        {
            var temples = await _db.Temples.ToListAsync();
            if (temples.Count < 4) return null;

            var correct = temples[_rng.Next(temples.Count)];
            var distractors = temples.Where(t => t.Id != correct.Id && t.Location != correct.Location)
                .OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = Fmt(
                new[] { "Where is \"{0}\" located?", "\"{0}\" can be found in which location?" },
                new[] { "معبد \"{0}\" موجود فين؟", "\"{0}\" واقع في أنهي مكان؟" },
                lang, _rng, name);

            var choices = BuildChoices(
                Pick(correct.Location, correct.LocationAr, lang),
                distractors.Select(t => Pick(t.Location, t.LocationAr, lang)));

            return new QuizQuestion { Category = "Temple", Text = text, Choices = choices };
        }

        // =====================================================================
        // 6) مكان → معبد (Reverse) — لازم الموقع يكون فريد لمعبد واحد بس عشان مايبقاش فيه أكتر من إجابة صح
        // =====================================================================
        private async Task<QuizQuestion?> GenerateTempleByLocationQuestion(string lang, QuizDifficulty difficulty)
        {
            var temples = await _db.Temples.ToListAsync();
            var uniqueLocationTemples = temples.GroupBy(t => t.Location).Where(g => g.Count() == 1)
                .Select(g => g.First()).ToList();
            if (uniqueLocationTemples.Count < 1 || temples.Count < 4) return null;

            var correct = uniqueLocationTemples[_rng.Next(uniqueLocationTemples.Count)];
            var locationLabel = Pick(correct.Location, correct.LocationAr, lang);

            var distractors = temples.Where(t => t.Id != correct.Id).OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var text = Fmt(
                new[] { "Which temple is located in \"{0}\"?", "You'll find which temple in \"{0}\"?" },
                new[] { "أنهي معبد موجود في \"{0}\"؟", "في \"{0}\" هتلاقي أنهي معبد؟" },
                lang, _rng, locationLabel);

            var choices = BuildChoices(
                Pick(correct.Name, correct.NameAr, lang),
                distractors.Select(t => Pick(t.Name, t.NameAr, lang)));

            return new QuizQuestion { Category = "Temple", Text = text, Choices = choices };
        }

        // =====================================================================
        // 7) متحف → مكان (Forward)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateMuseumLocationQuestion(string lang, QuizDifficulty difficulty)
        {
            var museums = await _db.Museums.ToListAsync();
            if (museums.Count < 4) return null;

            var correct = museums[_rng.Next(museums.Count)];
            var distractors = museums.Where(m => m.Id != correct.Id && m.Location != correct.Location)
                .OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = Fmt(
                new[] { "Where is \"{0}\" located?", "\"{0}\" is situated in which location?" },
                new[] { "متحف \"{0}\" موجود فين؟", "\"{0}\" واقع في أنهي مكان؟" },
                lang, _rng, name);

            var choices = BuildChoices(
                Pick(correct.Location, correct.LocationAr, lang),
                distractors.Select(m => Pick(m.Location, m.LocationAr, lang)));

            return new QuizQuestion { Category = "Museum", Text = text, Choices = choices };
        }

        // =====================================================================
        // 8) مكان → متحف (Reverse)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateMuseumByLocationQuestion(string lang, QuizDifficulty difficulty)
        {
            var museums = await _db.Museums.ToListAsync();
            var uniqueLocationMuseums = museums.GroupBy(m => m.Location).Where(g => g.Count() == 1)
                .Select(g => g.First()).ToList();
            if (uniqueLocationMuseums.Count < 1 || museums.Count < 4) return null;

            var correct = uniqueLocationMuseums[_rng.Next(uniqueLocationMuseums.Count)];
            var locationLabel = Pick(correct.Location, correct.LocationAr, lang);

            var distractors = museums.Where(m => m.Id != correct.Id).OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var text = Fmt(
                new[] { "Which museum is located in \"{0}\"?", "You'll find which museum in \"{0}\"?" },
                new[] { "أنهي متحف موجود في \"{0}\"؟", "في \"{0}\" هتلاقي أنهي متحف؟" },
                lang, _rng, locationLabel);

            var choices = BuildChoices(
                Pick(correct.Name, correct.NameAr, lang),
                distractors.Select(m => Pick(m.Name, m.NameAr, lang)));

            return new QuizQuestion { Category = "Museum", Text = text, Choices = choices };
        }

        // =====================================================================
        // 9) أسرة → عصر (Forward)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateDynastyEraQuestion(string lang, QuizDifficulty difficulty)
        {
            var dynasties = await _db.Dynasties.ToListAsync();
            if (dynasties.Count < 4) return null;

            var correct = dynasties[_rng.Next(dynasties.Count)];
            var pool = dynasties.Where(d => d.Id != correct.Id && d.Era != correct.Era).ToList();
            var ordered = OrderByYearProximity(correct, pool, difficulty, d => d.StartYear).Take(3).ToList();
            if (ordered.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = Fmt(
                new[] { "\"{0}\" belonged to which era?", "Which era does \"{0}\" fall under?" },
                new[] { "\"{0}\" كانت جزء من أنهي عصر؟", "\"{0}\" بتتبع أنهي عصر؟" },
                lang, _rng, name);

            var choices = BuildChoices(
                Pick(correct.Era, correct.EraAr, lang),
                ordered.Select(d => Pick(d.Era, d.EraAr, lang)));

            return new QuizQuestion { Category = "Dynasty", Text = text, Choices = choices };
        }

        // =====================================================================
        // 10) عصر → أسرة (Reverse)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateDynastyByEraQuestion(string lang, QuizDifficulty difficulty)
        {
            var dynasties = await _db.Dynasties.ToListAsync();
            if (dynasties.Count < 4) return null;

            var eraGroups = dynasties.GroupBy(d => d.Era).ToList();
            if (eraGroups.Count < 2) return null;

            var chosenGroup = eraGroups[_rng.Next(eraGroups.Count)];
            var correct = chosenGroup.ElementAt(_rng.Next(chosenGroup.Count()));
            var eraLabel = Pick(correct.Era, correct.EraAr, lang);

            var outsiders = dynasties.Where(d => d.Era != correct.Era).ToList();
            if (outsiders.Count < 3) return null;
            var ordered = OrderByYearProximity(correct, outsiders, difficulty, d => d.StartYear).Take(3).ToList();
            if (ordered.Count < 3) return null;

            var text = Fmt(
                new[] { "Which dynasty belongs to the \"{0}\" era?", "\"{0}\" era includes which dynasty?" },
                new[] { "أنهي أسرة بتتبع عصر \"{0}\"؟", "عصر \"{0}\" بيشمل أنهي أسرة؟" },
                lang, _rng, eraLabel);

            var choices = BuildChoices(
                Pick(correct.Name, correct.NameAr, lang),
                ordered.Select(d => Pick(d.Name, d.NameAr, lang)));

            return new QuizQuestion { Category = "Dynasty", Text = text, Choices = choices };
        }

        // =====================================================================
        // 11) حدث تاريخي → سنة (Forward) — الفرق بين السنين بيصغر مع زيادة الصعوبة
        // =====================================================================
        private async Task<QuizQuestion?> GenerateHistoricalEventYearQuestion(string lang, QuizDifficulty difficulty)
        {
            var events = await _db.HistoricalEvents.ToListAsync();
            if (events.Count < 1) return null;

            var correct = events[_rng.Next(events.Count)];
            var title = Pick(correct.Title, correct.TitleAr, lang);
            var text = Fmt(
                new[] { "Around which year did \"{0}\" happen?", "\"{0}\" took place around which year?" },
                new[] { "\"{0}\" حصل في أنهي سنة تقريبًا؟", "\"{0}\" وقع تقريبًا في أنهي سنة؟" },
                lang, _rng, title);

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

        // =====================================================================
        // 12) قطعة أثرية → متحف
        // ✅ Artifact.cs الحقيقي فيه Museum / MuseumAr (مش MuseumName / MuseumNameAr)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateArtifactMuseumQuestion(string lang, QuizDifficulty difficulty)
        {
            var artifacts = await _db.Artifacts.Where(a => !string.IsNullOrWhiteSpace(a.Museum)).ToListAsync();
            if (artifacts.Count < 4) return null;

            var correct = artifacts[_rng.Next(artifacts.Count)];
            var distractors = artifacts.Where(a => a.Id != correct.Id && a.Museum != correct.Museum)
                .OrderBy(_ => _rng.Next()).Take(3).ToList();
            if (distractors.Count < 3) return null;

            var name = Pick(correct.Name, correct.NameAr, lang);
            var text = Fmt(
                new[] { "In which museum is the artifact \"{0}\" displayed?", "\"{0}\" is displayed in which museum?" },
                new[] { "القطعة الأثرية \"{0}\" معروضة في أنهي متحف؟", "\"{0}\" هتلاقيها في أنهي متحف؟" },
                lang, _rng, name);

            var correctMuseum = Pick(correct.Museum, correct.MuseumAr, lang);
            var choices = BuildChoices(correctMuseum, distractors.Select(a => Pick(a.Museum, a.MuseumAr, lang)));
            return new QuizQuestion { Category = "Artifact", Text = text, Choices = choices };
        }

        // =====================================================================
        // 13) True / False — شكل مختلف تمامًا (اختيارين بس)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateTrueFalsePharaohDynastyQuestion(string lang, QuizDifficulty difficulty)
        {
            var pharaohs = await _db.Pharaohs.ToListAsync();
            if (pharaohs.Count < 4) return null;

            var correct = pharaohs[_rng.Next(pharaohs.Count)];
            var name = Pick(correct.Name, correct.NameAr, lang);

            bool makeTrue = _rng.Next(2) == 0;
            string dynastyToShow;

            if (makeTrue)
            {
                dynastyToShow = Pick(correct.Dynasty, correct.DynastyAr, lang);
            }
            else
            {
                var others = pharaohs.Where(p => p.Dynasty != correct.Dynasty).ToList();
                if (others.Count == 0) return null;
                var wrong = others[_rng.Next(others.Count)];
                dynastyToShow = Pick(wrong.Dynasty, wrong.DynastyAr, lang);
            }

            var text = Fmt(
                new[] { "True or False: \"{0}\" belonged to the \"{1}\"." },
                new[] { "صح ولا غلط: \"{0}\" كان فرعون من \"{1}\"." },
                lang, _rng, name, dynastyToShow);

            var trueLabel = lang == "ar" ? "صح" : "True";
            var falseLabel = lang == "ar" ? "غلط" : "False";

            return new QuizQuestion { Category = "Pharaoh", Text = text, Choices = BuildBinaryChoices(trueLabel, falseLabel, makeTrue) };
        }

        // =====================================================================
        // 14) Odd-One-Out — 3 فراعنة من نفس الأسرة + 1 من أسرة تانية
        // =====================================================================
        private async Task<QuizQuestion?> GenerateOddOneOutDynastyQuestion(string lang, QuizDifficulty difficulty)
        {
            var pharaohs = await _db.Pharaohs.ToListAsync();
            var groups = pharaohs.GroupBy(p => p.Dynasty).Where(g => g.Count() >= 3).ToList();
            if (groups.Count < 1) return null;

            var mainGroup = groups[_rng.Next(groups.Count)];
            var insiders = mainGroup.OrderBy(_ => _rng.Next()).Take(3).ToList();

            var outsiders = pharaohs.Where(p => p.Dynasty != mainGroup.Key).ToList();
            if (outsiders.Count < 1) return null;
            var outsider = outsiders[_rng.Next(outsiders.Count)];

            var dynastyLabel = Pick(mainGroup.Key, insiders.First().DynastyAr, lang);
            var text = Fmt(
                new[] { "Which one of these does NOT belong to the \"{0}\"?", "Spot the odd one out — who is NOT from the \"{0}\"?" },
                new[] { "مين من دول مش من \"{0}\"؟", "دور على الغريب — مين مش من \"{0}\"؟" },
                lang, _rng, dynastyLabel);

            var choices = new List<QuizChoice> { new QuizChoice { Text = Pick(outsider.Name, outsider.NameAr, lang), IsCorrect = true } };
            foreach (var p in insiders)
                choices.Add(new QuizChoice { Text = Pick(p.Name, p.NameAr, lang), IsCorrect = false });

            return new QuizQuestion { Category = "Pharaoh", Text = text, Choices = choices.OrderBy(_ => _rng.Next()).ToList() };
        }

        // =====================================================================
        // 15) Cross-Reference (Hard): حدث تاريخي → أسرة (عن طريق DynastyTag)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateEventDynastyLinkQuestion(string lang, QuizDifficulty difficulty)
        {
            var events = await _db.HistoricalEvents.Where(e => e.DynastyTag != null && e.DynastyTag != "").ToListAsync();
            if (events.Count < 1) return null;

            var dynasties = await _db.Dynasties.ToListAsync();
            if (dynasties.Count < 4) return null;

            var correct = events[_rng.Next(events.Count)];
            var distractors = dynasties.Where(d => d.Name != correct.DynastyTag)
                .OrderBy(_ => _rng.Next()).Take(3).Select(d => d.Name).ToList();
            if (distractors.Count < 3) return null;

            var title = Pick(correct.Title, correct.TitleAr, lang);
            var text = Fmt(
                new[] { "The event \"{0}\" is linked to which dynasty?", "Which dynasty is connected to \"{0}\"?" },
                new[] { "الحدث \"{0}\" مرتبط بأنهي أسرة؟", "أنهي أسرة مرتبطة بحدث \"{0}\"؟" },
                lang, _rng, title);

            var choices = BuildChoices(correct.DynastyTag!, distractors);
            return new QuizQuestion { Category = "HistoricalEvent", Text = text, Choices = choices };
        }

        // =====================================================================
        // 16) Cross-Reference (Hard): حدث تاريخي → فرعون (عن طريق PharaohTag)
        // =====================================================================
        private async Task<QuizQuestion?> GenerateEventPharaohLinkQuestion(string lang, QuizDifficulty difficulty)
        {
            var events = await _db.HistoricalEvents.Where(e => e.PharaohTag != null && e.PharaohTag != "").ToListAsync();
            if (events.Count < 1) return null;

            var pharaohs = await _db.Pharaohs.ToListAsync();
            if (pharaohs.Count < 4) return null;

            var correct = events[_rng.Next(events.Count)];
            var distractors = pharaohs.Where(p => p.Name != correct.PharaohTag)
                .OrderBy(_ => _rng.Next()).Take(3).Select(p => p.Name).ToList();
            if (distractors.Count < 3) return null;

            var title = Pick(correct.Title, correct.TitleAr, lang);
            var text = Fmt(
                new[] { "Which pharaoh is linked to the event \"{0}\"?", "The event \"{0}\" is connected to which pharaoh?" },
                new[] { "أنهي فرعون مرتبط بحدث \"{0}\"؟", "الحدث \"{0}\" مرتبط بأنهي فرعون؟" },
                lang, _rng, title);

            var choices = BuildChoices(correct.PharaohTag!, distractors);
            return new QuizQuestion { Category = "HistoricalEvent", Text = text, Choices = choices };
        }
    }
}
