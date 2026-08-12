using Microsoft.EntityFrameworkCore;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Albums;

public class AlbumsQueryService(
    PhotosDbContext dbContext
)
{
    // public async Task<AlbumDto> GetAlbumFiltersAsync()
    
    
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