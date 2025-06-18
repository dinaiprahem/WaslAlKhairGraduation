using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.DTOs.HomeSearch;

namespace WaslAlkhair.Api.Services
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAsync(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<string, string[]> _categoryKeywords;

        public SearchService(AppDbContext context)
        {
            _context = context;
            // Define keywords for each category in both English and Arabic
            _categoryKeywords = new Dictionary<string, string[]>
            {
                ["Opportunity"] = new[] { "opportunity", "opportunities", "فرصة", "فرص", "فرص تطوع", "تطوع" },
                ["Donation"] = new[] { "donation", "donations", "تبرع", "تبرعات", "تبرع خيري" },
                ["Assistance"] = new[] { "assistance", "help", "مساعدة", "مساعدات", "عون", "معونة" },
                ["AssistanceType"] = new[] { "assistance type", "نوع مساعدة", "أنواع المساعدات" },
                ["Charity"] = new[] { "charity", "charities", "جمعية", "جمعيات", "خيرية", "خيري" },
                ["DonationCategory"] = new[] { "donation category", "فئة تبرع", "فئات التبرعات" },
                ["User"] = new[] { "user", "users", "مستخدم", "مستخدمين" }
            };
        }

        public async Task<SearchResultDto> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new SearchResultDto();

            var searchTerm = query.ToLower();
            var result = new SearchResultDto();

            // Check if the search term matches any category keywords
            var matchedCategories = _categoryKeywords
                .Where(kvp => kvp.Value.Any(keyword => searchTerm.Contains(keyword.ToLower())))
                .Select(kvp => kvp.Key)
                .ToList();

            // If no specific category is matched, search in all fields
            if (!matchedCategories.Any())
            {
                // Search in Opportunities
                result.Opportunities = await _context.Opportunities
                    .Where(o => o.Title.ToLower().Contains(searchTerm) || 
                               o.Description.ToLower().Contains(searchTerm) ||
                               o.Location.ToLower().Contains(searchTerm) ||
                               o.Type.ToLower().Contains(searchTerm))
                    .Select(o => new SearchItemDto
                    {
                        Id = o.Id,
                        Title = o.Title,
                        Description = o.Description,
                        Type = "Opportunity",
                        CreatedAt = DateTime.UtcNow,
                        ImageUrl = o.PhotoUrl
                    })
                    .ToListAsync();

                // Search in Donations
                result.Donations = await _context.DonationOpportunities
                    .Where(d => d.Title.ToLower().Contains(searchTerm) || 
                               d.Description.ToLower().Contains(searchTerm))
                    .Select(d => new SearchItemDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Description = d.Description,
                        Type = "Donation",
                        CreatedAt = d.CreatedAt,
                        ImageUrl = d.ImageUrl
                    })
                    .ToListAsync();

                // Search in Assistances
                result.Assistances = await _context.Assistances
                    .Where(a => a.Title.ToLower().Contains(searchTerm) || 
                               a.Description.ToLower().Contains(searchTerm) ||
                               a.ContactInfo.ToLower().Contains(searchTerm))
                    .Select(a => new SearchItemDto
                    {
                        Id = a.Id.GetHashCode(),
                        Title = a.Title,
                        Description = a.Description,
                        Type = "Assistance",
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                // Search in AssistanceTypes
                result.AssistanceTypes = await _context.AssistanceTypes
                    .Where(at => at.Name.ToLower().Contains(searchTerm))
                    .Select(at => new SearchItemDto
                    {
                        Id = at.Id.GetHashCode(),
                        Title = at.Name,
                        Description = $"Assistance Type: {at.Name}",
                        Type = "AssistanceType",
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToListAsync();

                // Search in Charities
                result.Charities = await _context.Users
                    .Where(u => u.CharityRegistrationNumber != null && 
                               (u.FullName.ToLower().Contains(searchTerm) ||
                                u.CharityMission.ToLower().Contains(searchTerm) ||
                                u.Address.ToLower().Contains(searchTerm) ||
                                u.CharityRegistrationNumber.ToLower().Contains(searchTerm)))
                    .Select(u => new SearchItemDto
                    {
                        Id = u.Id.GetHashCode(),
                        Title = u.FullName,
                        Description = u.CharityMission ?? "No mission statement available",
                        Type = "Charity",
                        CreatedAt = u.CreatedAt,
                        ImageUrl = u.image
                    })
                    .ToListAsync();

                // Search in DonationCategories
                result.DonationCategories = await _context.DonationCategories
                    .Where(dc => dc.Name.ToLower().Contains(searchTerm) ||
                                dc.Description.ToLower().Contains(searchTerm))
                    .Select(dc => new SearchItemDto
                    {
                        Id = dc.Id,
                        Title = dc.Name,
                        Description = dc.Description,
                        Type = "DonationCategory",
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToListAsync();

                // Search in Users
                result.Users = await _context.Users
                    .Where(u => u.CharityRegistrationNumber == null && 
                               u.FullName.ToLower().Contains(searchTerm))
                    .Select(u => new SearchItemDto
                    {
                        Id = u.Id.GetHashCode(),
                        Title = u.FullName,
                        Description = $"User: {u.Email}",
                        Type = "User",
                        CreatedAt = u.CreatedAt,
                        ImageUrl = u.image
                    })
                    .ToListAsync();
            }
            else
            {
                // If category is matched, return all items of that type
                foreach (var category in matchedCategories)
                {
                    switch (category)
                    {
                        case "Opportunity":
                            result.Opportunities = await _context.Opportunities
                                .Select(o => new SearchItemDto
                                {
                                    Id = o.Id,
                                    Title = o.Title,
                                    Description = o.Description,
                                    Type = "Opportunity",
                                    CreatedAt = DateTime.UtcNow,
                                    ImageUrl = o.PhotoUrl
                                })
                                .ToListAsync();
                            break;

                        case "Donation":
                            result.Donations = await _context.DonationOpportunities
                                .Select(d => new SearchItemDto
                                {
                                    Id = d.Id,
                                    Title = d.Title,
                                    Description = d.Description,
                                    Type = "Donation",
                                    CreatedAt = d.CreatedAt,
                                    ImageUrl = d.ImageUrl
                                })
                                .ToListAsync();
                            break;

                        case "Assistance":
                            result.Assistances = await _context.Assistances
                                .Select(a => new SearchItemDto
                                {
                                    Id = a.Id.GetHashCode(),
                                    Title = a.Title,
                                    Description = a.Description,
                                    Type = "Assistance",
                                    CreatedAt = a.CreatedAt
                                })
                                .ToListAsync();
                            break;

                        case "AssistanceType":
                            result.AssistanceTypes = await _context.AssistanceTypes
                                .Select(at => new SearchItemDto
                                {
                                    Id = at.Id.GetHashCode(),
                                    Title = at.Name,
                                    Description = $"Assistance Type: {at.Name}",
                                    Type = "AssistanceType",
                                    CreatedAt = DateTime.UtcNow
                                })
                                .ToListAsync();
                            break;

                        case "Charity":
                            result.Charities = await _context.Users
                                .Where(u => u.CharityRegistrationNumber != null)
                                .Select(u => new SearchItemDto
                                {
                                    Id = u.Id.GetHashCode(),
                                    Title = u.FullName,
                                    Description = u.CharityMission ?? "No mission statement available",
                                    Type = "Charity",
                                    CreatedAt = u.CreatedAt,
                                    ImageUrl = u.image
                                })
                                .ToListAsync();
                            break;

                        case "DonationCategory":
                            result.DonationCategories = await _context.DonationCategories
                                .Select(dc => new SearchItemDto
                                {
                                    Id = dc.Id,
                                    Title = dc.Name,
                                    Description = dc.Description,
                                    Type = "DonationCategory",
                                    CreatedAt = DateTime.UtcNow
                                })
                                .ToListAsync();
                            break;

                        case "User":
                            result.Users = await _context.Users
                                .Where(u => u.CharityRegistrationNumber == null)
                                .Select(u => new SearchItemDto
                                {
                                    Id = u.Id.GetHashCode(),
                                    Title = u.FullName,
                                    Description = $"User: {u.Email}",
                                    Type = "User",
                                    CreatedAt = u.CreatedAt,
                                    ImageUrl = u.image
                                })
                                .ToListAsync();
                            break;
                    }
                }
            }

            return result;
        }
    }
} 