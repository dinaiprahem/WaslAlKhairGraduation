using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.MLModels;

namespace WaslAlkhair.Api.Services.Recommendation
{
    public class RecommendationDataService : IRecommendationDataService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<RecommendationDataService> _logger;
        private readonly string _modelArtifactsPath;

        public RecommendationDataService(AppDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<RecommendationDataService> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _modelArtifactsPath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "MLModelArtifacts");
            if (!Directory.Exists(_modelArtifactsPath))
            {
                Directory.CreateDirectory(_modelArtifactsPath);
            }
        }

        public async Task<List<ModelInput>> GetPreparedTrainingDataAsync()
        {
            _logger.LogInformation("Starting data preparation for recommendation model training.");

            var userActivities = await _context.DonationDistributions
                .AsNoTracking()
                .Include(d => d.Donation)
                .Where(d => d.Donation.DonorId != null)
                .ToListAsync();

            var modelInputs = new List<ModelInput>();
            var userMap = new Dictionary<string, uint>();
            var itemMap = new Dictionary<int, uint>();
            uint userIndex = 0;
            uint itemIndex = 0;

            foreach (var activity in userActivities)
            {
                if (!userMap.ContainsKey(activity.Donation.DonorId))
                {
                    userMap[activity.Donation.DonorId] = userIndex++;
                }

                if (!itemMap.ContainsKey(activity.DonationOpportunityId))
                {
                    itemMap[activity.DonationOpportunityId] = itemIndex++;
                }

                modelInputs.Add(new ModelInput
                {
                    UserIdEncoded = userMap[activity.Donation.DonorId],
                    ItemIdEncoded = itemMap[activity.DonationOpportunityId],
                    Rating = 5.0f // Strong positive signal for a donation
                });
            }

            _logger.LogInformation("Found {Count} user activities to use for training.", modelInputs.Count);

            // Persist the mappings
            var userMapPath = Path.Combine(_modelArtifactsPath, "donation_user_map.json");
            var itemMapPath = Path.Combine(_modelArtifactsPath, "donation_item_map.json");

            var userMapJson = JsonSerializer.Serialize(userMap);
            await File.WriteAllTextAsync(userMapPath, userMapJson);
            _logger.LogInformation("Saved user ID mapping to {Path}", userMapPath);

            var itemMapJson = JsonSerializer.Serialize(itemMap);
            await File.WriteAllTextAsync(itemMapPath, itemMapJson);
            _logger.LogInformation("Saved item ID mapping to {Path}", itemMapPath);

            return modelInputs;
        }

        public async Task<List<ModelInput>> GetPreparedVolunteeringTrainingDataAsync()
        {
            _logger.LogInformation("Starting data preparation for volunteering recommendation model training.");

            var userActivities = await _context.OpportunityParticipations
                .AsNoTracking()
                .Where(p => p.AppUserId != null)
                .ToListAsync();

            var modelInputs = new List<ModelInput>();
            var userMap = new Dictionary<string, uint>();
            var itemMap = new Dictionary<int, uint>();
            uint userIndex = 0;
            uint itemIndex = 0;

            foreach (var activity in userActivities)
            {
                if (!userMap.ContainsKey(activity.AppUserId))
                {
                    userMap[activity.AppUserId] = userIndex++;
                }

                if (!itemMap.ContainsKey(activity.OpportunityId))
                {
                    itemMap[activity.OpportunityId] = itemIndex++;
                }

                modelInputs.Add(new ModelInput
                {
                    UserIdEncoded = userMap[activity.AppUserId],
                    ItemIdEncoded = itemMap[activity.OpportunityId],
                    Rating = 5.0f // Strong positive signal for participation
                });
            }

            _logger.LogInformation("Found {Count} user participation activities to use for training.", modelInputs.Count);

            var userMapPath = Path.Combine(_modelArtifactsPath, "volunteering_user_map.json");
            var itemMapPath = Path.Combine(_modelArtifactsPath, "volunteering_item_map.json");

            var userMapJson = JsonSerializer.Serialize(userMap);
            await File.WriteAllTextAsync(userMapPath, userMapJson);
            _logger.LogInformation("Saved volunteering user ID mapping to {Path}", userMapPath);

            var itemMapJson = JsonSerializer.Serialize(itemMap);
            await File.WriteAllTextAsync(itemMapPath, itemMapJson);
            _logger.LogInformation("Saved volunteering item ID mapping to {Path}", itemMapPath);

            return modelInputs;
        }
    }
} 