namespace CarMS_API.RequestHelpers
{
    public class BrandSearchParams
    {
        public string? SearchTerm { get; set; }
        public bool? IsUsed { get; set; }
        public string? SortBy { get; set; } = "id";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}