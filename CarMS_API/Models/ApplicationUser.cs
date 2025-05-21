using Microsoft.AspNetCore.Identity;

namespace CarMS_API.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
