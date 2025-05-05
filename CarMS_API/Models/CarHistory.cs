namespace CarMS_API.Models
{
    public class CarHistory
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string TentativelyCost { get; set; }
    }
}
