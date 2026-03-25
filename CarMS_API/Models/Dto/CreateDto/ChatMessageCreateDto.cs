namespace CarMS_API.Models.Dto.CreateDto
{
    public class ChatMessageCreateDto
    {
        public string? SenderId { get; set; }   // รหัสคนที่กำลังพิมพ์ส่ง
        public string? ReceiverId { get; set; } // รหัสคนที่ต้องการส่งไปหา
        public string? Message { get; set; }    // เนื้อหาข้อความ
    }
}