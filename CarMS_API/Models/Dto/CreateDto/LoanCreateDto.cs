namespace CarMS_API.Models.Dto.CreateDto
{
    public class LoanCreateDto
    {
        public string UserId { get; set; }
        public int CarId { get; set; }
        public decimal DownPayment { get; set; }
        public int InstallmentTerm { get; set; }
        public decimal MonthlyIncome { get; set; }
    }
}