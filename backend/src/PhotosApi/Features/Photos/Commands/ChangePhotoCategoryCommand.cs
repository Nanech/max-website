using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PhotosApi.Contracts;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;

namespace PhotosApi.Features.Photos.Commands;

public record ChangePhotoCategoryCommand(
    [Required] Guid PhotoId,
    [Required] PhotoStatus NewCategory = PhotoStatus.Draft
) : IRequest;


public class ChangePhotoCategoryHandler(
    IStorageRepository storage,
    PhotosDbContext dbContext,
    ILogger<ChangePhotoCategoryHandler> logger
) : IRequestHandler<ChangePhotoCategoryCommand>
{
    public async Task Handle(ChangePhotoCategoryCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        var photo = await dbContext.Photos.
            FirstOrDefaultAsync(p => p.PhotoId == request.PhotoId,  cancellationToken);
        
        if (photo == null)
            throw new KeyNotFoundException($"Photo with ID {request.PhotoId} not found");
        
        if (photo.Status == request.NewCategory)
            throw new InvalidOperationException($"Current status of Photo with ID {request.PhotoId} is already in set");
        
        photo.Status = request.NewCategory;

        var oldPath = photo.S3Path;
        var filePath = StoragePaths.GetFileName(oldPath);
        logger.LogDebug("The photo with id {PhotoPhotoId} has file path: {FilePath}", photo.PhotoId, filePath);
        var newPath = StoragePaths.BuildPhotoPath(request.NewCategory, filePath);
        photo.S3Path = newPath;
        
        dbContext.Photos.Update(photo);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
        await storage.ChangeFilePathAsync(storage.DefaultPhotosBucket, oldPath, newPath, cancellationToken);
        
        
    }
}