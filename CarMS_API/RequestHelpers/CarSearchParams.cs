using CarMS_API.Models;

namespace CarMS_API.RequestHelpers
{
    public class CarSearchParams
    {
        public string? SearchTerm { get; set; }

        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }

        public int? MinMileage { get; set; }
        public int? MaxMileage { get; set; }

        public string? Color { get; set; }

        public EngineType? EngineType { get; set; }
        public GearType? GearType { get; set; }
        public CarType? CarType { get; set; }

        public bool? IsUsed { get; set; }
        public Status? Status { get; set; }

        public int? SellerId { get; set; }
        public int? BrandId { get; set; }

        public string? SortBy { get; set; } = "id";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 4;
    }
}
