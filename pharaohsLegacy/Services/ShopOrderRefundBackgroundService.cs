using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    /// <summary>
    /// 🆕 نفس فكرة BookingRefundBackgroundService بالظبط بس للشوب — بيفحص كل فترة الأوردرات
    /// اللي حالتها Cancelled من 24 ساعة أو أكتر ويحولها لـ Refunded فعليًا عن طريق البنك.
    /// </summary>
    public class ShopOrderRefundBackgroundService : BackgroundService
    {
        private static readonly TimeSpan RefundAfter = TimeSpan.FromHours(24);
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShopOrderRefundBackgroundService> _logger;

        public ShopOrderRefundBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ShopOrderRefundBackgroundService> logger)
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
                    await ProcessDueRefundsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ShopOrderRefundBackgroundService: failed while processing automatic refunds");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ProcessDueRefundsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var cutoff = DateTime.Now - RefundAfter;

            // 🆕 لازم Include(Items) هنا — ShopOrderStatusService.ToRefundedAsync محتاجهم عشان يرجّع المخزون
            var dueOrders = await db.ShopOrders
                .Include(o => o.Items)
                .Where(o => o.Status == "Cancelled"
                         && o.CancelledAt != null
                         && o.CancelledAt <= cutoff)
                .ToListAsync(stoppingToken);

            if (dueOrders.Count == 0)
                return;

            var statusService = new ShopOrderStatusService(db, httpClientFactory);

            foreach (var order in dueOrders)
            {
                var result = await statusService.ChangeStatusAsync(order, "Refunded");

                if (result.Success)
                    _logger.LogInformation("ShopOrder #{Id}: amount automatically refunded 24 hours after cancellation.", order.Id);
                else
                    _logger.LogWarning("ShopOrder #{Id}: automatic refund failed — {Message}", order.Id, result.Message);
            }
        }
    }
}
