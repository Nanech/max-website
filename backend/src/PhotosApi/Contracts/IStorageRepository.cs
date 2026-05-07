
namespace PhotosApi.Contracts;

public interface IStorageRepository
{
    public string DefaultPhotosBucket => "photos-bucket";
    Task<bool> CheckBucketExistsAsync(string bucketName, CancellationToken cancellationToken);
    Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken);
    Task CreateBucketIfNotExistsAsync(string bucketName, CancellationToken cancellationToken);
    Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken);
    Task<string> UploadFileAsync(UploadFileArgs args);
    Task RemoveFileAsync(string bucketName, string objectName, CancellationToken cancellationToken);
    Task<string> GetPresignedUrl(string bucketName, string objectName, int expirySeconds = 600);
    Task SetBucketConditionalPolicyAsync(string bucketName, CancellationToken cancellationToken);
    
    Task ChangeFilePathAsync(string bucketName, string objectName, string newFilePath, CancellationToken cancellationToken);
}
