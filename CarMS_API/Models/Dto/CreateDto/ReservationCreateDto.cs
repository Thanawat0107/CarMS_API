namespace CarMS_API.Models.Dto.CreateDto
{
    public class ReservationCreateDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string UserId { get; set; }
        public DateTime ReservedAt { get; set; } //เวลากดจอง
        public DateTime? ExpiryAt { get; set; } //หมดอายุการจอง (ถ้าไม่ชำระ)
        public ReservationStatus Status { get; set; }
    }
}
