namespace CarMS_API.Models.Dto
{
    public class TestDriveDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public UserDto User { get; set; }
        public int CarId { get; set; }
        public CarDto Car { get; set; }
        public DateTime AppointmentDate { get; set; }
        public StatusTestDrive StatusTestDrive { get; set; }
    }
}
