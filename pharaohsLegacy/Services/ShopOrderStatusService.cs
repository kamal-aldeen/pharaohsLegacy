using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using System.Net.Http.Json;

namespace pharaohsLegacy.Services
{
    /// <summary>
    /// 🆕 نفس فكرة BookingStatusService بالظبط — المصدر الوحيد للحقيقة بالنسبة لحالة الدفع
    /// في ShopOrder.Status. أي مكان عايز يغيّر حالة أوردر (اليوزر، الأدمن، الـ Background Job)
    /// لازم يمر من هنا.
    ///
    /// القواعد (مطابقة لـ BookingStatusService):
    /// - Confirmed → Cancelled : تسجيل CancelledAt بس، بدون نداء بنك (الفلوس لسه معلقة)
    ///                           — مسموح بس طالما ShippingStatus لسه "Processing" (قبل الشحن)
    /// - Cancelled → Refunded  : نداء /payments/refund فعليًا (تلقائي بعد 24 ساعة أو يدوي من الأدمن)
    /// - Confirmed → Refunded  : نداء /payments/refund مباشرة (تخطي مرحلة Cancelled) — للأدمن بس
    /// - Cancelled → Confirmed : "تراجع عن الإلغاء" — بدون نداء بنك (الفلوس أصلاً ما رجعتش)
    /// - Refunded → أي حاجة    : ممنوع، حالة نهائية (Terminal State)
    ///
    /// ملحوظة: ده منفصل تمامًا عن ShippingStatus (Processing/Shipped/Delivered) — تحديث الشحن
    /// بيتعمل مباشرة من الأدمن (تحديث حقل عادي)، مش من خلال السيرفيس ده.
    /// </summary>
    public class ShopOrderStatusService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopOrderStatusService(AppDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        public class TransitionResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
        }

        public async Task<TransitionResult> ChangeStatusAsync(ShopOrder order, string newStatus)
        {
            var currentStatus = order.Status;

            if (currentStatus == newStatus)
                return new TransitionResult { Success = true, Message = "Status unchanged." };

            if (currentStatus == "Refunded")
                return new TransitionResult { Success = false, Message = "This order has already been refunded — its status cannot be changed." };

            switch (newStatus)
            {
                case "Cancelled":
                    return await ToCancelledAsync(order, currentStatus);

                case "Refunded":
                    return await ToRefundedAsync(order, currentStatus);

                case "Confirmed":
                    return await ToConfirmedAsync(order, currentStatus);

                default:
                    return new TransitionResult { Success = false, Message = $"Unknown status: {newStatus}" };
            }
        }

        private async Task<TransitionResult> ToCancelledAsync(ShopOrder order, string currentStatus)
        {
            // بس أوردر Confirmed يقدر يتلغي
            if (currentStatus != "Confirmed")
                return new TransitionResult { Success = false, Message = $"Cannot cancel an order with current status '{currentStatus}'." };

            // 🆕 القاعدة اللي طلبها اليوزر: طالما الطلب طلع للشحن (Shipped/Delivered)
            // منقدرش نلغيه — الإلغاء متاح بس والطلب لسه Processing
            if (order.ShippingStatus != "Processing")
                return new TransitionResult { Success = false, Message = "This order cannot be cancelled — it has already been shipped." };

            // 🆕 مفيش نداء بنك هنا خالص — الفلوس بتفضل معلقة لحد ما الـ 24 ساعة تخلص
            // (أو الأدمن يعمل Refund يدوي فوري) — بنفس منطق الحجز بالظبط
            order.Status = "Cancelled";
            order.CancelledAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return new TransitionResult { Success = true, Message = "Order cancelled. The amount will be automatically refunded within 24 hours." };
        }

        private async Task<TransitionResult> ToRefundedAsync(ShopOrder order, string currentStatus)
        {
            if (currentStatus != "Confirmed" && currentStatus != "Cancelled")
                return new TransitionResult { Success = false, Message = $"Cannot refund an order with current status '{currentStatus}'." };

            var client = _httpClientFactory.CreateClient("BankService");
            var refundResponse = await client.PostAsJsonAsync("payments/refund", new
            {
                user_email = order.UserEmail,
                related_type = "ShopOrder",
                related_id = order.Id.ToString()
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

            order.Status = "Refunded";
            if (order.CancelledAt == null)
                order.CancelledAt = DateTime.Now; // للتوثيق فقط لو اتعمل Refund مباشر بدون مرور بـ Cancelled

            var payment = await _db.ShopPayments.FirstOrDefaultAsync(p => p.ShopOrderId == order.Id);
            if (payment != null) payment.Status = "Refunded";

            // 🆕 لازم نرجّع المخزون اللي اتخصم وقت الدفع — عكس اللي حصل في Confirm
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
            foreach (var item in order.Items)
            {
                if (products.TryGetValue(item.ProductId, out var p))
                    p.StockQuantity += item.Quantity;
            }

            await _db.SaveChangesAsync();

            return new TransitionResult
            {
                Success = true,
                Message = $"Successfully refunded {refund?.refunded_amount:N2} EGP."
            };
        }

        private async Task<TransitionResult> ToConfirmedAsync(ShopOrder order, string currentStatus)
        {
            if (currentStatus != "Cancelled")
                return new TransitionResult
                {
                    Success = false,
                    Message = "This order cannot be confirmed — it must be in 'Cancelled (pending refund)' status."
                };

            order.Status = "Confirmed";
            order.CancelledAt = null; // بيلغي عملية الريفند المجدولة (الـ Background Job بيعتمد على الحقل ده)

            var payment = await _db.ShopPayments.FirstOrDefaultAsync(p => p.ShopOrderId == order.Id);
            if (payment != null) payment.Status = "Completed";

            await _db.SaveChangesAsync();

            return new TransitionResult { Success = true, Message = "Cancellation reverted — the order is confirmed again." };
        }
    }
}
