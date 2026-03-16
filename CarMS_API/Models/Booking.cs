namespace CarMS_API.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime ReservedAt { get; set; } //เวลากดจอง
        public DateTime ExpiryAt { get; set; } //หมดอายุการจอง (ถ้าไม่ชำระ)
        public DateTime? ExpiredAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string BookingStatus { get; set; }
    }
}