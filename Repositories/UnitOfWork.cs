using Google;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

		public IAssistanceRepository AssistanceRepository { get; }
		public IOpportunityParticipationRepository OpportunityParticipation { get; }
		public IAssistanceTypeRepository AssistanceTypeRepository { get;}
		public UnitOfWork(AppDbContext context ,
            IOpportunityParticipationRepository opportunityParticipation,
			IAssistanceRepository AssistanceRepository,
			IAssistanceTypeRepository AssistanceTypeRepository)
		{
            _context = context;
            this.OpportunityParticipation = opportunityParticipation;
            this.AssistanceRepository = AssistanceRepository;
            this.AssistanceTypeRepository = AssistanceTypeRepository;
		
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
