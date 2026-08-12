using Microsoft.EntityFrameworkCore;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;

namespace PhotosApi.Features.Albums;

public class AlbumsCommandService(
    PhotosDbContext dbContext,
    ILogger<AlbumsCommandService> logger
)
{
    // public async Task DeleteAlbumByIdAsync(Guid albumId, CancellationToken ct)
    // {
    //     logger.LogInformation("Deleting album {AlbumId}", albumId);
    //     
    //     var album = await dbContext.Albums
    //         .Include(a => a.Photos)
    //         .FirstOrDefaultAsync(a => a.AlbumId == albumId, ct);
    //     
    //     if (album == null)
    //         throw new KeyNotFoundException($"Album {albumId} not found");
    //
    //     foreach (var photo in album.Photos)
    //     {
    //         try
    //         {
    //             await storage.RemoveFileAsync(
    //                 storage.DefaultPhotosBucket,
    //                 photo.S3Path,
    //                 ct
    //             );
    //
    //             logger.LogDebug("Файл {Path} удален", photo.S3Path);
    //         }
    //         catch (Exception e)
    //         {
    //             logger.LogWarning(e, "Ошибка при удалении файла {Path}", photo.S3Path);
    //         }
    //     }
    //     
    //     dbContext.Albums.Remove(album);
    //     await dbContext.SaveChangesAsync(ct);
    //     
    //     logger.LogInformation("Альбом {AlbumId} удален ({Count} фото)", albumId, album.Photos.Count);
    // }

    public async Task<Guid> CreateAlbumAsync(CreateAlbumCommand request, CancellationToken ct)
    {
        logger.LogInformation("Создание альбома {Name}", request.Name);

        var categoryExists = await dbContext.Categories.AnyAsync(c => 
            c.CategoryId == request.CategoryId, ct);
        
        if (!categoryExists)
            throw new ArgumentException($"Category {request.CategoryId} not found");

        var album = new Album
        {
            AlbumId = Guid.NewGuid(),
            Name = request.Name,
            CategoryId = request.CategoryId,
            ShootYear = request.ShootYear,
            CreatedAt =  DateTime.UtcNow,
            VisibilityStatus = request.Status
        };
        
        await dbContext.Albums.AddAsync(album, ct);
        await dbContext.SaveChangesAsync(ct);
        return album.AlbumId;
    }
    
    
}