using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(ApplicationDbContext db, ILogger<ReservationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> ExpireReservationsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var expiredReservations = await _db.Reservations
                .Include(r => r.Car)
                .Where(r => r.Status == ReservationStatus.Pending && r.ExpiryAt < now)
                .ToListAsync(cancellationToken);

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = ReservationStatus.Expired;
                reservation.UpdatedAt = now;

                if (reservation.Car != null)
                {
                    reservation.Car.Status = Status.Available;
                }
            }

            if (expiredReservations.Any())
            {
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"[บริการการจอง] หมดอายุ {expiredReservations.Count} การจอง ที่ {now:u}");
            }

            return expiredReservations.Count;
        }
    }
}
