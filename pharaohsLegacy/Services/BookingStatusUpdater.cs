using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    public class BookingStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusUpdater> _logger;

        // بيشتغل كل ساعة
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public BookingStatusUpdater(IServiceProvider serviceProvider, ILogger<BookingStatusUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingStatusUpdater started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateVisitedBookings();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task UpdateVisitedBookings()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var today = DateTime.Today;

                var expiredBookings = await context.Bookings
                    .Where(b => b.Status == "Confirmed"
                             && b.VisitDate < today)
                    .ToListAsync();

                if (expiredBookings.Any())
                {
                    foreach (var booking in expiredBookings)
                        booking.Status = "Visited";

                    await context.SaveChangesAsync();

                    _logger.LogInformation($"Updated {expiredBookings.Count} bookings to Visited.");

                    // 🔔 بند 17 — Achievements & Badges: بعد ما الحجوزات تتحول لـ Visited،
                    // نفحص Explorer Badge + Legendary لكل يوزر اتأثر (مرة واحدة لكل إيميل، مش
                    // لكل حجز، عشان مايتفحصش نفس اليوزر أكتر من مرة لو عنده أكتر من حجز في نفس الدورة)
                    // ⚠️ نفس قرار ShopOrderShippingBackgroundService بالظبط: الـ Background Service ده
                    // مالوش وصول لـ Session/LocalizationService (مفيش Request نعرف منه لغة اليوزر)،
                    // فالإشعار بيتسجل بالإنجليزي ثابت دايمًا هنا — مش قرار اختياري، تقني بحت.
                    var badgeService = scope.ServiceProvider.GetRequiredService<BadgeEvaluationService>();
                    var affectedUsers = expiredBookings.Select(b => b.UserEmail).Distinct();

                    foreach (var userEmail in affectedUsers)
                    {
                        try
                        {
                            var newBadges = await badgeService.EvaluateVisitAsync(userEmail);
                            // 🆕 فحص الشارات السرية المرتبطة بالحجوزات هنا كمان (Museum
                            // Completionist + Night Owl-booking) — قبل كده كانت بس بتتفحص لما
                            // اليوزر يفتح صفحة أسرة، يعني لو زار كل المتاحف بس معملش كده بعدها،
                            // الشارة تفضل متأخرة/معلّقة غلط
                            newBadges.AddRange(await badgeService.EvaluateHiddenAchievementsAsync(userEmail));
                            newBadges.AddRange(await badgeService.EvaluateLegendaryAsync(userEmail));
                            badgeService.NotifyNewBadges(userEmail, newBadges, "en");
                        }
                        catch (Exception badgeEx)
                        {
                            // Best effort مستقل لكل يوزر — فشل تقييم بادجات يوزر واحد مايوقفش الباقي
                            // ولا يوقف تحديث حالة الحجوزات نفسها (اللي خلص فوق خالص)
                            _logger.LogError(badgeEx, $"Badge evaluation failed for {userEmail} after Visited update.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingStatusUpdater.");
            }
        }
    }
}