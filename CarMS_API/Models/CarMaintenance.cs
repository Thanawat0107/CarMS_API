namespace CarMS_API.Models
{
    public class CarMaintenance
    {
        public int Id { get; set; }
        public int CarId { get; set; } // เปลี่ยนจาก CarHistoryId เป็น CarId
        public Car Car { get; set; }   // เพิ่ม Navigation Property
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal TentativelyCost { get; set; } // ปรับเป็น decimal
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
