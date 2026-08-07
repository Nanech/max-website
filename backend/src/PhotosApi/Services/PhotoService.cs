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
    private PhotoUrlResolver PhotoUrlResolver => new PhotoUrlResolver(objectRepo, minioOptions);
    
    public async Task DeletePhotoObjectAsync(Guid photoId, CancellationToken ct)
    {
        var photo = await dbContext.Photos
            .FirstOrDefaultAsync(p => p.PhotoId == photoId, ct);
        
        if (photo == null)
            throw new FileNotFoundException($"Photo with id {photoId} not found");
        
        dbContext.Photos.Remove(photo);
        await dbContext.SaveChangesAsync(ct);

        var paths = PhotoObjectFactory.GetAllVersionsPaths(photoId);
        await objectRepo.RemoveManyFilesAsync(PhotoBucket, paths, ct);
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
    
    public async Task<Guid> CreatePhotoObjectAsync(UploadPhotoRequest request, CancellationToken ct)
    {
        var album = await dbContext.Albums.
            Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.AlbumId == request.AlbumId, ct);
        
        if (album == null)
            throw new KeyNotFoundException($"Album with id {request.AlbumId} not found");

        var photoId = Guid.NewGuid();
        
        try
        {
            var photosPaths = await ProcessPhotoAsync(photoId, request.File);
            await UploadFileToObjectStorageAsync(photosPaths, ct);

            await SavePhotoToDbAsync(album, photoId, ct);
        
            logger.LogInformation("Photo {PhotoId} created successfully", photoId);
            return photoId;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create photo {PhotoId}", photoId);
            throw;
        }
    }

    private async Task<ProcessedPhotoGroup> ProcessPhotoAsync(Guid photoId, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        return await photoProcessor.ProcessPhotoAsync(photoId, stream);
    }

    private async Task UploadFileToObjectStorageAsync(ProcessedPhotoGroup photos, CancellationToken ct)
    {
        foreach (var version in photos.GetAllVersions())
        {
            await UploadWithRetryASync(version, ct);
        }
        
        logger.LogInformation("Photos successfully uploaded");
        
        // var uploadTasks = photos.GetAllVersions()
        //     .Select(version => UploadWithRetryASync(version, ct));
        //
        // try
        // {
        //     await Task.WhenAll(uploadTasks);
        //     logger.LogInformation("All photo versions uploaded successfully");
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ex, "Failed to upload one or more photo versions");
        //     throw;
        // }
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

    private async Task SavePhotoToDbAsync(Album album, Guid photoId, CancellationToken ct)
    {
        await using var tran = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var photo = new Photo
            {
                PhotoId = photoId,
                AlbumId = album.AlbumId,
                Album = album,
                UploadedAt = DateTime.UtcNow
            };

            await dbContext.Photos.AddAsync(photo, ct);
            await dbContext.SaveChangesAsync(ct);
            await tran.CommitAsync(ct);
            
            logger.LogDebug("Photo successfully saved in Db");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error while saving Photo in Db. Rollback tran. PhotoId: {PhotoId}",
                photoId
            );
            
            await tran.RollbackAsync(ct);
            await DeletePhotoObjectAsync(photoId, ct);
            
            throw new InvalidOperationException(
                $"Error while saving photo in Db. File deleted from storage: {e.Message}",
                e
            );
        }
    } 
    
    

}