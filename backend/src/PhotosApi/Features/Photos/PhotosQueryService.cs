using PhotosApi.Services;

namespace PhotosApi.Features.Photos;

public class PhotosQueryService(
    PhotoService service,
    ILogger<PhotosQueryService>  logger
)
{
    public async Task<PhotoUrlDto> GetPhotoUrlByIdAsync(Guid photoId, CancellationToken ct)
        => await service.GetPhotoUrlAsync(photoId, ct);
}