using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Services
{
    public class ReservationExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationExpiryService> _logger;

        public ReservationExpiryService(IServiceScopeFactory scopeFactory, ILogger<ReservationExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                try
                {
                    var expiredCount = await reservationService.ExpireReservationsAsync(stoppingToken);
                    if (expiredCount > 0)
                    {
                        _logger.LogInformation($"[บริการการหมดอายุการจอง] หมดอายุ {expiredCount} การจอง ที่ {DateTime.UtcNow:u}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ReservationExpiryService");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
