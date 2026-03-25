namespace CarMS_API.Models.Dto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public BookingDto Booking { get; set; }
        public DateTime? PaidAt { get; set; }      //เวลาชำระ (nullable — อาจยังไม่จ่าย)
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal TotalPrice { get; set; }    //ยอดเงินที่ชำระ
        public string SlipCarImages { get; set; }  //รูปสลิปโอนเงิน
        public string PaymentMethod { get; set; }  //วิธีจ่ายจริง
        public string PaymentStatus { get; set; }
        public string TransactionRef { get; set; } //เลขอ้างอิงธุรกรรม
    }
}
