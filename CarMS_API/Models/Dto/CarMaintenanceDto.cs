namespace CarMS_API.Models.Dto
{
    public class CarMaintenanceDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public CarDto Car { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal TentativelyCost { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
