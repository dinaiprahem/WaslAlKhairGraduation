using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.Donation
{
    public class CreateDonationOpportunityDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; } // instead of CategoryId
        public decimal? TargetAmount { get; set; }
        public DateTime? Deadline { get; set; }
        public IFormFile ImageUrl { get; set; } // optional
    }
}
