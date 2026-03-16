namespace CarMS_API.Models.Dto
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public CarDto Car { get; set; }
        public string UserId { get; set; }
        public UserDto User { get; set; }
        public DateTime ReservedAt { get; set; } //เวลากดจอง
        public DateTime ExpiryAt { get; set; } //หมดอายุการจอง (ถ้าไม่ชำระ)
        public string BookingStatus { get; set; }
    }
}
