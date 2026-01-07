using System.ComponentModel.DataAnnotations;
using MediatR;
using PhotosApi.Contracts;
using PhotosApi.Helpers;
using PhotosApi.Infrastructure.Data;
using PhotosApi.Models;
using PhotosApi.Services;

namespace PhotosApi.Features.Photos.Commands;

public record UploadPhotoCommand(
    [Required] IFormFile File,
    [MinLength(1)] List<CategoryType> Categories,
    [Range(1900, 2100)] short? ShootYear = null
) : IRequest<Guid>;

public class UploadPhotoHandler(
    IStorageRepository storage,
    PhotosDbContext dbContext,
    CategoryService categoryService
) : IRequestHandler<UploadPhotoCommand, Guid>
{
    public async Task<Guid> Handle(UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        var exisitngCategories = await
            categoryService.GetCategoriesByTypeAsync(
                request.Categories,
                cancellationToken
            );
            
        var newPhotoId = Guid.NewGuid();
        var objectName = $"{newPhotoId}-{request.File.FileName}";
        
        // todo: not optimized way to handle stream
        await using var stream = request.File.OpenReadStream();
        
        var args = new UploadFileArgs(
            storage.DefaultPhotosBucket, objectName, 
            stream, request.File.ContentType,
            cancellationToken);        
        
        var s3FilePath = await storage.UploadFileAsync(args);

        var newPhoto = new Photo
        {
            PhotoId = newPhotoId,
            S3FilePath = s3FilePath,
            PhotosToCategory = exisitngCategories
                .Select(c => new PhotoCategories()
                {
                    CategoryId = c.CategoryId,
                    PhotoId = newPhotoId
                })
                .ToList(),
            ShootYear = request.ShootYear ?? (short)DateTime.UtcNow.Year,
            UploadedAt = DateTime.UtcNow
        };

        await dbContext.Photos.AddAsync(newPhoto, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return newPhotoId;
    }
}