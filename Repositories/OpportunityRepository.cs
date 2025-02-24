using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.DTOs.Opportunity;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories
{
    public class OpportunityRepository : IOpportunityRepository
    {
        private readonly AppDbContext _context;

        public OpportunityRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Opportunity>> SearchOpportunitiesAsync(OpportunitySearchParams searchParams)
        {
            IQueryable<Opportunity> query = _context.Opportunities
                .Include(o => o.CreatedBy)
                .Include(o => o.Participants);

            // Apply filters based on search parameters
            if (!string.IsNullOrWhiteSpace(searchParams.Location))
            {
                query = query.Where(o => o.Location.ToLower().Contains(searchParams.Location.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Type))
            {
                query = query.Where(o => o.Type == searchParams.Type);
            }

            if (searchParams.StartDate.HasValue)
            {
                var startDate = DateOnly.FromDateTime(searchParams.StartDate.Value);
                query = query.Where(o => o.StartDate >= startDate);

            }

            if (searchParams.EndDate.HasValue)
            {
                var endDate = DateOnly.FromDateTime(searchParams.EndDate.Value);
                query = query.Where(o => o.EndDate <= endDate);
            }

            if (searchParams.MinAge.HasValue)
            {
                query = query.Where(o => o.RequiredAge >= searchParams.MinAge);
            }

            if (searchParams.MaxSeats.HasValue)
            {
                query = query.Where(o => o.SeatsAvailable <= searchParams.MaxSeats);
                Console.WriteLine($"MaxSeats filter applied: SeatsAvailable <= {searchParams.MaxSeats}");
            }

            if (searchParams.IsOpen.HasValue)
            {
                query = query.Where(o => o.IsClosed == !searchParams.IsOpen);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                query = query.Where(o =>
                    o.Title.Contains(searchParams.SearchTerm) ||
                    o.Description.Contains(searchParams.SearchTerm) ||
                    o.Tasks.Contains(searchParams.SearchTerm)
                );
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Opportunity>> GetAllOpportunitiesAsync()
        {
            return await _context.Opportunities
                .Include(o => o.CreatedBy)
                .Include(o => o.Participants)
                .ToListAsync();
        }

        public async Task<Opportunity> GetOpportunityByIdAsync(int id)
        {
            return await _context.Opportunities
                .Include(o => o.CreatedBy)
                .Include(o => o.Participants)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Opportunity> CreateOpportunityAsync(Opportunity opportunity)
        {
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            return opportunity;
        }

        public async Task UpdateOpportunityAsync(Opportunity opportunity)
        {
            _context.Opportunities.Update(opportunity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOpportunityAsync(int id)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity != null)
            {
                _context.Opportunities.Remove(opportunity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}