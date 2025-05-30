namespace CarMS_API.Models.Dto
{
    public class UserDto
    {
        public string Id { get; set; }              // จาก IdentityUser.Id
        public string UserName { get; set; }        // จาก IdentityUser
        public string FullName { get; set; }        // จาก ApplicationUser
        public string Email { get; set; }           // จาก IdentityUser
        public string PhoneNumber { get; set; }     // จาก IdentityUser
        public DateTime CreatedAt { get; set; }     // จาก ApplicationUser
    }
}
