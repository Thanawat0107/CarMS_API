using CarMS_API.Models;

namespace CarMS_API.RequestHelpers
{
    public class ApprovalSearchParams
    {
        public string? UserId { get; set; }
        public int? CarHistoryId { get; set; }
        public string? Remark { get; set; }
        public ApprovalStatus? ApprovalStatus { get; set; }

        public DateTime? ApprovedFrom { get; set; }
        public DateTime? ApprovedTo { get; set; }

        public string? SortBy { get; set; } = "approvedAt";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
