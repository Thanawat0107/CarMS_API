namespace CarMS_API.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDelete { get; set; }
    }
}
