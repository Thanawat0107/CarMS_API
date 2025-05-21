namespace CarMS_API.RequestHelpers
{
    public class CarMaintenanceSearchParams
    {
        public int? CarHistoryId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public double? MinCost { get; set; }
        public double? MaxCost { get; set; }

        public DateTime? ServiceDateFrom { get; set; }
        public DateTime? ServiceDateTo { get; set; }

        public bool? IsUsed { get; set; }

        public string? SortBy { get; set; } = "serviceDate";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
