namespace CarMS_API.Models.Dto.UpdateDto
{
    public class PaymentUpdateDto
    {
        public double TotalPrice { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionRef { get; set; }
    }
}
