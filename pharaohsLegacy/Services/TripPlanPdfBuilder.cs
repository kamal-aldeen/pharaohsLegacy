using pharaohsLegacy.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace pharaohsLegacy.Services
{
    // ---------------------------------------------------------
    // TripPlanPdfBuilder — بيبني ملف PDF لخطة رحلة واحدة بالـ QuestPDF.
    // بيدعم عربي (RTL + فونت Amiri) وإنجليزي (LTR + Arial)
    // حسب لغة الموقع الحالية (Session["Lang"]).
    //
    // ⚠️ قاعدة مهمة جدًا اتأكدت بالتجربة الفعلية (راجع نسخة PDF فعلية طلعت مكسورة):
    //    محرك الـ Arabic shaping بتاع QuestPDF/SkiaSharp بيكسر حرف "ي" (ولقيل احتمال حروف تانية)
    //    لما تتحط كلمة عربية ورقم في نفس الـ Text() Call الواحد — حتى لو استخدمت .Span() منفصلة،
    //    برضه بيتحسبوا كـ paragraph/shaping run واحد. الحل المؤكد: أي رقم ولزيقه كلمة عربية
    //    لازم يتحطوا في عنصرين Text() منفصلين تمامًا (مش بس Span منفصل) جوه Row.
    //    القاعدة دي متطبقة في كل الملف ده — متتلغيش لو بتضيف نص جديد بعدين.
    // ---------------------------------------------------------
    public static class TripPlanPdfBuilder
    {
        // نفس الـ palette المستخدم في Light Mode بتاع الموقع (Result.cshtml)
        // بس أهدأ شوية عشان يبان كويس على الورق/الطباعة
        private const string Gold = "#b8860b";
        private const string GoldBright = "#a9780f";
        private const string Papyrus = "#fffaf0";
        private const string PapyrusAlt = "#f3e8d0";
        private const string TextDark = "#3b2411";
        private const string TextDim = "#8a6f45";
        private const string Line = "#e0cfa0";

        public static byte[] Build(TripPlan plan, string lang, LocalizationService loc)
        {
            var isRtl = lang == "ar";
            var fontFamily = isRtl ? "Amiri" : "Arial";

            var modeLabels = new Dictionary<string, string>
            {
                { "Family", loc.Get("trip.mode.family", lang) },
                { "Student", loc.Get("trip.mode.student", lang) },
                { "Luxury", loc.Get("trip.mode.luxury", lang) }
            };
            var interestLabels = new Dictionary<string, string>
            {
                { "Temple", loc.Get("trip.interest.temple", lang) },
                { "Museum", loc.Get("trip.interest.museum", lang) }
            };
            var placeGlyphs = new Dictionary<string, string>
            {
                { "Temple", "𓊪" },
                { "Museum", "𓏛" }
            };

            var interestsList = (plan.Interests ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            var dayGroups = plan.Stops.GroupBy(s => s.DayNumber).OrderBy(g => g.Key).ToList();
            var totalCost = plan.Stops.Sum(s => s.EstimatedCost);
            var modeLabel = modeLabels.TryGetValue(plan.Mode, out var ml) ? ml : plan.Mode;
            var currency = loc.Get("trip.currency", lang);
            var dayLabel = loc.Get("trip.result.day.label", lang);

            string Num(object n) => ToLocalizedDigits(n?.ToString() ?? "", lang);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.PageColor(Papyrus);
                    page.DefaultTextStyle(x => x.FontFamily(fontFamily).FontSize(10.5f).FontColor(TextDark));

                    if (isRtl)
                        page.ContentFromRightToLeft();

                    // ---------------- Header ----------------
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text("𓁹 Pharaohs Legacy 𓁹").FontSize(13).Bold().FontColor(GoldBright);
                                inner.Item().PaddingTop(2).Text(loc.Get("trip.pdf.title", lang)).FontSize(18).Bold().FontColor(TextDark);
                            });
                            // 🔧 اتقسّم لعنصرين Text() منفصلين (مش .Span() في نفس الـ Text)
                            // عشان اللاصقة "تم الإنشاء في" + التاريخ اللاتيني كانت بتكسر حرف "ي"
                            row.ConstantItem(180).Row(dateRow =>
                            {
                                dateRow.RelativeItem().AlignRight().AlignMiddle()
                                    .Text(loc.Get("trip.pdf.generated", lang)).FontSize(9).FontColor(TextDim);
                                dateRow.AutoItem().PaddingHorizontal(4).AlignMiddle()
                                    .Text(plan.CreatedAt.ToString("d MMMM yyyy")).FontSize(9).FontColor(TextDim);
                            });
                        });
                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Gold);
                    });

                    // ---------------- Content ----------------
                    page.Content().PaddingTop(16).Column(col =>
                    {
                        col.Spacing(14);

                        // Summary pills
                        col.Item().Row(row =>
                        {
                            row.Spacing(8);
                            row.AutoItem().Element(c => SummaryPill(c, "📅", Num(plan.Days), loc.Get("trip.stat.days", lang)));
                            row.AutoItem().Element(c => SummaryPill(c, "💰", Num(plan.Budget.ToString("N0")), currency));
                            row.AutoItem().Element(c => SummaryPill(c, "📊", Num(totalCost.ToString("N0")), currency));
                            row.AutoItem().Element(c => SummaryPill(c, "🎯", null, modeLabel));
                        });

                        col.Item().Text(string.Join(" · ", interestsList.Select(i => interestLabels.TryGetValue(i, out var l) ? l : i)))
                            .FontSize(9.5f).FontColor(TextDim);

                        // Day-by-day itinerary
                        foreach (var day in dayGroups)
                        {
                            var dayCost = day.Sum(s => s.EstimatedCost);
                            var stops = day.OrderBy(s => s.SuggestedTime).ToList();
                            col.Item().Element(c => DaySection(c, day.Key, dayCost, stops, placeGlyphs, dayLabel, currency, Num));
                        }
                    });

                    // ---------------- Footer ----------------
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Line);
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text(loc.Get("trip.pdf.footer", lang)).FontSize(8).FontColor(TextDim);
                            // أرقام الصفحات لاتينية بحتة (مفيش عربي هنا)، آمن تفضل في نفس الـ Text()
                            row.ConstantItem(80).AlignRight().Text(x =>
                            {
                                x.CurrentPageNumber().FontSize(8).FontColor(TextDim);
                                x.Span(" / ").FontSize(8).FontColor(TextDim);
                                x.TotalPages().FontSize(8).FontColor(TextDim);
                            });
                        });
                    });
                });
            }).GeneratePdf();
        }

        // 🔧 emoji / رقم / تسمية — تلات عناصر Text() منفصلة تمامًا جوه نفس الـ pill
        // (رقم = null لو الـ pill مالهوش رقم زي pill الـ Mode)
        private static void SummaryPill(IContainer container, string emoji, string? number, string label)
        {
            container
                .Background(PapyrusAlt)
                .Border(1).BorderColor(Line)
                .CornerRadius(999)
                .Padding(6).PaddingHorizontal(12)
                .Row(row =>
                {
                    row.AutoItem().Text(emoji).FontSize(9.5f);
                    if (!string.IsNullOrEmpty(number))
                        row.AutoItem().PaddingHorizontal(4).Text(number).FontSize(9.5f).FontColor(TextDark).Bold();
                    row.AutoItem().Text(label).FontSize(9.5f).FontColor(TextDark).Bold();
                });
        }

        private static void DaySection(
            IContainer container,
            int dayNumber,
            decimal dayCost,
            List<TripPlanStop> stops,
            Dictionary<string, string> placeGlyphs,
            string dayLabel,
            string currency,
            Func<object, string> num)
        {
            container.Border(1).BorderColor(Line).CornerRadius(6).Column(col =>
            {
                // Day header
                col.Item().Background(PapyrusAlt).Padding(8).Row(row =>
                {
                    row.ConstantItem(24).Height(24).Background(Gold).AlignCenter().AlignMiddle()
                        .Text(num(dayNumber)).FontColor(Papyrus).Bold().FontSize(10);

                    // 🔧 "يوم" + رقم اليوم اتقسّموا لعنصرين Text() منفصلين (مش string واحد ملزّق)
                    row.RelativeItem().PaddingHorizontal(8).Row(labelRow =>
                    {
                        labelRow.AutoItem().AlignMiddle().Text(dayLabel).Bold().FontSize(11);
                        labelRow.AutoItem().PaddingHorizontal(4).AlignMiddle().Text(num(dayNumber)).Bold().FontSize(11);
                    });

                    // 🔧 تكلفة اليوم + "جنيه" اتقسّموا لعنصرين Text() منفصلين
                    row.AutoItem().Row(costRow =>
                    {
                        costRow.AutoItem().AlignMiddle().Text(num(dayCost.ToString("N0"))).FontSize(9).FontColor(TextDim);
                        costRow.AutoItem().PaddingHorizontal(3).AlignMiddle().Text(currency).FontSize(9).FontColor(TextDim);
                    });
                });

                // Stops
                foreach (var stop in stops)
                {
                    var glyph = placeGlyphs.TryGetValue(stop.PlaceType, out var g) ? g : "𓆓";
                    col.Item().BorderTop(1).BorderColor(Line).Padding(8).Row(row =>
                    {
                        row.ConstantItem(24).AlignMiddle().Text(glyph).FontSize(13).FontColor(Gold);
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().Row(r2 =>
                            {
                                r2.RelativeItem().Text(stop.PlaceName).Bold().FontSize(10.5f);

                                if (!string.IsNullOrWhiteSpace(stop.SuggestedTime))
                                {
                                    // 🔧 🕐 اتفصلت عن نص الوقت في عنصر Text() لوحدها
                                    r2.AutoItem().PaddingHorizontal(3).Text("🕐").FontSize(9);
                                    r2.AutoItem().PaddingHorizontal(3).Text(num(stop.SuggestedTime)).FontSize(9).FontColor(GoldBright);
                                }

                                // 🔧 السعر + "جنيه" اتقسّموا لعنصرين Text() منفصلين
                                r2.AutoItem().PaddingHorizontal(3).Text(num(stop.EstimatedCost.ToString("N0"))).FontSize(9).FontColor(TextDim);
                                r2.AutoItem().Text(currency).FontSize(9).FontColor(TextDim);
                            });
                            if (!string.IsNullOrWhiteSpace(stop.Notes))
                                inner.Item().PaddingTop(2).Text(stop.Notes).FontSize(9).FontColor(TextDim);
                        });
                    });
                }
            });
        }

        // بيحول الأرقام الإنجليزية (0-9) لأرقام هندية-عربية (٠-٩) لو اللغة عربي،
        // بنفس فكرة Html.Num() في الـ Views (بس نسخة بسيطة تشتغل من الـ Controller/Service)
        private static string ToLocalizedDigits(string input, string lang)
        {
            if (lang != "ar" || string.IsNullOrEmpty(input))
                return input;

            var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
            var chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] >= '0' && chars[i] <= '9')
                    chars[i] = arabicDigits[chars[i] - '0'];
            }
            return new string(chars);
        }
    }
}
