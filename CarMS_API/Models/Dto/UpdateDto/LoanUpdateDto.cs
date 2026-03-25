namespace CarMS_API.Models.Dto.UpdateDto
{
    public class LoanUpdateDto
    {
        // ผู้ขายมักจะอัปเดตแค่สถานะ เช่น เปลี่ยนจาก Pending -> Contacted (ติดต่อแล้ว)
        public string LoanStatus { get; set; } 
    }
}