namespace CarMS_API.Models.Dto.CreateDto
{
    public class CarMaintenanceCreateDto
    {
        public int Id { get; set; }
        public int? CarId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal? TentativelyCost { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
