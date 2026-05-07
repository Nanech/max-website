using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Features.Photos.Shared;
using PhotosApi.Models;

namespace PhotosApi.Features.Photos.Commands;

public record UploadPhotoCommand(
    [Required] IFormFile File, 
    [Required] Guid AlbumId,
    PhotoStatus Status = PhotoStatus.Draft
) : IRequest<Guid>;

public class UploadPhotoHandler(
    IStorageRepository storage,
    PhotosDbContext dbContext,
    ILogger<UploadPhotoHandler> logger
) : IRequestHandler<UploadPhotoCommand, Guid>
{
    private const int MaxRetries = 3;
    
    public async Task<Guid> Handle(UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        
        var album = await ValidateAlbum(request.AlbumId, cancellationToken);
        
        var photoId = Guid.NewGuid();
        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var fileName = $"{photoId}{extension}";
        var s3Path = StoragePaths.BuildPhotoPath(request.Status, fileName);
        
        await UploadToMinioAsyncWithRetries(request.File, s3Path, cancellationToken);
        await SaveToDbAsync(album, album.AlbumId, photoId, s3Path, request.Status, cancellationToken);

        return photoId;
    }

    private static void ValidateRequest(UploadPhotoCommand request)
    {
        if (request.AlbumId == Guid.Empty)
            throw new ValidationException("AlbumId can't be empty");
        
        if (!Enum.IsDefined(request.Status))
            throw new ValidationException("Status can't be invalid");
        
        PhotoFileValidator.ValidateFile(request.File);
    }

    private async Task<Album> ValidateAlbum(Guid albumId, CancellationToken cancellationToken)
    {
        var album = await dbContext.Albums
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.AlbumId == albumId, cancellationToken);

        return album ?? throw new KeyNotFoundException($"Album {albumId} not found");
    }

    private async Task UploadToMinioAsyncWithRetries(
        IFormFile file,
        string s3Path,
        CancellationToken ct
    )
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
    

}