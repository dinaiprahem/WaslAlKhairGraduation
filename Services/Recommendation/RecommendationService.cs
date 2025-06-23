using WaslAlkhair.Api.DTOs.Donation;
using Microsoft.ML;
using System.Text.Json;
using WaslAlkhair.Api.MLModels;
using WaslAlkhair.Api.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.DTOs.Opportunity;

namespace WaslAlkhair.Api.Services.Recommendation
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<RecommendationService> _logger;
        private readonly string _modelArtifactsPath;
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;
        private readonly Dictionary<string, uint> _userMap;
        private readonly Dictionary<int, uint> _itemMap;

        private readonly PredictionEngine<ModelInput, ModelOutput> _volunteeringPredictionEngine;
        private readonly Dictionary<string, uint> _volunteeringUserMap;
        private readonly Dictionary<int, uint> _volunteeringItemMap;

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RecommendationService(IWebHostEnvironment hostingEnvironment, ILogger<RecommendationService> logger, AppDbContext context, IMapper mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _modelArtifactsPath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "MLModelArtifacts");

            // Load donation model
            var modelPath = Path.Combine(_modelArtifactsPath, "DonationRecommender.zip");
            if (File.Exists(modelPath))
            {
                var mlContext = new MLContext();
                var model = mlContext.Model.Load(modelPath, out _);
                _predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
                _logger.LogInformation("Donation recommendation model loaded successfully.");
            }
            else
            {
                _logger.LogWarning("Donation recommendation model not found at {Path}. The service will not be able to provide ML-based recommendations.", modelPath);
            }

            // Load mappings
            var userMapPath = Path.Combine(_modelArtifactsPath, "donation_user_map.json");
            if (File.Exists(userMapPath))
            {
                var userMapJson = File.ReadAllText(userMapPath);
                _userMap = JsonSerializer.Deserialize<Dictionary<string, uint>>(userMapJson);
            }
            else
            {
                _logger.LogWarning("User map not found. Cold start for all users.");
                _userMap = new Dictionary<string, uint>();
            }


            var itemMapPath = Path.Combine(_modelArtifactsPath, "donation_item_map.json");
            if(File.Exists(itemMapPath))
            {
                var itemMapJson = File.ReadAllText(itemMapPath);
                _itemMap = JsonSerializer.Deserialize<Dictionary<int, uint>>(itemMapJson);
            }
            else
            {
                _logger.LogWarning("Donation item map not found. Cold start for all items.");
                _itemMap = new Dictionary<int, uint>();
            }

            // Load volunteering model
            var volunteeringModelPath = Path.Combine(_modelArtifactsPath, "VolunteeringRecommender.zip");
            if (File.Exists(volunteeringModelPath))
            {
                var mlContext = new MLContext();
                var model = mlContext.Model.Load(volunteeringModelPath, out _);
                _volunteeringPredictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
                _logger.LogInformation("Volunteering recommendation model loaded successfully.");
            }
            else
            {
                _logger.LogWarning("Volunteering recommendation model not found at {Path}. The service will not be able to provide ML-based recommendations.", volunteeringModelPath);
            }

            // Load volunteering mappings
            var volunteeringUserMapPath = Path.Combine(_modelArtifactsPath, "volunteering_user_map.json");
            if (File.Exists(volunteeringUserMapPath))
            {
                var userMapJson = File.ReadAllText(volunteeringUserMapPath);
                _volunteeringUserMap = JsonSerializer.Deserialize<Dictionary<string, uint>>(userMapJson);
            }
            else
            {
                _logger.LogWarning("Volunteering user map not found. Cold start for all users.");
                _volunteeringUserMap = new Dictionary<string, uint>();
            }


            var volunteeringItemMapPath = Path.Combine(_modelArtifactsPath, "volunteering_item_map.json");
            if(File.Exists(volunteeringItemMapPath))
            {
                var itemMapJson = File.ReadAllText(volunteeringItemMapPath);
                _volunteeringItemMap = JsonSerializer.Deserialize<Dictionary<int, uint>>(itemMapJson);
            }
            else
            {
                _logger.LogWarning("Volunteering item map not found. Cold start for all items.");
                _volunteeringItemMap = new Dictionary<int, uint>();
            }
        }

        public async Task<List<ResponseAllDonationOpportunities>> GetRecommendedDonationOpportunitiesAsync(string userId, int count = 10)
        {
            if (_predictionEngine == null)
            {
                _logger.LogWarning("Prediction engine is not available. Cannot generate recommendations.");
                return new List<ResponseAllDonationOpportunities>();
            }

            // 1. Candidate Selection
            var userDonatedOpportunityIds = await _context.DonationDistributions
                .Where(dd => dd.Donation.DonorId == userId)
                .Select(dd => dd.DonationOpportunityId)
                .ToListAsync();

            var candidates = await _context.DonationOpportunities
                .AsNoTracking()
                .Where(opp => opp.Status == OpportunityStatus.Active && !userDonatedOpportunityIds.Contains(opp.Id))
                .ToListAsync();

            // 2. Scoring
            var scoredCandidates = new List<(DonationOpportunity opportunity, float score)>();

            if (!_userMap.TryGetValue(userId, out var userIdEncoded))
            {
                _logger.LogWarning("User with ID {UserId} not found in user map. Cannot provide personalized recommendations.", userId);
                // Fallback: return most popular items if user is new
                var popularItems = candidates.OrderByDescending(c => c.NumberOfDonors).Take(count);
                return _mapper.Map<List<ResponseAllDonationOpportunities>>(popularItems);
            }

            foreach (var candidate in candidates)
            {
                if (_itemMap.TryGetValue(candidate.Id, out var itemIdEncoded))
                {
                    var input = new ModelInput { UserIdEncoded = userIdEncoded, ItemIdEncoded = itemIdEncoded };
                    var prediction = _predictionEngine.Predict(input);

                    // 3. Ranking (Combine Scores)
                    // ML Score (weighted higher) + Heuristic Score
                    var hybridScore = (prediction.Score * 2.0f) + (float)Math.Log(1 + candidate.NumberOfDonors);

                    scoredCandidates.Add((candidate, hybridScore));
                }
            }

            // 4. Return Top N
            var recommendedOpportunities = scoredCandidates
                .OrderByDescending(sc => sc.score)
                .Take(count)
                .Select(sc => sc.opportunity)
                .ToList();

            return _mapper.Map<List<ResponseAllDonationOpportunities>>(recommendedOpportunities);
        }

        public async Task<List<OpportunityDto>> GetRecommendedVolunteeringOpportunitiesAsync(string userId, int count = 10)
        {
            if (_volunteeringPredictionEngine == null)
            {
                _logger.LogWarning("Volunteering prediction engine is not available. Cannot generate recommendations.");
                return new List<OpportunityDto>();
            }

            var userParticipatedOpportunityIds = await _context.OpportunityParticipations
                .Where(p => p.AppUserId == userId)
                .Select(p => p.OpportunityId)
                .ToListAsync();

            var candidates = await _context.Opportunities
                .AsNoTracking()
                .Where(opp => !opp.IsClosed && !userParticipatedOpportunityIds.Contains(opp.Id))
                .Include(opp => opp.Participants)
                .ToListAsync();

            var scoredCandidates = new List<(Opportunity opportunity, float score)>();

            if (!_volunteeringUserMap.TryGetValue(userId, out var userIdEncoded))
            {
                _logger.LogWarning("User with ID {UserId} not found in volunteering user map. Cannot provide personalized recommendations.", userId);
                var popularItems = candidates.OrderByDescending(c => c.Participants.Count).Take(count);
                return _mapper.Map<List<OpportunityDto>>(popularItems);
            }

            foreach (var candidate in candidates)
            {
                if (_volunteeringItemMap.TryGetValue(candidate.Id, out var itemIdEncoded))
                {
                    var input = new ModelInput { UserIdEncoded = userIdEncoded, ItemIdEncoded = itemIdEncoded };
                    var prediction = _volunteeringPredictionEngine.Predict(input);

                    var hybridScore = (prediction.Score * 2.0f) + (float)Math.Log(1 + candidate.Participants.Count);

                    scoredCandidates.Add((candidate, hybridScore));
                }
            }

            var recommendedOpportunities = scoredCandidates
                .OrderByDescending(sc => sc.score)
                .Take(count)
                .Select(sc => sc.opportunity)
                .ToList();

            return _mapper.Map<List<OpportunityDto>>(recommendedOpportunities);
        }

        public object GetDiagnostics(string userId = null)
        {
            return new
            {
                DonationModelLoaded = _predictionEngine != null,
                DonationUserMapCount = _userMap?.Count ?? 0,
                DonationItemMapCount = _itemMap?.Count ?? 0,
                DonationUserInMap = userId != null && _userMap?.ContainsKey(userId) == true,
                VolunteeringModelLoaded = _volunteeringPredictionEngine != null,
                VolunteeringUserMapCount = _volunteeringUserMap?.Count ?? 0,
                VolunteeringItemMapCount = _volunteeringItemMap?.Count ?? 0,
                VolunteeringUserInMap = userId != null && _volunteeringUserMap?.ContainsKey(userId) == true
            };
        }
    }
} 