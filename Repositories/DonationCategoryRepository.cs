using System.Linq.Expressions;
using Google;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs.Donation;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;
using System.Threading.Tasks;

namespace WaslAlkhair.Api.Repositories
{
    public class DonationCategoryRepository : Repositery<DonationCategory> ,IDonationCategoryRepository
    {
        private readonly AppDbContext _context;

        public DonationCategoryRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }


        public async Task<ResponseDonationCategoryOpportunitiesDTO?> GetCategoryWithOpportunitiesAsync(Expression<Func<DonationCategory, bool>> filter = null)
        {
            return await _context.DonationCategories
                .Where(x => !x.IsDeleted)
                .Where(filter)
                .Select(c => new ResponseDonationCategoryOpportunitiesDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DonationOpportunities = c.DonationOpportunities
                    .Where(o => o.Status == OpportunityStatus.Active)
                    .Select(o => new ResponseAllDonationOpportunities
                    {
                        Id = o.Id,
                        Title = o.Title,
                        ImageUrl = o.ImageUrl
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

    }

}
