namespace WaslAlkhair.Api.Services.Recommendation
{
    public interface IModelTrainingService
    {
        Task TrainDonationRecommenderModelAsync();
        Task TrainVolunteeringRecommenderModelAsync();
    }
} 