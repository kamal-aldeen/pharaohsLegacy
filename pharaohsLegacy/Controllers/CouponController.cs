using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using System.Net.Http.Json;

namespace pharaohsLegacy.Controllers
{
    public class CouponController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CouponController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET /Coupon/MyCoupons
        public async Task<IActionResult> MyCoupons()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var coupons = new List<CouponListItem>();
            ViewBag.BankConnectionError = false;

            try
            {
                var client = _httpClientFactory.CreateClient("BankService");
                var response = await client.GetAsync($"coupons/{Uri.EscapeDataString(userEmail)}");

                if (response.IsSuccessStatusCode)
                {
                    coupons = await response.Content.ReadFromJsonAsync<List<CouponListItem>>() ?? new List<CouponListItem>();
                }
                // 404 = مفيش endpoint أو مفيش كوبونات لليوزر ده لسه — الصفحة تعرض "مفيش كوبونات" عادي
                else if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    ViewBag.BankConnectionError = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BankService] Coupons fetch failed for {userEmail}: {ex.Message}");
                ViewBag.BankConnectionError = true;
            }

            var ordered = coupons
                .OrderBy(c => c.is_used)                         // النشطة الأول
                .ThenByDescending(c => c.expires_at)
                .ToList();

            return View(ordered);
        }
    }
}
