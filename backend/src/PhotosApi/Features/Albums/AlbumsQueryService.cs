using Microsoft.EntityFrameworkCore;
using PhotosApi.Features.Photos;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Albums;

public class AlbumsQueryService(
    PhotosDbContext dbContext,
    ILogger<AlbumsQueryService> logger
)
{
    // public async Task<AlbumDto> GetAlbumFiltersAsync()
    
    
    public async Task<AlbumDetailDto> GetAlbumByIdAsync(Guid id, CancellationToken ct)
    {
        var album = await dbContext.Albums
            .Include(a => a.Category)
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AlbumId == id, ct);
        
        if (album == null)
            throw new KeyNotFoundException($"Album {id} not found");

        var photosDto = new List<PhotoDto>();

        foreach (var photo in album.Photos.OrderBy(p => p.UploadedAt))
        {
            // var url = await storage.GetPresignedUrl(bu)
        }

        return new AlbumDetailDto(
            album.AlbumId,
            album.Name,
            album.CategoryId,
            album.Category?.Name,
            album.ShootYear,
            album.CreatedAt,
            photosDto
        );
    }
    
    public async Task<List<AlbumDto>> GetAlbumsWithFiltersAsync(GetAlbumsQuery filters, CancellationToken ct)
    {
        var query = dbContext.Albums.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            query = query.Where(a => EF.Functions.ILike(a.Name, $"%{filters.SearchTerm}%"));
        
        if (filters.CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == filters.CategoryId.Value);
        
        if (filters.Year.HasValue)
            query = query.Where(a => a.ShootYear == filters.Year.Value);
        
        var albums = await query
            .Include(a => a.Category)
            .Include(a => a.Photos)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AlbumDto(
                a.AlbumId,
                a.Name,
                a.CategoryId,
                a.Category != null ? a.Category.Name : null,
                a.ShootYear,
                a.Photos.Count,
                a.CreatedAt
            ))
            .ToListAsync(ct);
        return albums;
    }
    
}