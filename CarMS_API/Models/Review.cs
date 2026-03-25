namespace CarMS_API.Models
{
    public class Review
    {
        public int Id { get; set; }

        // 1. คนที่เขียนรีวิว (ใส่ ? เพื่อตัดวงจร Cascade)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // 2. คนที่ถูกรีวิว (ใส่ ? เพื่อตัดวงจร Cascade)
        public int? SellerId { get; set; } // เปลี่ยนจาก int เป็น int?
        public Seller? Seller { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; } // แถม: ใส่ ? เพื่อลด Warning
        public DateTime CreatedAt { get; set; }
    }
}
