using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.Donation
{
    public class CreateDonationOpportunityDTO
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public IFormFile ImageUrl { get; set; }



        public int CategoryId { get; set; }
        public string CharityId { get; set; }
    }
}
