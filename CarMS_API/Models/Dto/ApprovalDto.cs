namespace CarMS_API.Models.Dto
{
    public class ApprovalDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CarHistoryId { get; set; }
        public CarHistoryDto CarHistory { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string Remark { get; set; }
        public DateTime ApprovedAt { get; set; }
    }
}
