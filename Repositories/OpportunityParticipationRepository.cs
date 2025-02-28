using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
    public class OpportunityParticipationRepository : Repositery<OpportunityParticipation>, IOpportunityParticipationRepository
    {
        private readonly AppDbContext _context;

        public OpportunityParticipationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
