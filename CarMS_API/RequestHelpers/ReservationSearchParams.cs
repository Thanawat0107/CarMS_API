using CarMS_API.Models;

namespace CarMS_API.RequestHelpers
{
    public class ReservationSearchParams
    {
        public string? UserName { get; set; }
        public string? CarSearchTerm { get; set; }
        public ReservationStatus? Status { get; set; }
        public DateTime? ReservedAtFrom { get; set; }
        public DateTime? ReservedAtTo { get; set; }
        public DateTime? ExpiryAtFrom { get; set; }
        public DateTime? ExpiryAtTo { get; set; }

        public string? SortBy { get; set; } = "reservedAt";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
