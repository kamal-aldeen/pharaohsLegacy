using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using System.Net.Http.Json;

namespace pharaohsLegacy.Services
{
    /// <summary>
    /// 🆕 المصدر الوحيد للحقيقة بالنسبة لحالات الحجز (Booking.Status).
    /// أي مكان في المشروع عايز يغيّر حالة حجز (الأدمن، اليوزر، الـ Background Job)
    /// لازم يمر من هنا — عشان نضمن إن مفيش تضارب بين اللي بيتسجل في قاعدة البيانات المحلية
    /// واللي فعليًا بيحصل في البنك (Wallet Balance الحقيقي).
    ///
    /// القواعد:
    /// - Confirmed → Cancelled : تسجيل CancelledAt بس، بدون أي نداء للبنك (الفلوس لسه معلقة)
    /// - Cancelled → Refunded  : نداء /payments/refund فعليًا (تلقائي بعد 24 ساعة أو يدوي من الأدمن)
    /// - Confirmed → Refunded  : نداء /payments/refund فعليًا مباشرة (تخطي مرحلة Cancelled)
    /// - Cancelled → Confirmed : "تراجع عن الإلغاء" — بدون نداء بنك (الفلوس أصلاً ما رجعتش)
    /// - Refunded → أي حاجة    : ممنوع تمامًا، حالة نهائية (Terminal State)
    /// - أي حاجة → Confirmed إلا من Cancelled : ممنوع، وممنوع كمان لو VisitDate فات
    /// </summary>
    public class BookingStatusService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookingStatusService(AppDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        public class TransitionResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
        }

        /// <summary>
        /// نقطة الدخول الوحيدة لتغيير حالة حجز. بترجع Success=false + رسالة واضحة
        /// لو الانتقال مش مسموح أو لو نداء البنك فشل — من غير ما تلمس قاعدة البيانات
        /// في أي حالة فشل (عشان الحالة المحلية تفضل متطابقة مع البنك دايمًا).
        /// </summary>
        public async Task<TransitionResult> ChangeStatusAsync(Booking booking, string newStatus)
        {
            var currentStatus = booking.Status;

            // مفيش أي تغيير فعلي — نرجع نجاح "وهمي" بدون ما نعمل حاجة
            if (currentStatus == newStatus)
                return new TransitionResult { Success = true, Message = "Status unchanged." };

            // Refunded حالة نهائية — منقدرش نتحرك منها لحاجة تانية خالص
            if (currentStatus == "Refunded")
                return new TransitionResult { Success = false, Message = "This booking has already been refunded — its status cannot be changed." };

            switch (newStatus)
            {
                case "Cancelled":
                    return await ToCancelledAsync(booking, currentStatus);

                case "Refunded":
                    return await ToRefundedAsync(booking, currentStatus);

                case "Confirmed":
                    return await ToConfirmedAsync(booking, currentStatus);

                case "Visited":
                    return ToVisited(booking, currentStatus);

                default:
                    return new TransitionResult { Success = false, Message = $"Unknown status: {newStatus}" };
            }
        }

        private async Task<TransitionResult> ToCancelledAsync(Booking booking, string currentStatus)
        {
            // بس حجز Confirmed (أو Visited لو حابب تسمح بيها) يقدر يتلغي
            if (currentStatus != "Confirmed")
                return new TransitionResult { Success = false, Message = $"Cannot cancel a booking with current status '{currentStatus}'." };

            // 🆕 منع إلغاء حجز معاده فات فعليًا — ده ممكن يحصل لو اليوزر لغى
            // في الفترة اللي بين ما الـ VisitDate يعدي وبين ما BookingStatusUpdater
            // (اللي بيشتغل كل ساعة) يلحق يحوله لـ Visited
            if (booking.VisitDate.Date < DateTime.Today)
                return new TransitionResult { Success = false, Message = "This booking cannot be cancelled — the visit date has already passed." };

            // 🆕 مفيش نداء بنك هنا خالص — الفلوس بتفضل معلقة لحد ما الـ 24 ساعة تخلص
            // (أو الأدمن يعمل Refund يدوي فوري)
            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return new TransitionResult { Success = true, Message = "Booking cancelled. The amount will be automatically refunded within 24 hours." };
        }

