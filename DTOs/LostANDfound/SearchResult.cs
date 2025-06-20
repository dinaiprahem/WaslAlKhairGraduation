namespace WaslAlkhair.Api.DTOs.LostANDfound
{
    public class SearchResult
    {
        public Guid ImageId { get; set; }
        public string ImagePath { get; set; }
        public double Similarity { get; set; }
        public string Metadata { get; set; }
    }

}
