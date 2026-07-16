using System.ComponentModel.DataAnnotations;

namespace PharaohsLegacy.Models
{
    // ==========================================================================
    // ViewModel لصفحة الدفع — الفيلدز مطابقة تمامًا لبيانات الكارت في الـ Bank Service
    // ==========================================================================
    public class BookingPaymentViewModel
    {
        public int BookingId { get; set; }
        public string BookingTitle { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "اسم حامل الكارت مطلوب")]
        [StringLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الكارت مطلوب")]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$", ErrorMessage = "رقم الكارت لازم يكون 16 رقم")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "الصيغة المطلوبة MM/YY")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV مطلوب")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV لازم يكون 3 أرقام")]
        public string Cvv { get; set; } = string.Empty;

        public string? CouponCode { get; set; }
    }

    // شكل رد البنك — يتحط في نفس المشروع (DTOs)
    public class ChargeResult
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public decimal original_amount { get; set; }
        public decimal discount_applied { get; set; }
        public decimal final_amount { get; set; }
        public decimal balance_after { get; set; }
    }

    public class RefundResult
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public decimal refunded_amount { get; set; }
        public decimal balance_after { get; set; }
    }

    public class ErrorResult
    {
        public string detail { get; set; } = string.Empty;
    }
}

/*
==============================================================================
مثال Action إضافي مطلوب في BookingController — للتحقق من الكوبون بدون استخدامه
(الـ JS في الـ View بينادي عليه: /Booking/ValidateCoupon?code=...)
==============================================================================

[HttpGet]
public async Task<IActionResult> ValidateCoupon(string code)
{
    var client = _httpClientFactory.CreateClient("BankService");
    var response = await client.PostAsJsonAsync("coupons/validate", new
    {
        code = code,
        user_email = User.Identity.Name
    });

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadFromJsonAsync<ErrorResult>();
        return Json(new { valid = false, message = error?.detail ?? "كود غير صالح" });
    }

    var result = await response.Content.ReadFromJsonAsync<dynamic>();
    return Json(new { valid = true, discountPercent = result.discount_percent });
}
*/
