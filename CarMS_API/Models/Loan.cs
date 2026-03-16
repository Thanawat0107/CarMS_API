namespace CarMS_API.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public decimal DownPayment { get; set; } // เงินดาวน์
        public int InstallmentTerm { get; set; } // จำนวนเดือนที่ผ่อน
        public decimal MonthlyIncome { get; set; } // รายได้ต่อเดือนของผู้ซื้อ
        public string LoanStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
