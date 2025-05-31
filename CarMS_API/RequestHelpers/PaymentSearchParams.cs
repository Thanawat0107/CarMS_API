using CarMS_API.Models;

namespace CarMS_API.RequestHelpers
{
    public class PaymentSearchParams
    {
        public string? UserName { get; set; } // จาก Reservation.User.UserName
        public string? TransactionRef { get; set; }

        public PaymentMethod? Method { get; set; }
        public PaymentStatus? Status { get; set; }

        public DateTime? PaidAtFrom { get; set; }
        public DateTime? PaidAtTo { get; set; }

        public double? MinTotal { get; set; }
        public double? MaxTotal { get; set; }

        public string? SortBy { get; set; } = "paidat";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
