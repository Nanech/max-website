using Microsoft.Extensions.Options;
using PhotosApi.Contracts;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Storage;
using PhotosApi.Models;

namespace PhotosApi.Services;

public record PhotoUrlDto(string OriginalUrl, string LargeUrl, string PreviewUrl);

public class PhotoUrlResolver(
    IObjectRepository objectRepository,
    IOptions<MinioOptions> minioOptions
)
{
    private string PhotosBucket { get; init; } = minioOptions.Value.PhotosBucket;
    private const int PrivateUrlExpirationSeconds = 900; // 15 minutes
    private string BaseUrlCdn => minioOptions.Value.PublicEndpoint;
    
    public async Task<PhotoUrlDto> GetPhotoUrlsAsync(Photo photo, CancellationToken ct)
    {
        if (photo?.AlbumId == null)
            throw new ArgumentNullException(nameof(photo), "Photo or album cannot be null.");

        var isPublic = photo.Album.VisibilityStatus == AlbumStatus.Published;
        if (isPublic)
            return GetPublicPhotoUrlsDto(photo);
        
        return await GetPrivatePhotoUrlsDto(photo);
    }

    private PhotoUrlDto GetPublicPhotoUrlsDto(Photo photo)
    {
        return new PhotoUrlDto(
            OriginalUrl: $"{BaseUrlCdn}/{PhotoObjectFactory.BuildOriginalPath(photo.PhotoId)}" ,
            LargeUrl:$"{BaseUrlCdn}/{PhotoObjectFactory.BuildLargePath(photo.PhotoId)}" , 
            PreviewUrl:$"{BaseUrlCdn}/{PhotoObjectFactory.BuildPreviewPath(photo.PhotoId)}" 
        );
    }

    private async Task<PhotoUrlDto> GetPrivatePhotoUrlsDto(Photo photo)
    {
        var versionsDict = PhotoObjectFactory.GetAllAsVersionDictionary(photo.PhotoId);
        var presignedUrlsTasks = versionsDict.Select(async kvp => new
            {
                Version = kvp.Key,
                Url = await objectRepository.GetPresignedUrlAsync(PhotosBucket, kvp.Value, PrivateUrlExpirationSeconds)
            });
        
        try
        {
            var results = await Task.WhenAll(presignedUrlsTasks);
            
            var presignedUrls = results.ToDictionary(x => x.Version,
                x => x.Url);
            
            return new PhotoUrlDto(
                OriginalUrl: presignedUrls["original"],
                LargeUrl: presignedUrls["large"],
                PreviewUrl: presignedUrls["preview"]
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
}