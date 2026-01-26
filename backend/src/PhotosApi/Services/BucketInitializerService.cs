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
        
        var bucketExistense = 
            buckets.Select(b => storageRepository.CheckBucketExistsAsync(b, cancellationToken))
            .ToArray();
        await Task.WhenAll(bucketExistense);

        if (bucketExistense.Any(t => !t.Result))
        {
            logger.LogWarning("Некоторые бакеты отсутствуют. Инициализация...");
            var tasks =
                buckets.Select(b => storageRepository.CreateBucketIfNotExistsAsync(b, cancellationToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        
        logger.LogInformation("Бакеты инициализированы");
    }
}