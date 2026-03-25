using System.ComponentModel.DataAnnotations.Schema;

namespace CarMS_API.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        // 1. คนส่งข้อความ (ใส่ ? เพื่อให้เป็น Nullable)
        public string? SenderId { get; set; }

        [ForeignKey("SenderId")]
        public ApplicationUser? Sender { get; set; }

        // 2. คนรับข้อความ (ใส่ ? เพื่อให้เป็น Nullable)
        public string? ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public ApplicationUser? Receiver { get; set; }

        public string Message { get; set; } // แถม: ใส่ ? เพื่อลด Warning CS8618
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
