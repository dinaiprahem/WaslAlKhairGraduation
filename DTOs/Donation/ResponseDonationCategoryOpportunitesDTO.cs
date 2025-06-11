using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.Donation
{
    public class ResponseDonationCategoryOpportunitiesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<ResponseAllDonationOpportunities> DonationOpportunities { get; set; }
    }

    

}
