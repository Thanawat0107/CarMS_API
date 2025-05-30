namespace CarMS_API.Models.Dto.UpdaeteDto
{
    public class BrandUpdateDto
    {
        public string? Name { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDelete { get; set; }
    }
}
