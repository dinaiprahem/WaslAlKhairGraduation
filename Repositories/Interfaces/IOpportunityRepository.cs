using System.Collections.Generic;
using System.Threading.Tasks;
using WaslAlkhair.Api.DTOs.Opportunity;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories
{
    public interface IOpportunityRepository
    {
        Task<IEnumerable<Opportunity>> GetAllOpportunitiesAsync();
        Task<Opportunity> GetOpportunityByIdAsync(int id);
        Task<Opportunity> CreateOpportunityAsync(Opportunity opportunity);
        Task UpdateOpportunityAsync(Opportunity opportunity);
        Task DeleteOpportunityAsync(int id);
        Task<bool> SaveChangesAsync();
        Task<IEnumerable<Opportunity>> SearchOpportunitiesAsync(OpportunitySearchParams searchParams);
    }
}