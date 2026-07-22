using System.ComponentModel.DataAnnotations;

namespace pharaohsLegacy.Models
{
    // ==========================================================================
    // شكل رد الـ Bank Service — الأسماء لازم تفضل زي ما هي (snake_case) عشان
    // تتطابق مع الـ JSON اللي راجع من Python بدون أي تعديل إضافي
    // ==========================================================================

    public class ChargeResult
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public decimal original_amount { get; set; }
        public decimal discount_applied { get; set; }
        public decimal final_amount { get; set; }
        public decimal balance_after { get; set; }
    }

    public class RefundResult
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public decimal refunded_amount { get; set; }
        public decimal balance_after { get; set; }
    }

    public class CouponValidateResult
    {
        public bool valid { get; set; }
        public double discount_percent { get; set; }
        public DateTime? expires_at { get; set; }
    }

    // ==========================================================================
    // 🆕 رد POST /coupons/create من خدمة البنك — بينادى من QuizController لما
    // اليوزر يخلص الكويز بنتيجة ≥ 70% ويستاهل كوبون.
    // ⚠️ الأسامي دي تخمين معقول بناءً على شكل CouponValidateResult فوق — لو
    // شكل الـ schema عندك في main.py (بايثون) مختلف، عدّل هنا بس.
    // ==========================================================================
    public class CouponCreateResult
    {
        public bool success { get; set; }
        public string code { get; set; } = "";
        public double discount_percent { get; set; }
        public DateTime? expires_at { get; set; }
    }

    // ==========================================================================
    // 🆕 عنصر واحد في رد GET /coupons/user/{email} — كل الكوبونات اللي اليوزر
    // كسبها (من الكويز أو أي مصدر تاني مستقبلي)، مستخدمة كانت ولا لسه.
    // ⚠️ نفس التحذير: الأسامي تخمين معقول — لازم تتأكد من شكل الـ endpoint ده
    // في خدمة البنك (main.py). لو الـ endpoint مش موجود أصلاً، محتاج تضيفه هناك الأول.
    // ==========================================================================
    public class CouponListItem
    {
        public string code { get; set; } = "";
        public double discount_percent { get; set; }
        public DateTime? expires_at { get; set; }
        public bool is_used { get; set; }
    }

    public class BankErrorResult
    {
        public string detail { get; set; } = "";
    }

    // ==========================================================================
    // بيانات الدفع اللي جايه من فورم صفحة الحجز (Create.cshtml)
    // اتضافت كـ parameters في BookingController.Confirm — مش لازم تتربط بـ class
    // منفصل، لكن عملتها هنا عشان نقدر نعمل عليها Validation بسيطة قبل ما نبعتها للبنك
    // ==========================================================================

    public class CardPaymentInput
    {
        [Required(ErrorMessage = "اسم حامل الكارت مطلوب")]
        public string CardHolderName { get; set; } = "";

        [Required(ErrorMessage = "رقم الكارت مطلوب")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "رقم الكارت لازم يكون 16 رقم بدون مسافات")]
        public string CardNumber { get; set; } = "";

        [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "الصيغة المطلوبة MM/YY")]
        public string ExpiryDate { get; set; } = "";

        [Required(ErrorMessage = "CVV مطلوب")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV لازم يكون 3 أرقام")]
        public string Cvv { get; set; } = "";

        public string? CouponCode { get; set; }
    }
}