        private async Task<TransitionResult> ToRefundedAsync(Booking booking, string currentStatus)
        {
            // بس Confirmed أو Cancelled يقدروا يتحولوا لـ Refunded
            if (currentStatus != "Confirmed" && currentStatus != "Cancelled")
                return new TransitionResult { Success = false, Message = $"Cannot refund a booking with current status '{currentStatus}'." };

            var client = _httpClientFactory.CreateClient("BankService");
            var refundResponse = await client.PostAsJsonAsync("payments/refund", new
            {
                user_email = booking.UserEmail,
                related_type = "Booking",
                related_id = booking.Id.ToString()
            });

            if (!refundResponse.IsSuccessStatusCode)
            {
                var error = await refundResponse.Content.ReadFromJsonAsync<BankErrorResult>();
                return new TransitionResult
                {
                    Success = false,
                    Message = error?.detail ?? "Failed to refund the amount from the bank — status was not updated."
                };
            }

            var refund = await refundResponse.Content.ReadFromJsonAsync<RefundResult>();

            booking.Status = "Refunded";
            if (booking.CancelledAt == null)
                booking.CancelledAt = DateTime.Now; // للتوثيق فقط لو اتعمل Refund مباشر بدون مرور بـ Cancelled

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.BookingId == booking.Id);
            if (payment != null) payment.Status = "Refunded";

            await _db.SaveChangesAsync();

            return new TransitionResult
            {
                Success = true,
                Message = $"Successfully refunded {refund?.refunded_amount:N2} EGP."
            };
        }

        private async Task<TransitionResult> ToConfirmedAsync(Booking booking, string currentStatus)
        {
            // 🆕 الحالة الوحيدة المسموحة للرجوع لـ Confirmed هي "التراجع عن الإلغاء"
            // (Cancelled لسه ما اتحولش Refunded) — الفلوس أصلاً ما رجعتش، فمفيش داعي لنداء بنك
            if (currentStatus != "Cancelled")
                return new TransitionResult
                {
                    Success = false,
                    Message = "This booking cannot be confirmed — it must be in 'Cancelled (pending refund)' status."
                };

            // منطقي إننا منسمحش بتأكيد حجز معاده فات
            if (booking.VisitDate.Date < DateTime.Today)
                return new TransitionResult
                {
                    Success = false,
                    Message = "This booking cannot be confirmed — the visit date has already passed."
                };

            booking.Status = "Confirmed";
            booking.CancelledAt = null; // بيلغي عملية الريفند المجدولة (الـ Background Job بيعتمد على الحقل ده)

            // 🆕 لو الحجز اتلغى قبل ما يتولد له توكن (حالة نادرة)، أو من أي سبب لسه TicketToken فاضي،
            // نولده هنا كمان. مش بس أول مرة (Confirm الأساسي في BookingController).
            if (booking.TicketToken == null)
                booking.TicketToken = Guid.NewGuid();

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.BookingId == booking.Id);
            if (payment != null) payment.Status = "Completed";

            await _db.SaveChangesAsync();

            return new TransitionResult { Success = true, Message = "Cancellation reverted — the booking is confirmed again." };
        }

        private TransitionResult ToVisited(Booking booking, string currentStatus)
        {
            if (currentStatus != "Confirmed")
                return new TransitionResult { Success = false, Message = $"Cannot mark a booking with status '{currentStatus}' as Visited." };

            if (booking.VisitDate.Date > DateTime.Today)
                return new TransitionResult { Success = false, Message = "Cannot mark this booking as Visited before the visit date." };

            booking.Status = "Visited";
            _db.SaveChanges();
            return new TransitionResult { Success = true, Message = "Booking marked as Visited." };
        }
    }
}
