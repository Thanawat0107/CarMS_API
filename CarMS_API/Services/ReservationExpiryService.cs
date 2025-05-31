using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Services.IServices;
using CarMS_API.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;

namespace CarMS_API.Services
{
    public class ReservationExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationExpiryService> _logger;
        private readonly ReservationSettings _settings;

        public ReservationExpiryService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReservationExpiryService> logger,
            IOptions<ReservationSettings> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _settings = options.Value;
        }

        private bool _isRunning = false;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_isRunning)
                {
                    _logger.LogWarning("การจองล่วงหน้ายังคงหมดอายุอยู่");
                }
                else
                {
                    _isRunning = true;

                    using var scope = _scopeFactory.CreateScope();
                    var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                    try
                    {
                        var expiredCount = await Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(
                                3,
                                attempt => TimeSpan.FromSeconds(10),
                                (ex, ts, retry, ctx) => _logger.LogWarning($"Retry {retry} after {ts.TotalSeconds}s due to: {ex.Message}")
                            )
                            .ExecuteAsync(() => reservationService.ExpireReservationsAsync(stoppingToken));

                        if (expiredCount > 0)
                        {
                            _logger.LogInformation($"[บริการการหมดอายุการจอง] หมดอายุแล้ว {expiredCount} การจอง ที่ {DateTime.UtcNow:u}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ความล้มเหลวครั้งสุดท้ายหลังจากการลองใหม่ใน บริการการหมดอายุการจอง");
                    }

                    _isRunning = false;
                }

                await Task.Delay(TimeSpan.FromMinutes(_settings.ReservationCleanupIntervalMinutes), stoppingToken);
            }
        }
    }
}
