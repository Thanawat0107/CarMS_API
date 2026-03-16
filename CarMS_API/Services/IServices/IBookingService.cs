namespace CarMS_API.Services.IServices
{
    public interface IBookingService
    {
        Task<int> ExpireBookingsAsync(CancellationToken cancellationToken = default);
    }
}