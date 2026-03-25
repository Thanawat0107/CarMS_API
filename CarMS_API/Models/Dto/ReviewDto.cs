namespace CarMS_API.Models.Dto
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public UserDto? User { get; set; } // แสดงชื่อ/รูปคนรีวิว
        public int? SellerId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}