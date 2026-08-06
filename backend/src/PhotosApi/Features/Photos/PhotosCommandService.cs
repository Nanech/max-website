using PhotosApi.Services;

namespace PhotosApi.Features.Photos;

public class PhotosCommandService(
    ILogger<PhotosCommandService>  logger,
    PhotoService service
)
{
    public async Task DeletePhotoByIdAsync(Guid photoId, CancellationToken ct) =>
        await service.DeletePhotoObjectAsync(photoId, ct); 
    
    public async Task<Guid> UploadPhotoAsync(UploadPhotoRequest request, CancellationToken ct) => 
        await service.CreatePhotoObjectAsync(request, ct);
}