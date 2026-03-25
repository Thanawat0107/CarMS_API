namespace CarMS_API.Models.Dto.UpdaeteDto
{
    public class CarUpdateDto
    {
        public int? SellerId { get; set; }
        public int? BrandId { get; set; }

        public string? CarRegistrationNumber { get; set; }
        public string? CarIdentificationNumber { get; set; }
        public string? EngineNumber { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public decimal? Price { get; set; }
        public decimal? BookingPrice { get; set; }
        public int? Mileage { get; set; }
        public string? Color { get; set; }

        public string EngineType { get; set; }
        public string GearType { get; set; }
        public string CarType { get; set; }

        public string? Description { get; set; }

        // merged from CarHistory
        public bool IsCollisionHistory { get; set; }
        public string? Insurance { get; set; }
        public string? Act { get; set; }

        // 1. ส่ง List ของ URL เดิมที่ยัง "อยากเก็บไว้" มา (กรณี User กดลบรูปเก่าบางรูปออก)
        public List<string>? KeepImages { get; set; }

        // 2. ส่งไฟล์ใหม่ที่ต้องการ "เพิ่มเข้าไป"
        public List<IFormFile>? NewImages { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CarStatus { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
