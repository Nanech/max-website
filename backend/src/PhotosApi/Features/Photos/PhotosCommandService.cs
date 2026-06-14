using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;

namespace PhotosApi.Features.Photos;

public class PhotosCommandService(
    PhotosDbContext dbContext,
    IStorageRepository storage,
    ILogger<PhotosCommandService>  logger
)
{
    private const int MaxRetries = 3;
    
    public async Task DeletePhotoByIdAsync(Guid photoId, CancellationToken ct)
    {
        var photo = await dbContext.Photos
            .FirstOrDefaultAsync(p => p.PhotoId == photoId, ct);
        
        if (photo == null)
            throw new FileNotFoundException($"Photo with id {photoId} not found");
        
        dbContext.Photos.Remove(photo);
        await dbContext.SaveChangesAsync(ct);
        
        await storage.RemoveFileAsync(storage.DefaultPhotosBucket, photo.S3Path, ct);
    }

    public async Task ChangePhotoStatusAsync(Guid photoId, PhotoStatus newStatus, CancellationToken ct)
    {
        var photo = await dbContext.Photos.
            FirstOrDefaultAsync(p => p.PhotoId == photoId, ct);
        
        if (photo == null)
            throw new KeyNotFoundException($"Photo with ID {photoId} not found");

        if (photo.Status == newStatus) return;
        
        // file path logic
        var oldPath = photo.S3Path;
        if (string.IsNullOrEmpty(oldPath))
            throw new InvalidOperationException($"Photo with ID {photoId} has no S3 path");
        
        var filePath = StoragePaths.GetFileName(oldPath);
        logger.LogDebug("The photo with id {PhotoPhotoId} has file path: {FilePath}", photo.PhotoId, filePath);
        var newPath = StoragePaths.BuildPhotoPath(newStatus, filePath);

        if (oldPath == newPath) return;
        
        // create a copy of object in s3
        await storage.ChangeFilePathAsync(storage.DefaultPhotosBucket, oldPath, newPath, ct);
     
        // change data in db
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        photo.Status = newStatus;
        photo.S3Path = newPath;
        dbContext.Photos.Update(photo);
        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        try
        {
            await storage.RemoveFileAsync(storage.DefaultPhotosBucket, oldPath, ct);
            logger.LogDebug("Old file removed {FilePath}", oldPath);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to remove old file {oldPath}, but operation is ok", oldPath);
        }
    }

    public async Task<Guid> UploadPhotoAsync(UploadPhotoRequest request, CancellationToken ct)
    {
        var album = await ValidateAlbumAsync(request.AlbumId, ct);
        
        var photoId = Guid.NewGuid();
        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var fileName = $"{photoId}{extension}";
        var s3Path = StoragePaths.BuildPhotoPath(request.Status, fileName);
        
        await UploadToMinioAsyncWithRetries(request.File, s3Path, ct);
        await SaveToDbAsync(album, album.AlbumId, photoId, s3Path, request.Status, ct);

        return photoId;
    }
    
    private async Task UploadToMinioAsyncWithRetries(IFormFile file, string s3Path, CancellationToken ct)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < MaxRetries)
        {
            try
            {
                attempt++;

                await using var stream = file.OpenReadStream();

                await storage.UploadFileAsync(new UploadFileArgs(
                    storage.DefaultPhotosBucket,
                    s3Path,
                    stream,
                    file.ContentType,
                    ct
                ));

                return;
            }
            catch (Exception e) when (attempt < MaxRetries)
            {
                lastException = e;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                
                await Task.Delay(delay, ct);
            }
        }
        
        logger.LogError(
            lastException,
            "Не удалось загрузить файл после {MaxRetries} попыток. S3Path: {S3Path}",
            MaxRetries,
            s3Path
        );
        
        throw new InvalidOperationException(
            $"Не удалось загрузить файл Minio после {MaxRetries} попыток. " +
            $"Last error: {lastException?.Message}"
            );
    }

    private async Task SaveToDbAsync(
        Album album, 
        Guid albumId, 
        Guid photoId,
        string s3Path, 
        PhotoStatus status,
        CancellationToken ct)
    {
        await using var tran = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var photo = new Photo
            {
                PhotoId = photoId,
                AlbumId = albumId,
                Album = album,
                S3Path = s3Path,
                Status = status,
                UploadedAt = DateTime.UtcNow,
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
            await DeleteFileInStorageAsync(s3Path, ct);

            throw new InvalidOperationException(
                $"Error while saving photo in Db. File deleted from storage: {e.Message}",
                e
            );

        }
    }

    private async Task DeleteFileInStorageAsync(string s3Path, CancellationToken ct)
    {
        try
        {
            await storage.RemoveFileAsync(storage.DefaultPhotosBucket, s3Path, ct);
            logger.LogDebug("File successfully deleted in Storage {s3Path}", s3Path);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while deleting file in Storage {s3Path}", s3Path);
        }
    }
    
    
    public async Task<Album> ValidateAlbumAsync(Guid albumId, CancellationToken cancellationToken)
    {
        var album = await dbContext.Albums
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.AlbumId == albumId, cancellationToken);

        return album ?? throw new KeyNotFoundException($"Album {albumId} not found");
    }
    
}