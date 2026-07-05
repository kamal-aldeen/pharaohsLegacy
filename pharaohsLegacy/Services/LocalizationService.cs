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
    }
}