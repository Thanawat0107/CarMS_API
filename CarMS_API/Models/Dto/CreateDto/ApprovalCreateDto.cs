namespace CarMS_API.Models.Dto.CreateDto
{
    public class ApprovalCreateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CarHistoryId { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string? Remark { get; set; }
        public DateTime ApprovedAt { get; set; }
    }
}
