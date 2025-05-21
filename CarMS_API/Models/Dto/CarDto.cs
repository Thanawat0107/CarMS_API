namespace CarMS_API.Models.Dto
{
    public class CarDto
    {
        public int Id { get; set; }

        public int SellerId { get; set; }
        public SellerDto Seller { get; set; }
        public int BrandId { get; set; }
        public BrandDto Brand { get; set; }

        public string CarRegistrationNumber { get; set; }
        public string CarIdentificationNumber { get; set; }
        public string EngineNumber { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Price { get; set; }
        public int Mileage { get; set; }
        public string Color { get; set; }

        public EngineType EngineType { get; set; }
        public GearType GearType { get; set; }
        public CarType CarType { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Status Status { get; set; }
        public bool IsUsed { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
    }
}
