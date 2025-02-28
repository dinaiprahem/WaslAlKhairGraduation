namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        public IOpportunityParticipationRepository OpportunityParticipation { get;  }
        Task<bool> SaveAsync();
    }
}
