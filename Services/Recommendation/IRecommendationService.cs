using WaslAlkhair.Api.DTOs.Donation;
using WaslAlkhair.Api.DTOs.Opportunity;

namespace WaslAlkhair.Api.Services.Recommendation
{
    public interface IRecommendationService
    {
        Task<List<ResponseAllDonationOpportunities>> GetRecommendedDonationOpportunitiesAsync(string userId, int count = 10);
        Task<List<OpportunityDto>> GetRecommendedVolunteeringOpportunitiesAsync(string userId, int count = 10);
        object GetDiagnostics(string userId = null);
    }
} 