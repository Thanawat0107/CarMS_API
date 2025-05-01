namespace CarMS_API.Models.Dto
{
    public class CarDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string Price { get; set; }
        public string Mileage { get; set; }
        public string Color { get; set; }
        public string EngineType { get; set; }
        public string GearType { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }

        public CarType CarType { get; set; }
        public string SellerId { get; set; }
    }
}
