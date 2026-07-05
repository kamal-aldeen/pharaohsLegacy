using Microsoft.AspNetCore.Mvc;

namespace pharaohsLegacy.Controllers
{
    public class HieroglyphicsController : Controller
    {
        public IActionResult Translator()
        {
            var userName = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserName = userName == "guest" || string.IsNullOrEmpty(userName) ? "" : userName;
            return View();
        }
    }
}