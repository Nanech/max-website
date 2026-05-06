  using Microsoft.Extensions.Options;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Storage;

namespace PhotosApi.Services;

public class BucketInitializerService(
    IStorageRepository storageRepository,
    IOptions<MinioOptions> minioOptions,
    ILogger<BucketInitializerService> logger
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => InitializeBucketsAsync(cancellationToken);
    
    public async Task InitializeBucketsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Проверка существования необходимых бакетов");
        var buckets = minioOptions.Value?.Buckets.ToList() ?? new List<string>();
        if (!buckets.Contains(storageRepository.DefaultPhotosBucket))
        {
            buckets.Add(storageRepository.DefaultPhotosBucket);
        }        
        
        var bucketExists = 
            buckets.Select(b => storageRepository.CheckBucketExistsAsync(b, cancellationToken))
            .ToArray();
        await Task.WhenAll(bucketExists);

        if (bucketExists.Any(t => !t.Result))
        {
            logger.LogWarning("Некоторые бакеты отсутствуют. Инициализация...");
            var tasks =
                buckets.Select(b => storageRepository.CreateBucketIfNotExistsAsync(b, cancellationToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        
        logger.LogInformation("Применение политик для photos-bucket");

        try
        {
            await storageRepository.SetBucketConditionalPolicyAsync(
                storageRepository.DefaultPhotosBucket,
                cancellationToken
            );
            
            logger.LogInformation("Conditional policy успешно применены");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при применении conditional policy");
            throw;
        }
        
        logger.LogInformation("Бакеты инициализированы");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}