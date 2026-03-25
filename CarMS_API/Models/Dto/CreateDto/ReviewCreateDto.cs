namespace CarMS_API.Models.Dto.CreateDto
{
    public class ReviewCreateDto
    {
        public string? UserId { get; set; }
        public int? SellerId { get; set; }
        public int Rating { get; set; } // ควรดัก Validate หน้าเว็บให้ส่งมาแค่ 1-5
        public string? Comment { get; set; }
    }
}