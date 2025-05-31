namespace CarMS_API.Models.Dto.CreateDto
{
    public class PaymentCreateDto
    {
        public int ReservationId { get; set; }
        public double TotalPrice { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionRef { get; set; }
    }
}
