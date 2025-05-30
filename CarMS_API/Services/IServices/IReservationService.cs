namespace CarMS_API.Services.IServices
{
    public interface IReservationService
    {
        Task<int> ExpireReservationsAsync(CancellationToken cancellationToken = default);
    }
}