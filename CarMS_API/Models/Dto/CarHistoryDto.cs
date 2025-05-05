namespace CarMS_API.Models.Dto
{
    public class CarHistoryDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public CarDto Car { get; set; }

        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public string? TentativelyCost { get; set; }
    }
}
