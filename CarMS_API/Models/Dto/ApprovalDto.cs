namespace CarMS_API.Models.Dto
{
    // Approval ถูกรวมเข้า Car แล้ว — Dto นี้ใช้แสดงสถานะการอนุมัติของรถ
    public class ApprovalDto
    {
        public int CarId { get; set; }
        public CarDto? Car { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovalRemark { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
