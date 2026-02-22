using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Photos.Commands;

public record DeletePhotoCommand(Guid Id) :  IRequest;

public class DeletePhotoHandle(IStorageRepository storage, PhotosDbContext dbContext)
    : IRequestHandler<DeletePhotoCommand>
{
    public async Task Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        var photo = await dbContext.Photos
            .FirstOrDefaultAsync(p => p.PhotoId == request.Id, cancellationToken);
        
        if (photo == null)
            throw new FileNotFoundException($"Photo with id {request.Id} not found");
        
        dbContext.Photos.Remove(photo);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await storage.RemoveFileAsync(storage.DefaultPhotosBucket, photo.S3Path, cancellationToken);
    }
}