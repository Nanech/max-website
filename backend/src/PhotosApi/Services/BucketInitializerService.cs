using Microsoft.Extensions.Options;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Storage;

namespace PhotosApi.Services;

public class BucketInitializerService(
    IServiceProvider serviceProvider,
    IOptions<MinioOptions> minioOptions,
    ILogger<BucketInitializerService> logger
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => InitializeBucketsAsync(cancellationToken);
    
    public async Task InitializeBucketsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Проверка существования необходимых бакетов");

        using var scope = serviceProvider.CreateScope();
        var objectRepository = scope.ServiceProvider.GetRequiredService<IObjectRepository>();
        
        var buckets = new HashSet<string> { minioOptions.Value.PhotosBucket };
        logger.LogInformation("Бакеты которые поступили: {buckets}, {photosBuckets}" , buckets, minioOptions.Value.PhotosBucket);
        
        var bucketExists = 
            buckets.Select(b => objectRepository.CheckBucketExistsAsync(b, cancellationToken))
            .ToArray();
        await Task.WhenAll(bucketExists);
        
        if (bucketExists.Any(t => !t.Result))
        {
            logger.LogWarning("Некоторые бакеты отсутствуют. Инициализация...");
            var tasks =
                buckets.Select(b => objectRepository.CreateBucketIfNotExistsAsync(b, cancellationToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        
        logger.LogInformation("Бакеты инициализированы");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}