namespace CarMS_API.Models.Dto
{
    public class CarDto
    {
        public int Id { get; set; }

        public int? SellerId { get; set; }
        public SellerDto? Seller { get; set; }
        public int? BrandId { get; set; }
        public BrandDto? Brand { get; set; }

        public string CarRegistrationNumber { get; set; }
        public string CarIdentificationNumber { get; set; }
        public string EngineNumber { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public decimal BookingPrice { get; set; }
        public int Mileage { get; set; }
        public string Color { get; set; }

        public string EngineType { get; set; }
        public string GearType { get; set; }
        public string CarType { get; set; }

        public string Description { get; set; }

        // merged from CarHistory
        public bool IsCollisionHistory { get; set; }
        public string Insurance { get; set; }
        public string Act { get; set; }

        // merged from Approval
        public string ApprovalRemark { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public List<string> CarImages { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CarStatus { get; set; }
        public bool IsUsed { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
    }
}
