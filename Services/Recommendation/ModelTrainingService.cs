using Microsoft.ML;
using Microsoft.ML.Trainers;
using WaslAlkhair.Api.MLModels;

namespace WaslAlkhair.Api.Services.Recommendation
{
    public class ModelTrainingService : IModelTrainingService
    {
        private readonly IRecommendationDataService _dataService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ModelTrainingService> _logger;
        private readonly string _modelArtifactsPath;

        public ModelTrainingService(IRecommendationDataService dataService, IWebHostEnvironment hostingEnvironment, ILogger<ModelTrainingService> logger)
        {
            _dataService = dataService;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _modelArtifactsPath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "MLModelArtifacts");
        }

        public async Task TrainDonationRecommenderModelAsync()
        {
            _logger.LogInformation("Starting donation recommender model training.");

            var mlContext = new MLContext();

            var preparedData = await _dataService.GetPreparedTrainingDataAsync();
            if (preparedData == null || !preparedData.Any())
            {
                _logger.LogWarning("No training data available. Skipping model training.");
                return;
            }

            var dataView = mlContext.Data.LoadFromEnumerable(preparedData);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(ModelInput.UserIdEncoded),
                MatrixRowIndexColumnName = nameof(ModelInput.ItemIdEncoded),
                LabelColumnName = nameof(ModelInput.Rating),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var pipeline = mlContext.Transforms.CopyColumns("Label", "Rating")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "UserIdEncodedKey", inputColumnName: "UserIdEncoded"))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "DonationIdEncodedKey", inputColumnName: "DonationIdEncoded"))
                .Append(mlContext.Recommendation().Trainers.MatrixFactorization(
                    labelColumnName: "Label",
                    matrixColumnIndexColumnName: "UserIdEncodedKey",
                    matrixRowIndexColumnName: "DonationIdEncodedKey"));

            _logger.LogInformation("Starting model training...");
            var model = pipeline.Fit(dataView);
            _logger.LogInformation("Model training completed.");

            var modelPath = Path.Combine(_modelArtifactsPath, "DonationRecommender.zip");
            mlContext.Model.Save(model, dataView.Schema, modelPath);

            _logger.LogInformation("Model saved to {Path}", modelPath);
        }

        public async Task TrainVolunteeringRecommenderModelAsync()
        {
            _logger.LogInformation("Starting volunteering recommender model training.");

            var mlContext = new MLContext();

            var preparedData = await _dataService.GetPreparedVolunteeringTrainingDataAsync();
            if (preparedData == null || !preparedData.Any())
            {
                _logger.LogWarning("No training data available for volunteering. Skipping model training.");
                return;
            }

            var dataView = mlContext.Data.LoadFromEnumerable(preparedData);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(ModelInput.UserIdEncoded),
                MatrixRowIndexColumnName = nameof(ModelInput.ItemIdEncoded),
                LabelColumnName = nameof(ModelInput.Rating),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var pipeline = mlContext.Recommendation().Trainers.MatrixFactorization(options);

            _logger.LogInformation("Starting volunteering model training...");
            var model = pipeline.Fit(dataView);
            _logger.LogInformation("Volunteering model training completed.");

            var modelPath = Path.Combine(_modelArtifactsPath, "VolunteeringRecommender.zip");
            mlContext.Model.Save(model, dataView.Schema, modelPath);

            _logger.LogInformation("Volunteering model saved to {Path}", modelPath);
        }
    }
}