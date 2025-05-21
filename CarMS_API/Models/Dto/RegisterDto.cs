namespace CarMS_API.Models.Dto
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; }
    }
}
