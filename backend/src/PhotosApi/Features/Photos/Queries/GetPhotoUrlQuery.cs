using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Photos.Queries;

public record GetPhotoUrlQuery(Guid Id) : IRequest<string>;

public class GetPhotoUrlHandler(IStorageRepository storage, PhotosDbContext dbContext)
    : IRequestHandler<GetPhotoUrlQuery, string>
{
    private const string BucketName = "photos-bucket";
    private const int ExpirySeconds = 3600; // 1 hour
    
    public async Task<string> Handle(GetPhotoUrlQuery request, CancellationToken cancellationToken)
    {
        var photo = await dbContext.Photos
            .Select(p => new { p.S3Path, p.PhotoId })
            .FirstOrDefaultAsync(p => p.PhotoId == request.Id, cancellationToken);

        if (photo == null)
            throw new FileNotFoundException($"Photo with ID {request.Id} not found");

        var presignedUrl = await storage.GetPresignedUrl(BucketName, photo.S3Path, ExpirySeconds);
        
        return presignedUrl;
    }
}