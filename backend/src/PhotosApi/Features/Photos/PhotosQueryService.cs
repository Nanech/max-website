using PhotosApi.Services;

namespace PhotosApi.Features.Photos;

public class PhotosQueryService(PhotoService service)
{
    public async Task<PhotoUrlDto> GetPhotoUrlByIdAsync(Guid photoId, CancellationToken ct)
        => await service.GetPhotoUrlAsync(photoId, ct);
    
    public async Task<List<PhotoUrlDto>> GetPhotosUrlsByAlbumAsync(Guid albumId, CancellationToken ct) 
        => await service.GetPhotosUrlsByAlbumAsync(albumId, ct);
    
}