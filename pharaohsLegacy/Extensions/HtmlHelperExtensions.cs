using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using pharaohsLegacy.Services;

namespace pharaohsLegacy.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string L(this IHtmlHelper html, string key)
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "en";
            var service = html.ViewContext.HttpContext.RequestServices.GetRequiredService<LocalizationService>();
            return service.Get(key, lang);
        }

        public static IHtmlContent D(this IHtmlHelper html, string? arabicValue, string englishValue)
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "en";
            var value = (lang == "ar" && !string.IsNullOrEmpty(arabicValue))
                ? ToArabicDigits(arabicValue)
                : englishValue;
            return new HtmlString(html.Encode(value));
        }

        // بقت public عشان تتنادى من أي مكان تاني (Views أو Controllers) لو احتجت
        public static string ToArabicDigits(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input ?? "";
            var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
            var sb = new System.Text.StringBuilder();
            foreach (var c in input)
                sb.Append(c >= '0' && c <= '9' ? arabicDigits[c - '0'] : c);
            return sb.ToString();
        }

        // لأي رقم/نص فيه أرقام هيتعرض مباشرة من غير صيغة خاصة (زي YearLabel أو DateLoc)
        public static string Digits(this IHtmlHelper html, object? value)
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "en";
            var str = value?.ToString() ?? "";
            return lang == "ar" ? ToArabicDigits(str) : str;
        }

        public static string DateLoc(this IHtmlHelper html, DateTime date, string format = "dd MMM yyyy — hh:mm tt")
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "en";
            var culture = lang == "ar"
                ? new System.Globalization.CultureInfo("ar-EG")
                : new System.Globalization.CultureInfo("en-US");

            var formatted = date.ToString(format, culture);
            return lang == "ar" ? ToArabicDigits(formatted) : formatted;
        }

        public static string YearLabel(this IHtmlHelper html, int year)
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "en";
            var absYear = Math.Abs(year);

            if (lang == "ar")
            {
                var digits = ToArabicDigits(absYear.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return year < 0 ? $"{digits} ق.م" : $"{digits} م";
            }
            return year < 0 ? $"{absYear} BC" : $"{year} AD";
        }

        // لأي رقم خام (سنة تأسيس، سعر، عدد...) عايزه يتحول لأرقام عربية حسب اللغة
        public static string Num(this IHtmlHelper html, object? value)
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "en";
            var str = value?.ToString() ?? "";
            return lang == "ar" ? ToArabicDigits(str) : str;
        }
    }
}