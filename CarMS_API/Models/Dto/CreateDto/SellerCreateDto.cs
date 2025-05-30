namespace CarMS_API.Models.Dto.CreateDto
{
    public class SellerCreateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string IdentityNumber { get; set; }
        public string Address { get; set; }
        public bool IsVerified { get; set; }
    }
}
