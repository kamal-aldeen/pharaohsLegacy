using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    /// <summary>
    /// 🆕 IHostedService بسيط بيشتغل جوه التطبيق نفسه (مش محتاج Hangfire أو أي حاجة خارجية).
    /// كل فترة (Interval) بيفحص الحجوزات اللي حالتها Cancelled من 24 ساعة أو أكتر،
    /// ويحولها فعليًا لـ Refunded عن طريق نداء البنك — بنفس منطق BookingStatusService
    /// بالظبط، عشان يفضل مفيش تضارب مع اللي الأدمن أو اليوزر بيعملوه يدويًا.
    /// </summary>
    public class BookingRefundBackgroundService : BackgroundService
    {
        // بعد قد إيه من الإلغاء نرجع الفلوس أوتوماتيك
        private static readonly TimeSpan RefundAfter = TimeSpan.FromHours(24);

        // كل قد إيه نفحص (مش لازم يكون Real-Time، كل 10 دقايق كفاية)
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingRefundBackgroundService> _logger;

        public BookingRefundBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingRefundBackgroundService> logger)
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
                    // أي فشل هنا (مثلاً البنك واقع مؤقتًا) منسيبوش يوقف الـ Job كله —
                    // نسجل اللوج ونجرب تاني في الدورة الجاية
                    _logger.LogError(ex, "BookingRefundBackgroundService: failed while processing automatic refunds");
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

            var dueBookings = await db.Bookings
                .Where(b => b.Status == "Cancelled"
                         && b.CancelledAt != null
                         && b.CancelledAt <= cutoff)
                .ToListAsync(stoppingToken);

            if (dueBookings.Count == 0)
                return;

            var statusService = new BookingStatusService(db, httpClientFactory);

            foreach (var booking in dueBookings)
            {
                var result = await statusService.ChangeStatusAsync(booking, "Refunded");

                if (result.Success)
                    _logger.LogInformation("Booking #{Id}: amount automatically refunded 24 hours after cancellation.", booking.Id);
                else
                    _logger.LogWarning("Booking #{Id}: automatic refund failed — {Message}", booking.Id, result.Message);
            }
        }
    }
}
