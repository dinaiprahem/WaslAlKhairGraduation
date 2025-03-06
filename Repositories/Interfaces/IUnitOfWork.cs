namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        public IOpportunityParticipationRepository OpportunityParticipation { get;  }
		public IAssistanceRepository AssistanceRepository { get; }
		public IAssistanceTypeRepository AssistanceTypeRepository { get; }
		Task<bool> SaveAsync();
    }
}
