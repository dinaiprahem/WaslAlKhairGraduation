using Google;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;


        public IOpportunityParticipationRepository OpportunityParticipation { get; }
        public IDonationCategoryRepository DonationCategory { get; }
        public IDonationOpportunityRepository DonationOpportunity { get; }
        public UnitOfWork(AppDbContext context ,
            IOpportunityParticipationRepository opportunityParticipation,
            IDonationCategoryRepository DonationCategory,
            IDonationOpportunityRepository donationOpportunity)
        {
            _context = context;
            this.OpportunityParticipation = opportunityParticipation;
            this.DonationCategory = DonationCategory;
            this.DonationOpportunity = donationOpportunity;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
