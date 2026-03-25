namespace CarMS_API.Models.Dto.UpdateDto
{
    public class PaymentUpdateDto
    {
        public decimal TotalPrice { get; set; }
        public DateTime? PaidAt { get; set; }
        public string PaymentStatus { get; set; }  //แก้จาก BookingStatus → PaymentStatus
        public string PaymentMethod { get; set; }
        public string? TransactionRef { get; set; }
        public IFormFile? SlipImage { get; set; }  //อัปเดตสลิปโอนเงิน
    }
}
