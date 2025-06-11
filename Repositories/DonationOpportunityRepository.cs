using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
    public class DonationOpportunityRepository : Repositery<DonationOpportunity>, IDonationOpportunityRepository
    {
        private readonly AppDbContext _db;

        public DonationOpportunityRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

    
      
    }
}