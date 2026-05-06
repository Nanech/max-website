namespace PhotosApi.Infrastructure.Storage;

public class MinioOptions
{
    public const string SectionName = "Minio";
    public string Container { get; init; } = string.Empty;
    public string PublicEndpoint { get; set; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string GetContainerConnection => $"{Container}:{Port}";
    public string GetPublicEndpoint() => Endpoint;
    public string GetInternalEndpoint() => $"{Endpoint}:{Port}";
    public List<string> Buckets { get; init; } = [];
}