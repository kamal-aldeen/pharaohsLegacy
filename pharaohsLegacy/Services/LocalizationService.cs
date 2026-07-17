using System.Text.Json;

namespace pharaohsLegacy.Services
{
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new();

        public LocalizationService(IWebHostEnvironment env)
        {
            foreach (var lang in new[] { "ar", "en" })
            {
                var path = Path.Combine(env.WebRootPath, "lang", $"{lang}.json");
                var json = File.ReadAllText(path);
                _translations[lang] = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }

        public string Get(string key, string lang)
        {
            if (_translations.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
                return value;
            return key; // fallback لو المفتاح مش موجود
        }

        // 🆕 لرسائل فيها قيمة متغيرة (زي "وفرت {0} EGP بالكوبون") — بيجيب النص المترجم
        // ويعمله Format بالقيم اللي جاية من الكود، بدل ما نبني الرسالة يدويًا بلغة واحدة بس
        public string GetFormatted(string key, string lang, params object[] args)
        {
            var template = Get(key, lang);
            return args.Length > 0 ? string.Format(template, args) : template;
        }
    }
}