namespace WaslAlkhair.Api.DTOs.LostANDfound
{
    public class StoredImage
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; } // Cloudinary URL
        public List<float> Embedding { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
