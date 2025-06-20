namespace WaslAlkhair.Api.Models
{
    public class ImageEntity
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public string Embedding { get; set; } // JSON string
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
