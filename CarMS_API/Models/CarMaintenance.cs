namespace CarMS_API.Models
{
    public class CarMaintenance
    {
        public int Id { get; set; }
        public int? CarHistoryId { get; set; }
        public CarHistory? CarHistory { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ServiceDate {  get; set; }
        public double TentativelyCost { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
