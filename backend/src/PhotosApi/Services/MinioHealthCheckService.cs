using PhotosApi.Contracts;

namespace PhotosApi.Services;

public class MinioHealthCheckService(
    ILogger<MinioHealthCheckService> logger,
    BucketInitializerService bucketInitializerService,
    IStorageRepository storage
    )
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const int maxRetries = 15;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("MinIo health check attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                var buckets = await storage.ListBucketsAsync(cancellationToken);
                logger.LogInformation("MinIo is healthy and reachable.");
                
                if (buckets.Count > 0)
                    logger.LogInformation("Found {BucketCount} buckets in MinIo.", buckets.Count);
                else
                    logger.LogWarning("No buckets found in MinIo.");
                
                await bucketInitializerService.InitializeBucketsAsync(cancellationToken);
                
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "MinIo health check failed");
                if (attempt == maxRetries) throw;
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}