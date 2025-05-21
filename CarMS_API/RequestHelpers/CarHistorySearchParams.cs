namespace CarMS_API.RequestHelpers
{
    public class CarHistorySearchParams
    {
        public int? CarId { get; set; }
        public string? Detail { get; set; }
        public string? Insurance { get; set; }
        public string? Act { get; set; }
        public string? Finance { get; set; }
        public string? Source { get; set; }

        public bool? IsCollisionHistory { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsUsed { get; set; }
        public bool? IsDeleted { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public string? SortBy { get; set; } = "createdAt";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
