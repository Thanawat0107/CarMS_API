namespace CarMS_API.Models
{
    public class TestDrive
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime CreatedAt { get; set; } // เพิ่มเวลาที่ทำการจองเข้ามา
        public string StatusTestDrive { get; set; }
    }
}