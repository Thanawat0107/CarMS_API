namespace CarMS_API.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; } // เพิ่มบรรทัดนี้
        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; } // เพิ่มบรรทัดนี้
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
