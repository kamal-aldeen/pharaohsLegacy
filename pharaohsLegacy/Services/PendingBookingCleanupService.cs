using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    /// <summary>
    /// لو اليوزر بدأ حجز (دوس "ابعت كود تحقق") وسابه معلق من غير ما يكمل الدفع
    /// (قفل التاب، اتلهى، الكود خلصت صلاحيته...)، الحجز ده بيفضل PendingPayment
    /// للأبد من غير الخدمة دي. زي أي موقع حجوزات حقيقي (طيران/فنادق) بيحرر
    /// "الحجز المؤقت" لو الدفع مكملش خلال وقت معين.
    /// </summary>
    public class PendingBookingCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

        public PendingBookingCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var cutoff = DateTime.Now - GracePeriod;

                    var staleBookings = await db.Bookings
                        .Where(b => b.Status == "PendingPayment" && b.CreatedAt < cutoff)
                        .ToListAsync(stoppingToken);

                    if (staleBookings.Count > 0)
                    {
                        // مفيش دفع اتخصم فعليًا لحجز لسه PendingPayment (الخصم بيحصل بس
                        // بعد نجاح /payments/charge)، فمفيش داعي لأي Refund هنا — نلغي بس
                        db.Bookings.RemoveRange(staleBookings);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }
    }
}
