using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Services.IServices;
using CarMS_API.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;

namespace CarMS_API.Services
{
    public class BookingExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingExpiryService> _logger;
        private readonly BookingSettings _settings;

        public BookingExpiryService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingExpiryService> logger,
            IOptions<BookingSettings> options)
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
                    var BookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    try
                    {
                        var expiredCount = await Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(
                                3,
                                attempt => TimeSpan.FromSeconds(10),
                                (ex, ts, retry, ctx) => _logger.LogWarning($"Retry {retry} after {ts.TotalSeconds}s due to: {ex.Message}")
                            )
                            .ExecuteAsync(() => BookingService.ExpireBookingsAsync(stoppingToken));

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

                await Task.Delay(TimeSpan.FromMinutes(_settings.BookingCleanupIntervalMinutes), stoppingToken);
            }
        }
    }
}
