namespace CarMS_API.Models.Dto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public ReservationDto Reservation { get; set; }
        public DateTime PaidAt { get; set; } //เวลาชำระ
        public double TotalPrice { get; set; } //ยอดเงินที่ชำระ
        public PaymentMethod Method { get; set; } //วิธีจ่ายจริง
        public PaymentStatus Status { get; set; }
        public string TransactionRef { get; set; } //เลขอ้างอิงธุรกรรม
    }
}
