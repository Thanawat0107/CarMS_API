namespace CarMS_API.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; }
        public int SellerId { get; set; }
        public Seller Seller { get; set; }
        public int Rating { get; set; } // คะแนน 1-5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
