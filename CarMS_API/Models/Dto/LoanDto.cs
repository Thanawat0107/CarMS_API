namespace CarMS_API.Models.Dto
{
    public class LoanDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public UserDto? User { get; set; } // ข้อมูลผู้ขอกู้
        public int CarId { get; set; }
        public CarDto? Car { get; set; } // ข้อมูลรถที่จะจัดไฟแนนซ์
        public decimal DownPayment { get; set; } 
        public int InstallmentTerm { get; set; } 
        public decimal MonthlyIncome { get; set; } 
        public string LoanStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}