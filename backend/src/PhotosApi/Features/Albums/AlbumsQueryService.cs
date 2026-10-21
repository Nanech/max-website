using Microsoft.EntityFrameworkCore;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Albums;

public class AlbumsQueryService(
    PhotosDbContext dbContext
)
{
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
                a.CreatedAt,
                a.CoverPhotoId
            ))
            .ToListAsync(ct);
        return albums;
    }

    public async Task<IEnumerable<AlbumCardDto>> GetAlbumsByIdsAsync(IEnumerable<Guid> albumIds, CancellationToken ct)
    {
        var enumerable = albumIds as Guid[] ?? [.. albumIds];
        if (enumerable.Length == 0) return [];

        var albums = await dbContext.Albums
            .Where(a => enumerable.Contains(a.AlbumId))
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AlbumCardDto(
                a.AlbumId,
                a.Name,
                a.CoverPhotoId
            )).ToArrayAsync(ct);
        
        return albums;
    }
    
    
}