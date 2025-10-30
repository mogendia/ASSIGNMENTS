using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Infrastucture.Repositories
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly ILogger<TemporalBlockCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TemporalBlockCleanupService(
            ILogger<TemporalBlockCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporal Block Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredBlocksAsync();
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired temporal blocks");
                }
            }
        }

        private async Task CleanupExpiredBlocksAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITemporalBlockRepository>();

            var expiredBlocks = await repository.GetExpiredBlocksAsync();

            foreach (var block in expiredBlocks)
            {
                await repository.RemoveTemporalBlockAsync(block.CountryCode);
                _logger.LogInformation(
                    "Removed expired temporal block for country: {CountryCode}",
                    block.CountryCode);
            }

            if (expiredBlocks.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired temporal blocks", expiredBlocks.Count);
            }
        }
    }
}
