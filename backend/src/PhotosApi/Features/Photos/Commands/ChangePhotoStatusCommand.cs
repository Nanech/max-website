using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;

namespace PhotosApi.Features.Photos.Commands;

public record ChangePhotoStatusCommand(
    [Required] Guid PhotoId,
    [Required] PhotoStatus NewCategory = PhotoStatus.Draft
) : IRequest;


public class ChangePhotoStatusHandler(
    IStorageRepository storage,
    PhotosDbContext dbContext,
    ILogger<ChangePhotoStatusHandler> logger
) : IRequestHandler<ChangePhotoStatusCommand>
{
    public async Task Handle(ChangePhotoStatusCommand request, CancellationToken cancellationToken)
    {
        var photo = await dbContext.Photos.
            FirstOrDefaultAsync(p => p.PhotoId == request.PhotoId,  cancellationToken);
        
        if (photo == null)
            throw new KeyNotFoundException($"Photo with ID {request.PhotoId} not found");

        if (photo.Status == request.NewCategory) return;
        
        // file path logic
        var oldPath = photo.S3Path;
        if (string.IsNullOrEmpty(oldPath))
            throw new InvalidOperationException($"Photo with ID {request.PhotoId} has no S3 path");
        
        var filePath = StoragePaths.GetFileName(oldPath);
        logger.LogDebug("The photo with id {PhotoPhotoId} has file path: {FilePath}", photo.PhotoId, filePath);
        var newPath = StoragePaths.BuildPhotoPath(request.NewCategory, filePath);

        if (oldPath == newPath) return;
        
        // create a copy of object in s3
        await storage.ChangeFilePathAsync(storage.DefaultPhotosBucket, oldPath, newPath, cancellationToken);
     
        // change data in db
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        photo.Status = request.NewCategory;
        photo.S3Path = newPath;
        dbContext.Photos.Update(photo);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        try
        {
            await storage.RemoveFileAsync(storage.DefaultPhotosBucket, oldPath, cancellationToken);
            logger.LogDebug("Old file removed {FilePath}", oldPath);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to remove old file {oldPath}, but operation is ok", oldPath);
        }
    }
}