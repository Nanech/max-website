using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Albums.Queries;

public record GetAlbumByIdQuery(Guid AlbumId) : IRequest<AlbumDetailDto>;

public class AlbumDetailDto(
    Guid AlbumId,
    string Name,
    short? CategoryId,
    string? CategoryName,
    short ShootYear,
    DateTime CreateAt,
    List<PhotoDto> Photos
);

public record PhotoDto(
    Guid PhotoId,
    string Url,
    string Status,
    DateTime UploadedAt
);

public class GetAlbumByIdHandler(
    PhotosDbContext dbContext,
    IStorageRepository storage
) : IRequestHandler<GetAlbumByIdQuery, AlbumDetailDto>
{
    public async Task<AlbumDetailDto> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        var album = await dbContext.Albums
            .Include(a => a.Category)
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.AlbumId == request.AlbumId, cancellationToken);
        
        if (album == null)
            throw new KeyNotFoundException($"Album {request.AlbumId} not found");

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
}
