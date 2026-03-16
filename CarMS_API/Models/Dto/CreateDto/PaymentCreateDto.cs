namespace CarMS_API.Models.Dto.CreateDto
{
    public class PaymentCreateDto
    {
        public int BookingId { get; set; }
        public double TotalPrice { get; set; }
        public string PaymentMethod { get; set; }
        public string? TransactionRef { get; set; }
    }
}
