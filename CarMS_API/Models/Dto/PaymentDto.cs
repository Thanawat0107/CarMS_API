namespace CarMS_API.Models.Dto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public BookingDto Booking { get; set; }
        public DateTime PaidAt { get; set; } //เวลาชำระ
        public double TotalPrice { get; set; } //ยอดเงินที่ชำระ
        public string PaymentMethod { get; set; } //วิธีจ่ายจริง
        public string PaymentStatus { get; set; }
        public string TransactionRef { get; set; } //เลขอ้างอิงธุรกรรม
    }
}
