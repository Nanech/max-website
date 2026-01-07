namespace PhotosApi.Infrastructure.Storage;

public class MinioOptions
{
    public const string SectionName = "Minio";
    public List<string> Buckets { get; init; } = [];
}