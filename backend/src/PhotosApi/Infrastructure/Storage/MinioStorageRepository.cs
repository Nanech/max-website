using Minio;
using Minio.DataModel.Args;
using PhotosApi.Contracts;

namespace PhotosApi.Infrastructure.Storage;

// todo: implement cancelation tokens
// todo: implement logging
// todo: implement retry policy

public class MinioStorageRepository(
    IMinioClient minioClient,
    ILogger<MinioStorageRepository> logger
    ) : IStorageRepository
{
    public async Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Получения списка бакетов из MinIO");
        var buckets = await minioClient.ListBucketsAsync(cancellationToken);
        logger.LogDebug("Найдено {Count} бакетов", buckets.Buckets.Count);
        return buckets.Buckets.Select(b => b.Name).ToList();
    }
    
    public async Task<bool> CheckBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Проверка существования бакета {BucketName}", bucketName);
        var bucketArgs = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await minioClient.BucketExistsAsync(bucketArgs, cancellationToken);
        logger.LogDebug("Бакет {BucketName} существует: {Exists}", bucketName, exists);
        return exists;
    }

    public async Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Создание бакета {BucketName}", bucketName);
        var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
        await minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
        logger.LogDebug("Бакет {BucketName} создан", bucketName);
    }
    
    public async Task CreateBucketIfNotExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        if (!await CheckBucketExistsAsync(bucketName, cancellationToken))
        {
            logger.LogInformation("Бакет {BucketName} не существует. Создание...", bucketName);
            await CreateBucketAsync(bucketName, cancellationToken);
            logger.LogInformation("Бакет {BucketName} успешно создан", bucketName);
        }
    }
    
    
    public async Task<string> UploadFileAsync(UploadFileArgs args)
    {
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(args.BucketName)
            .WithObject(args.ObjectName)
            .WithStreamData(args.Data)
            .WithObjectSize(args.Data.Length)
            .WithContentType(args.ContentType);
        
        await minioClient.PutObjectAsync(putObjectArgs, args.CancellationToken);
        
        return args.ObjectName;
    }
    
    public async Task RemoveFileAsync(string bucketName, string objectName, CancellationToken cancellationToken)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);
        
        await minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task<string> GetPresignedUrl(string bucketName, string objectName, int expirySeconds = 600)
    {
        var getObjectArgs = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expirySeconds);

        return await minioClient.PresignedGetObjectAsync(getObjectArgs);
    }
}