namespace PhotosApi.Infrastructure.Storage;

public record UploadFileArgs(
    string BucketName,
    string ObjectName,
    Stream Data,
    string ContentType,
    CancellationToken CancellationToken = default
);