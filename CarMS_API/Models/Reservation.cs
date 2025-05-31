namespace CarMS_API.Models
{
    public class Reservation
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
        public ReservationStatus Status { get; set; }
    }

    public enum ReservationStatus
    {
        Pending, //จองแล้วแต่ยังไม่ได้สร้าง payment
        PendingPayment, // จองแล้วและมี payment แต่ยังไม่ได้จ่าย
        Confirmed, // จ่ายแล้ว
        Expired, // หมดเวลาจอง
        Canceled,
    } 
}