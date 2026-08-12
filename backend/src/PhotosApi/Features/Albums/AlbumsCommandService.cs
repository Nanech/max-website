using Microsoft.EntityFrameworkCore;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;
using PhotosApi.Services;

namespace PhotosApi.Features.Albums;

public class AlbumsCommandService(
    PhotosDbContext dbContext,
    PhotoService service,
    ILogger<AlbumsCommandService> logger
)
{
    public async Task DeleteAlbumByIdAsync(Guid albumId, CancellationToken ct)
    {
        var album = await dbContext.Albums
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AlbumId == albumId, ct);
   
        if (album == null)
            throw new ArgumentException($"Album {albumId} not found");
        
        var photoIds = album.Photos.Select(p => p.PhotoId);
        await service.DeletePhotosAsync(photoIds, ct);
        
        dbContext.Albums.Remove(album);
        await dbContext.SaveChangesAsync(ct);
    }

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