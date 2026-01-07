using Microsoft.Extensions.Options;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Storage;

namespace PhotosApi.Services;

public class BucketInitializerService(
    IStorageRepository storageRepository,
    IOptions<MinioOptions> minioOptions,
    ILogger<BucketInitializerService> logger
) 
{
    
    public async Task InitializeBucketsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Проверка существования необходимых бакетов");
        var buckets = minioOptions.Value.Buckets;
        
        var bucketExistense =  buckets.Select(b => storageRepository.CheckBucketExistsAsync(b, cancellationToken))
            .ToArray();
        await Task.WhenAll(bucketExistense);

        if (bucketExistense.Any(t => !t.Result))
        {
            logger.LogWarning("Некоторые бакеты отсутствуют. Инициализация...");
            var tasks = buckets.Select(b => storageRepository.CreateBucketIfNotExistsAsync(b, cancellationToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        
        logger.LogInformation("Бакеты инициализированы");
    }
    
    
    // public async Task StartAsync(CancellationToken cancellationToken)
    // {
    //     logger.LogInformation("Starting bucket initializer");
    //     
    //     using var scope = scopeFactory.CreateScope();
    //     
    //     var storage = scope.ServiceProvider.GetRequiredService<IStorageRepository>();
    //     var minioOptions = scope.ServiceProvider
    //         .GetRequiredService<IOptions<MinioOptions>>()
    //         .Value;
    //     
    //     var tasks = minioOptions.Buckets
    //         .Select(b => CreateBucketIfNotExistsAsync(storage, b, cancellationToken))
    //         .ToArray();
    //
    //     await Task.WhenAll(tasks);
    //     logger.LogInformation("Bucket initializer finished");
    // }
    //
    // private async Task CreateBucketIfNotExistsAsync(
    //     IStorageRepository storage,
    //     string bucket,
    //     CancellationToken ct)
    // {
    //     try
    //     {
    //         if (await storage.CheckBucketExistsAsync(bucket, ct))
    //         {
    //             logger.LogInformation("Bucket {Bucket} already exists", bucket);
    //             return;
    //         }
    //
    //         await storage.CreateBucketAsync(bucket, ct);
    //         logger.LogInformation("Created bucket {Bucket}", bucket);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Bucket {Bucket} creation failed", bucket);
    //     }
    // }
    //
    // public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}