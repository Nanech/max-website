using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Albums.Commands;

public record DeleteAlbumCommand(Guid AlbumId) : IRequest;

public class DeleteAlbumHandler(
    PhotosDbContext dbContext,
    IStorageRepository storage,
    ILogger<DeleteAlbumHandler> logger
) : IRequestHandler<DeleteAlbumCommand>
{
    public async Task Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Удаление альбом {AlbumId}", request.AlbumId);
        
        var album = await dbContext.Albums
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AlbumId == request.AlbumId, cancellationToken);
        
        if (album == null)
            throw new KeyNotFoundException($"Album {request.AlbumId} not found");

        foreach (var photo in album.Photos)
        {
            try
            {
                await storage.RemoveFileAsync(
                    storage.DefaultPhotosBucket,
                    photo.S3Path,
                    cancellationToken
                );

                logger.LogDebug("Файл {Path} удален", photo.S3Path);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Ошибка при удалении файла {Path}", photo.S3Path);
            }
        }
        
        dbContext.Albums.Remove(album);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Альбом {AlbumId} удален ({Count} фото)", request.AlbumId, album.Photos.Count);
    }
}
