namespace CarMS_API.Models.Dto.CreateDto
{
    public class TestDriveCreateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CarId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
