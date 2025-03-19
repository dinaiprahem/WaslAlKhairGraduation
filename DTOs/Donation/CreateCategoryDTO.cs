namespace WaslAlkhair.Api.DTOs.Donation
{
    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ImageUrl { get; set; }
    }
}
