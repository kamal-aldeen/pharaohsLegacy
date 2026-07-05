using Microsoft.AspNetCore.Mvc.Rendering;
using pharaohsLegacy.Services;

namespace pharaohsLegacy.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string L(this IHtmlHelper html, string key)
        {
            var lang = html.ViewContext.HttpContext.Session.GetString("Lang") ?? "ar";
            var service = html.ViewContext.HttpContext.RequestServices.GetRequiredService<LocalizationService>();
            return service.Get(key, lang);
        }
    }
}