using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WaslAlkhair.Api.Services.Recommendation;

namespace WaslAlkhair.Api.Services
{
    public class ModelRetrainingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModelRetrainingBackgroundService> _logger;

        public ModelRetrainingBackgroundService(IServiceProvider serviceProvider, ILogger<ModelRetrainingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Model Retraining Background Service is starting.");

            // Run once at startup, then on a schedule
            await DoWork(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Model Retraining Background Service is stopping.");
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Model retraining work is starting.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var modelTrainingService = scope.ServiceProvider.GetRequiredService<IModelTrainingService>();
                try
                {
                    _logger.LogInformation("Starting donation model retraining cycle.");
                    await modelTrainingService.TrainDonationRecommenderModelAsync();
                    _logger.LogInformation("Successfully completed donation model retraining cycle.");

                    _logger.LogInformation("Starting volunteering model retraining cycle.");
                    await modelTrainingService.TrainVolunteeringRecommenderModelAsync();
                    _logger.LogInformation("Successfully completed volunteering model retraining cycle.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the scheduled model retraining.");
                }
            }
        }
    }
} 