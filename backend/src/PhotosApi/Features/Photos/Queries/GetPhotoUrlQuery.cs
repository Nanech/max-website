using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Photos.Queries;

public record GetPhotoUrlQuery(Guid Id) : IRequest<Guid>, IRequest<string>;

public class GetPhotoUrlHandler(IStorageRepository storage, PhotosDbContext dbContext)
    : IRequestHandler<GetPhotoUrlQuery, string>
{
    private const string BucketName = "photots-bucket";
    private const int ExpirySeconds = 300;
    
    public async Task<string> Handle(GetPhotoUrlQuery request, CancellationToken cancellationToken)
    {
        var photo = await dbContext.Photos
            .Select(p => new { p.S3FilePath, p.PhotoId })
            .FirstOrDefaultAsync(p => p.PhotoId == request.Id, cancellationToken);

        if (photo == null)
            throw new FileNotFoundException($"Photo with ID {request.Id} not found");

        var presignedUrl = await storage.GetPresignedUrl(BucketName, photo.S3FilePath, ExpirySeconds);
        
        return presignedUrl;
    }
}