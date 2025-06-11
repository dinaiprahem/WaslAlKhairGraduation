namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        public IOpportunityParticipationRepository OpportunityParticipation { get;  }
        public IDonationCategoryRepository DonationCategory { get; }
        public IDonationOpportunityRepository DonationOpportunity { get; }
        Task<bool> SaveAsync();
    }
}
