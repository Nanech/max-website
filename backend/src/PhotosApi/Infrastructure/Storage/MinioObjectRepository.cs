using Minio;
using Minio.DataModel.Args;
using PhotosApi.Contracts;

namespace PhotosApi.Infrastructure.Storage;

public class MinioObjectRepository(
    [FromKeyedServices("internal")] IMinioClient internalClient,
    [FromKeyedServices("public")] IMinioClient publicClient,
    ILogger<MinioObjectRepository> logger)
    : IObjectRepository
{


    public async Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Получения списка бакетов из MinIO");
        var buckets = await internalClient.ListBucketsAsync(cancellationToken);
        logger.LogDebug("Найдено {Count} бакетов", buckets.Buckets.Count);
        return buckets.Buckets.Select(b => b.Name).ToList();
    }
    
    public async Task<bool> CheckBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Проверка существования бакета {BucketName}", bucketName);
        var bucketArgs = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await internalClient.BucketExistsAsync(bucketArgs, cancellationToken);
        logger.LogDebug("Бакет {BucketName} существует: {Exists}", bucketName, exists);
        return exists;
    }

    public async Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Создание бакета {BucketName}", bucketName);
        var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
        await internalClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
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
        if (args.Data.CanSeek)
            args.Data.Position = 0;
        
        logger.LogDebug("Загрузка файла {ObjectName} в бакет {BucketName} (размер: {Size} байт)", 
            args.ObjectName, args.BucketName, args.Data.Length);
        
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(args.BucketName)
            .WithObject(args.ObjectName)
            .WithStreamData(args.Data)
            .WithObjectSize(args.Data.Length)
            .WithContentType(args.ContentType);
        
        await internalClient.PutObjectAsync(putObjectArgs, args.CancellationToken);
        
        logger.LogDebug("Файл {ObjectName} успешно загружен в бакет {BucketName}", args.ObjectName, args.BucketName);
        
        return args.ObjectName;
    }
    
    public async Task RemoveFileAsync(string bucketName, string objectName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Удаление файла {ObjectName} из бакета {BucketName}", objectName, bucketName);
        
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);
        
        await internalClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task RemoveManyFilesAsync(string bucketName, IEnumerable<string> objectsKeys, CancellationToken ct)
    {
        var enumerable = objectsKeys as string[] ?? objectsKeys.ToArray();
        
        logger.LogDebug("Удаление пачки файлов ({Count}) из бакета {Bucket}", enumerable.Length, bucketName);
        
        var removeObjectsArgs = new RemoveObjectsArgs()
            .WithBucket(bucketName)
            .WithObjects(enumerable);
        
        await internalClient.RemoveObjectsAsync(removeObjectsArgs, ct);
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucketName,
        string objectName,
        int expirySeconds = 600
        )
    {
        var getObjectArgs = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expirySeconds);

        return await publicClient.PresignedGetObjectAsync(getObjectArgs);
    }

   
}