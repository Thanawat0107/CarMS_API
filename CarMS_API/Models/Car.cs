namespace CarMS_API.Models
{
    public class Car
    {
        public int Id { get; set; }

        public int? SellerId { get; set; }
        public Seller? Seller { get; set; }
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }

        public string CarRegistrationNumber { get; set; }
        public string CarIdentificationNumber { get; set; }
        public string EngineNumber { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public decimal BookingPrice { get; set; }
        public int Mileage { get; set; }
        public string Color { get; set; }

        // --- ปรับเป็น string แล้ว ---
        public string EngineType { get; set; }
        public string GearType { get; set; }
        public string CarType { get; set; }

        public string Description { get; set; }

        // --- ส่วนที่ดึงมาจาก CarHistory เดิม ---
        public bool IsCollisionHistory { get; set; }
        public string Insurance { get; set; }
        public string Act { get; set; }

        // --- ส่วนที่ดึงมาจาก Approval เดิม ---
        public string ApprovalRemark { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CarStatus { get; set; } // ใช้เก็บค่าจาก SD.Status_...
        public bool IsUsed { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }

        public string CarImages { get; set; }
    }
}
