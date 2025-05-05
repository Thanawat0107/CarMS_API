namespace CarMS_API.Models.Responsts
{
    public class PaginationMeta
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int PageCount => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
