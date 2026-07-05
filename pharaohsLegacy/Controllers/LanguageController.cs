using Microsoft.AspNetCore.Mvc;

namespace pharaohsLegacy.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult SetLanguage(string lang, string returnUrl)
        {
            HttpContext.Session.SetString("Lang", lang);
            return Redirect(returnUrl ?? "/");
        }
    }
}