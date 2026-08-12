using PhotosApi.Services;

namespace PhotosApi.Features.Photos;

public class PhotosCommandService(PhotoService service)
{
    public async Task DeletePhotoByIdAsync(List<Guid> photoIds, CancellationToken ct) =>
        await service.DeletePhotosAsync(photoIds, ct); 
    
    public async Task<List<Guid>> UploadPhotoAsync(UploadPhotoRequest request, CancellationToken ct) => 
        await service.CreatePhotoObjectAsync(request, ct);
}