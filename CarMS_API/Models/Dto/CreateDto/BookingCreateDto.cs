namespace CarMS_API.Models.Dto.CreateDto
{
    public class BookingCreateDto
    {
        public int CarId { get; set; }
        public string UserId { get; set; }
        // ReservedAt, ExpiryAt, BookingStatus ถูก set โดย server
    }
}
