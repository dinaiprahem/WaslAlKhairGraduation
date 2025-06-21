using WaslAlkhair.Api.DTOs.Authentication;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.LostANDfound
{
    public class LostItemSearchResult
    {
        public Guid ItemId { get; set; }
        public string ImagePath { get; set; }
        public double Similarity { get; set; }
        public string Metadata { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public ItemType Type { get; set; }
        public string ContactInfo { get; set; }
        public bool IsResolved { get; set; }
        public UserDTO User { get; set; }
    }

}
