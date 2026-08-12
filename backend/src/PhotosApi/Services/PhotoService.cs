using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhotosApi.Contracts;
using PhotosApi.Features.Photos;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Infrastructure.Storage;
using PhotosApi.Models;

namespace PhotosApi.Services;

public class PhotoService(
    PhotosDbContext dbContext,
    IObjectRepository objectRepo,
    ImageSharpPhotoProcessor photoProcessor,
    IOptions<MinioOptions>  minioOptions,
    ILogger<PhotoService> logger
)
{
    private string PhotoBucket => minioOptions.Value.PhotosBucket; 
    private PhotoUrlResolver PhotoUrlResolver => new(objectRepo, minioOptions);
    
    public async Task DeletePhotosAsync(IEnumerable<Guid> photoIds, CancellationToken ct)
    {
        var idsList = photoIds.ToList();
        if (!idsList.Any()) return;

        var photos = await dbContext.Photos.Where(p => idsList.Contains(p.PhotoId)).ToListAsync(ct);
        if (!photos.Any()) return;

        dbContext.Photos.RemoveRange(photos);
        await dbContext.SaveChangesAsync(ct);

        var allPaths = photos.SelectMany(p => PhotoObjectFactory.GetAllVersionsPaths(p.PhotoId))
            .ToList();

        try
        {
            await objectRepo.RemoveManyFilesAsync(PhotoBucket, allPaths, ct);
            logger.LogInformation("Deleted {Count} photos and files", photos.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete {Count} photos and files", photos.Count);
            throw;
        }
    }

    public async Task<List<PhotoUrlDto>> GetPhotosUrlsByAlbumAsync(Guid albumId, CancellationToken ct)
    {
        var album = await dbContext.Albums.Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AlbumId == albumId, ct);
        
        if (album == null)
            throw new KeyNotFoundException($"Album with id {albumId} not found");

        var photoTasks = album.Photos.Select(x => PhotoUrlResolver.GetPhotoUrlsAsync(x, ct));
        var photoUrls = await Task.WhenAll(photoTasks);
        return photoUrls.ToList();
    }
    
    public async Task<PhotoUrlDto> GetPhotoUrlAsync(Guid photoId, CancellationToken ct)
    {
        var photo = await dbContext.Photos.Include(p => p.Album)
            .FirstOrDefaultAsync(p => p.PhotoId == photoId, cancellationToken: ct);
        
        if (photo == null)
            throw new KeyNotFoundException("Photo not found");

        return await PhotoUrlResolver.GetPhotoUrlsAsync(photo, ct);
    }
    
    public async Task<List<Guid>> CreatePhotoObjectAsync(UploadPhotoRequest request, CancellationToken ct)
    {
        var album = await dbContext.Albums.
            Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.AlbumId == request.AlbumId, ct);
        
        if (album == null)
            throw new KeyNotFoundException($"Album with id {request.AlbumId} not found");
       
        var uploadTasks = request.Files.Select(async file =>
        {
            var photoId = Guid.NewGuid();

            try
            {
                var photosPaths = await ProcessPhotoAsync(photoId, file);
                
                await UploadFileToObjectStorageAsync(photosPaths, ct);
                
                logger.LogInformation("Photo {PhotoId} created successfully", photoId);
                return photoId;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process photo {PhotoId} in batch upload", photoId);
                throw;
            }
        });
        
        var createdIds = await Task.WhenAll(uploadTasks);
        await SavePhotoToDbAsync(album, createdIds, ct);
        
        return createdIds.ToList();
    }

    private async Task<ProcessedPhotoGroup> ProcessPhotoAsync(Guid photoId, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        return await photoProcessor.ProcessPhotoAsync(photoId, stream);
    }

    private async Task UploadFileToObjectStorageAsync(ProcessedPhotoGroup photos, CancellationToken ct)
    {
        var uploadTasks = photos.GetAllVersions()
            .Select(version => UploadWithRetryASync(version, ct));
        
        try
        {
            await Task.WhenAll(uploadTasks);
            logger.LogInformation("All photo versions uploaded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload one or more photo versions");
            throw;
        }
        
        logger.LogInformation("Photos successfully uploaded");
    }
    
    private async Task UploadWithRetryASync(ProcessedPhotoVersion version, CancellationToken ct, int maxAttempts = 3)
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (version.Data.CanSeek)
                    version.Data.Position = 0;
                
                var id = await objectRepo.UploadFileAsync(new UploadFileArgs(
                    PhotoBucket,
                    version.Path,
                    version.Data,
                    version.ContentType,
                    ct)
                );

                if (!string.IsNullOrEmpty(id))
                    return;

                throw new Exception();
            }
            catch (Exception e)
            {
                lastException = e;
                logger.LogWarning(
                    e,
                    "Upload failed for {Path} (attempt {Attempt})",
                    version.Path,
                    attempt
                    );
                if (attempt < maxAttempts)
                {
                    int delayMs = (int)(Math.Pow(2, attempt) * 100); // 200ms, 400ms, 800ms
                    await Task.Delay(delayMs, ct);
                }
            }
        }
        
        throw new InvalidOperationException(
            $"Failed to upload {version.Path} after {maxAttempts} attempts",
            lastException);
    }

    private async Task SavePhotoToDbAsync(Album album, IEnumerable<Guid> photoIds, CancellationToken ct)
    {
        await using var tran = await dbContext.Database.BeginTransactionAsync(ct);

        var photos = photoIds.Select(photoId => new Photo
        {
            PhotoId = photoId,
            AlbumId = album.AlbumId,
            Album = album,
            UploadedAt = DateTime.UtcNow
        }).ToList();

        try
        {
            await dbContext.Photos.AddRangeAsync(photos, ct);
            await dbContext.SaveChangesAsync(ct);
            await tran.CommitAsync(ct);
            logger.LogDebug("Photos successfully saved in Db");
        }
        catch (Exception e)
        {
            await tran.RollbackAsync(ct);
            // todo: delete photos from storage  
            
            throw new InvalidOperationException(
                $"Error while saving photo in Db. File deleted from storage: {e.Message}",
                e
            );
        }
        
    } 
    
}