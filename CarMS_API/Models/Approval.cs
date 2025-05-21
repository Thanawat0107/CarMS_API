namespace CarMS_API.Models
{
    public class Approval
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int CarHistoryId { get; set; }
        public CarHistory CarHistory { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string Remark { get; set; }
        public DateTime ApprovedAt { get; set; }
    }

    public enum ApprovalStatus { rejected, Approved }
}
