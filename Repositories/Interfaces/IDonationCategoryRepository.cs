using System.Linq.Expressions;
using WaslAlkhair.Api.DTOs.Donation;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IDonationCategoryRepository : IRepositery<DonationCategory>
    {
        Task<ResponseDonationCategoryOpportunitiesDTO?> GetCategoryWithOpportunitiesAsync(Expression<Func<DonationCategory, bool>> filter = null);
    }
}
