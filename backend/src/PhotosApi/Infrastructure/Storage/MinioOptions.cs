namespace PhotosApi.Infrastructure.Storage;

public class MinioOptions
{
    public string Container { get; set; } = string.Empty;
    public string PublicEndpoint { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }

    public string PhotosBucket { get; set; } = string.Empty;
  
}