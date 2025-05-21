using CarMS_API.Models;

namespace CarMS_API.RequestHelpers
{
    public class TestDriveSearchParams
    {
        public string? UserId { get; set; }
        public int? CarId { get; set; }

        public DateTime? AppointmentFrom { get; set; }
        public DateTime? AppointmentTo { get; set; }

        public StatusTestDrive? Status { get; set; }

        public string? SortBy { get; set; } = "appointmentDate";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
