using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Albums.Queries;

public record GetAlbumsQuery(short? CategoryId = null) : IRequest<List<AlbumDto>>;

public record AlbumDto(
    Guid AlbumId,
    string Name,
    short? CategoryId,
    string? CategoryName,
    short ShootYear,
    int PhotosCount,
    DateTime CreatedAt
);

public class GetAlbumHandler(
    PhotosDbContext dbContext,
    ILogger<GetAlbumHandler> logger
) : IRequestHandler<GetAlbumsQuery, List<AlbumDto>>
{
    public async Task<List<AlbumDto>> Handle(GetAlbumsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Albums.AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        
        var albums = await query
            .Include(a => a.Category)
            .Include(a => a.Photos)
            .Select(a => new AlbumDto(
                a.AlbumId,
                a.Name,
                a.CategoryId,
                a.Category != null ? a.Category.Name : null,
                a.ShootYear,
                a.Photos.Count,
                a.CreatedAt
            ))
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken); ;
            
        return albums;
    }
}