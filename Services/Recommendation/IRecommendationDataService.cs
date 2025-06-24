using WaslAlkhair.Api.MLModels;

namespace WaslAlkhair.Api.Services.Recommendation
{
    public interface IRecommendationDataService
    {
        Task<List<ModelInput>> GetPreparedTrainingDataAsync();
        Task<List<ModelInput>> GetPreparedVolunteeringTrainingDataAsync();
    }
} 