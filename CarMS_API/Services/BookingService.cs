using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<BookingService> _logger;

        public BookingService(ApplicationDbContext db, ILogger<BookingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> ExpireBookingsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var expiredBookings = await _db.Bookings
                .Include(r => r.Car)
                .Where(r => r.Status == BookingStatus.Pending && r.ExpiryAt < now)
                .ToListAsync(cancellationToken);

            foreach (var Booking in expiredBookings)
            {
                Booking.Status = BookingStatus.Expired;
                Booking.UpdatedAt = now;
                Booking.ExpiredAt = now;

                if (Booking.Car != null)
                {
                    Booking.Car.Status = Status.Available;
                }
            }

            if (expiredBookings.Any())
            {
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"[บริการการจอง] หมดอายุ {expiredBookings.Count} การจอง ที่ {now:u}");
            }

            return expiredBookings.Count;
        }
    }
}
