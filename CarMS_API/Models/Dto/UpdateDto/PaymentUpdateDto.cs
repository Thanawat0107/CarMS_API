namespace CarMS_API.Models.Dto.UpdateDto
{
    public class PaymentUpdateDto
    {
        public double TotalPrice { get; set; }
        public string BookingStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string? TransactionRef { get; set; }
    }
}
