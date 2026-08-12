using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Storage;

namespace PhotosApi.Contracts;

public interface IObjectRepository
{
    Task<bool> CheckBucketExistsAsync(string bucketName, CancellationToken cancellationToken);
    Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken);
    Task CreateBucketIfNotExistsAsync(string bucketName, CancellationToken cancellationToken);
    Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken);
    Task<string> UploadFileAsync(UploadFileArgs args);
    Task RemoveFileAsync(string bucketName, string objectName, CancellationToken ct);
    Task RemoveManyFilesAsync(string bucketName, IEnumerable<string> objectsKeys, CancellationToken ct);
    Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 600);
}
