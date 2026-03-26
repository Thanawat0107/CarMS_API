namespace CarMS_API.Models.Dto.UpdaeteDto
{
    public class CarMaintenanceUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal? TentativelyCost { get; set; }
    }
}
