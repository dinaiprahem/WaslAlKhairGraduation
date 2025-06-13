using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.DTOs;

namespace WaslAlkhair.Api.Services
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAsync(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;

        public SearchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResultDto> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new SearchResultDto();

            var searchTerm = query.ToLower();

            var opportunities = await _context.Opportunities
                .Where(o => o.Title.ToLower().Contains(searchTerm) || 
                           o.Description.ToLower().Contains(searchTerm))
                .Select(o => new SearchItemDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description,
                    Type = "Opportunity",
                    CreatedAt = DateTime.UtcNow // Opportunity doesn't have CreatedAt
                })
                .ToListAsync();

            var donations = await _context.DonationOpportunities
                .Where(d => d.Title.ToLower().Contains(searchTerm) || 
                           d.Description.ToLower().Contains(searchTerm))
                .Select(d => new SearchItemDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    Type = "Donation",
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            var assistance = await _context.Assistances
                .Where(a => a.Title.ToLower().Contains(searchTerm) || 
                           a.Description.ToLower().Contains(searchTerm))
                .Select(a => new SearchItemDto
                {
                    Id = a.Id.GetHashCode(), // Convert Guid to int for consistency
                    Title = a.Title,
                    Description = a.Description,
                    Type = "Assistance",
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            var assistanceTypes = await _context.AssistanceTypes
                .Where(at => at.Name.ToLower().Contains(searchTerm))
                .Select(at => new SearchItemDto
                {
                    Id = at.Id.GetHashCode(), // Convert Guid to int for consistency
                    Title = at.Name,
                    Description = $"Assistance Type: {at.Name}",
                    Type = "AssistanceType",
                    CreatedAt = DateTime.UtcNow // AssistanceType doesn't have CreatedAt
                })
                .ToListAsync();

            return new SearchResultDto
            {
                Opportunities = opportunities,
                Donations = donations,
                Assistances = assistance,
                AssistanceTypes = assistanceTypes
            };
        }
    }
} 