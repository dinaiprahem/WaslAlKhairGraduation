using Microsoft.AspNetCore.Mvc;
using WaslAlkhair.Api.Services.Recommendation;

namespace WaslAlkhair.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IModelTrainingService _modelTrainingService;
        private readonly ILogger<RecommendationController> _logger;
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IModelTrainingService modelTrainingService, ILogger<RecommendationController> logger, IRecommendationService recommendationService)
        {
            _modelTrainingService = modelTrainingService;
            _logger = logger;
            _recommendationService = recommendationService;
        }

       [HttpPost("train-model")]
public async Task<IActionResult> TrainModel()
{
    try
    {
        _logger.LogInformation("Manual model training triggered via API.");
        await _modelTrainingService.TrainDonationRecommenderModelAsync();
        return Ok("Model training started successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred during model training: {Message}", ex.Message);
        // Return the full error for debugging (remove in production!)
        return StatusCode(500, ex.ToString());
    }
}

        [HttpGet("donations")]
        public async Task<IActionResult> GetDonationRecommendations()
        {
            try
            {
                // Note: In a real app, you would get the user ID from the authenticated context (e.g., HttpContext.User).
                // For this example, we'll use a hardcoded user ID.
                var userId = "a1b2c3d4-e5f6-g7h8-i9j0-k1l2m3n4o5p6"; // Replace with a real user ID from your database for testing.

                var recommendations = await _recommendationService.GetRecommendedDonationOpportunitiesAsync(userId);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting recommendations.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("volunteering")]
        public async Task<IActionResult> GetVolunteeringRecommendations()
        {
            try
            {
                var userId = "a1b2c3d4-e5f6-g7h8-i9j0-k1l2m3n4o5p6"; // Replace with a real user ID

                var recommendations = await _recommendationService.GetRecommendedVolunteeringOpportunitiesAsync(userId);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting volunteering recommendations.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("personalized")]
        public async Task<IActionResult> GetPersonalizedRecommendations()
        {
            try
            {
                var userId = "a1b2c3d4-e5f6-g7h8-i9j0-k1l2m3n4o5p6"; // Replace with a real user ID

                var donationTask = _recommendationService.GetRecommendedDonationOpportunitiesAsync(userId);
                var volunteeringTask = _recommendationService.GetRecommendedVolunteeringOpportunitiesAsync(userId);

                await Task.WhenAll(donationTask, volunteeringTask);

                var result = new
                {
                    Donations = await donationTask,
                    Volunteering = await volunteeringTask
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting personalized recommendations.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("diagnostics")]
        public IActionResult GetRecommendationDiagnostics()
        {
            var diagnostics = _recommendationService.GetDiagnostics();
            return Ok(diagnostics);
        }
    }
} 