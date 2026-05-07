namespace PhotosApi.Contracts;

public record UploadFileArgs(
    string BucketName,
    string ObjectName,
    Stream Data,
    string ContentType,
    CancellationToken CancellationToken = default
);