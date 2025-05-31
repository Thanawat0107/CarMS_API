namespace CarMS_API.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public DateTime PaidAt {  get; set; } //เวลาชำระ
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public double TotalPrice { get; set; } //ยอดเงินที่ชำระ
        public PaymentMethod Method { get; set; } //วิธีจ่ายจริง
        public PaymentStatus Status { get; set; }
        public string TransactionRef { get; set; } //เลขอ้างอิงธุรกรรม
    }

    public enum PaymentMethod 
    {
        QR,
        PromptPay,
        CreditCard,
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded,
    }
}
