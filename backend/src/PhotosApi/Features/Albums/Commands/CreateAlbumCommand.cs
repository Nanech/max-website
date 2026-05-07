using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;

namespace PhotosApi.Features.Albums.Commands;

public record CreateAlbumCommand(
    string Name,
    short? CategoryId = null,
    short? ShootYear = null
) : IRequest<Guid>;

public class CreateAlbumHandler(
    PhotosDbContext dbContext,
    ILogger<CreateAlbumHandler> logger
) : IRequestHandler<CreateAlbumCommand, Guid>
{
    public async Task<Guid> Handle(CreateAlbumCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Создание альбома {Name}", request.Name);

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await dbContext.Categories.AnyAsync(c => 
                c.CategoryId == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
                throw new ArgumentException($"Category {request.CategoryId} not found");
        }

        var album = new Album
        {
            AlbumId = Guid.NewGuid(),
            Name = request.Name,
            CategoryId = request.CategoryId,
            ShootYear = request.ShootYear ?? (short)DateTime.UtcNow.Year,
            CreatedAt =  DateTime.UtcNow
        };
        
        await dbContext.Albums.AddAsync(album, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return album.AlbumId;
    }
} 