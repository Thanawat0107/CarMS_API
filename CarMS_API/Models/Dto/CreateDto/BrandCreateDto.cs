namespace CarMS_API.Models.Dto.CreateDto
{
    public class BrandCreateDto
    {
        public string? Name { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool IsUsed { get; set; }
    }
}