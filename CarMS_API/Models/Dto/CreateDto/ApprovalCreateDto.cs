namespace CarMS_API.Models.Dto.CreateDto
{
    // ใช้ส่งคำสั่งอนุมัติ/ปฏิเสธรถ (Approval ถูกรวมเข้า Car แล้ว)
    public class ApprovalCreateDto
    {
        public int CarId { get; set; }
        public bool IsApproved { get; set; }
        public string? Remark { get; set; }
    }
}
