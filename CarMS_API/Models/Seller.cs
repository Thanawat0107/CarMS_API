namespace CarMS_API.Models
{
    public class Seller
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string IdentityNumber {  get; set; }
        public string Address { get; set; }
        public bool IsVerified { get; set; }
    }
}
