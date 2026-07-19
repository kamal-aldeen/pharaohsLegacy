using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    /// <summary>
    /// 🆕 IHostedService بيحدّث ShippingStatus أوتوماتيك على مرحلتين، بنفس فلسفة
    /// ShopOrderRefundBackgroundService بالظبط:
    ///
    /// 1) Processing → Shipped: بعد 48 ساعة من ConfirmedAt (لحظة نجاح الدفع الفعلية).
    /// 2) Shipped → Delivered: بعد عدد أيام يعتمد على محافظة الأوردر (Governorates.GetDeliveryDays)،
    ///    محسوبة من لحظة ShippedAt فعليًا (سواء اتشحن أوتوماتيك هنا أو الأدمن غيّرها يدوي).
    ///
    /// منفصل تمامًا عن Status (حالة الدفع) — منقدرش نشحن أوردر لسه PendingPayment أو اتلغى/اترفند.
    /// الأدمن يقدر يفضل يغيّر ShippingStatus يدويًا في أي وقت (UpdateShopOrderShipping) —
    /// السيرفيس ده بيشتغل على فلاتر واضحة فمنقدرش نرجّع حالة اتغيرت يدوي بالغلط.
    /// </summary>
    public class ShopOrderShippingBackgroundService : BackgroundService
    {
        // بعد قد إيه من التأكيد نحول الأوردر لـ Shipped أوتوماتيك
        private static readonly TimeSpan ShipAfter = TimeSpan.FromHours(48);

        // كل قد إيه نفحص (مش لازم يكون Real-Time، كل 10 دقايق كفاية)
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShopOrderShippingBackgroundService> _logger;

        public ShopOrderShippingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ShopOrderShippingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessShippingTransitionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // أي فشل هنا منسيبوش يوقف الـ Job كله — نسجل اللوج ونجرب تاني في الدورة الجاية
                    _logger.LogError(ex, "ShopOrderShippingBackgroundService: failed while processing automatic shipping transitions");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ProcessShippingTransitionsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ProcessDueShipmentsAsync(db, stoppingToken);
            await ProcessDueDeliveriesAsync(db, stoppingToken);
        }

        // المرحلة 1: Processing → Shipped بعد 48 ساعة من تأكيد الدفع
        private async Task ProcessDueShipmentsAsync(AppDbContext db, CancellationToken stoppingToken)
        {
            var cutoff = DateTime.Now - ShipAfter;

            var dueOrders = await db.ShopOrders
                .Where(o => o.Status == "Confirmed"
                         && o.ShippingStatus == "Processing"
                         && o.ConfirmedAt != null
                         && o.ConfirmedAt <= cutoff)
                .ToListAsync(stoppingToken);

            if (dueOrders.Count == 0)
                return;

            foreach (var order in dueOrders)
            {
                order.ShippingStatus = "Shipped";
                order.ShippedAt = DateTime.Now;
                _logger.LogInformation("ShopOrder #{Id}: automatically marked as Shipped 48 hours after confirmation.", order.Id);
            }

            await db.SaveChangesAsync(stoppingToken);
        }

        // المرحلة 2: Shipped → Delivered بعد عدد أيام حسب المحافظة
        private async Task ProcessDueDeliveriesAsync(AppDbContext db, CancellationToken stoppingToken)
        {
            var shippedOrders = await db.ShopOrders
                .Where(o => o.Status == "Confirmed"
                         && o.ShippingStatus == "Shipped"
                         && o.ShippedAt != null)
                .ToListAsync(stoppingToken);

            if (shippedOrders.Count == 0)
                return;

            // 🆕 عدد الأيام بيختلف لكل محافظة (Governorates.GetDeliveryDays)، فمش ممكن نعمله في الـ Where
            // فوق بشكل مباشر — بنجيب المرشحين الشحن الأول ونفلترهم في الميموري
            var dueOrders = shippedOrders
                .Where(o => o.ShippedAt!.Value.AddDays(Governorates.GetDeliveryDays(o.Governorate)) <= DateTime.Now)
                .ToList();

            if (dueOrders.Count == 0)
                return;

            foreach (var order in dueOrders)
            {
                order.ShippingStatus = "Delivered";
                order.DeliveredAt = DateTime.Now;
                _logger.LogInformation("ShopOrder #{Id}: automatically marked as Delivered ({Days} day(s) after shipping to {Gov}).",
                    order.Id, Governorates.GetDeliveryDays(order.Governorate), order.Governorate);
            }

            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
