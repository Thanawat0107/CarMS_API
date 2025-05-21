namespace CarMS_API.Models
{
    public class CarHistory
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public bool IsCollisionHistory { get; set; }
        public string Detail { get; set; }
        public string Insurance { get; set; }
        public string Act { get; set; }
        public string Finance { get; set; }
        public string Source { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsApproved { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
