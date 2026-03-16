namespace CarMS_API.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        public DateTime? PaidAt { get; set; } // เปลี่ยนเป็น nullable เพราะอาจจะยังไม่จ่ายทันทีที่สร้าง record
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal TotalPrice { get; set; } // ปรับเป็น decimal
        public string SlipImageUrl { get; set; } // เพิ่มช่องเก็บสลิปโอนเงิน
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionRef { get; set; }
    }
}
