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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingStatusUpdater.");
            }
        }
    }
}