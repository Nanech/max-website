using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Photos;

public class PhotosQueryService(
    PhotosDbContext dbContext,
    IStorageRepository storage,
    ILogger<PhotosQueryService>  logger
)
{
    private const int ExpirySeconds = 3600; // 1 hour

    public async Task<string> GetPhotoUrlByIdAsync(Guid photoId, CancellationToken ct)
    {
        if (photoId == Guid.Empty)
            throw new ArgumentException("Album ID cannot be empty", nameof(photoId));
        
        var photo = await dbContext.Photos
            .Select(p => new { p.S3Path, p.PhotoId, p.Status })
            .FirstOrDefaultAsync(p => p.PhotoId == photoId, ct);

        if (photo == null)
            throw new FileNotFoundException($"Photo with ID {photoId} not found");

        var presignedUrl = await storage.GetPresignedUrl(storage.DefaultPhotosBucket, photo.S3Path, ExpirySeconds);
        
        return presignedUrl;
    }
    
 
    
    
    
}