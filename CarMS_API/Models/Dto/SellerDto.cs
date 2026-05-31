namespace CarMS_API.Models.Dto
{
    public class SellerDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public UserDto User { get; set; }
        public string IdentityNumber { get; set; }
        public string Address { get; set; }
        public bool IsVerified { get; set; }
        public string? MeetingUrl { get; set; }
    }
}
