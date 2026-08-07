using FluentValidation;

namespace PhotosApi.Features.Photos;

public record PhotoDto(
    Guid PhotoId,
    DateTime UploadedAt
);

public record PhotosByAlbumRequest(Guid AlbumId);


public record UploadPhotoRequest(
    IFormFile File, 
    Guid AlbumId
);


public class PhotosByAlbumValidator : AbstractValidator<PhotosByAlbumRequest>
{
    public  PhotosByAlbumValidator()
    {
        When(x => x.AlbumId != Guid.Empty, () =>
        {
            RuleFor(x => x.AlbumId).NotEmpty().WithMessage("AlbumId can`t be empty");
        });
    }
}

public class UploadPhotoDtoValidator : AbstractValidator<UploadPhotoRequest>
{
    public UploadPhotoDtoValidator()
    {
        RuleFor(x => x.AlbumId).NotEmpty().WithMessage("AlbumId can`t be empty");
        
        RuleFor(x => x.File)
            .NotNull().WithMessage("File can`t be null")
            .Must(x => x.Length > 0).WithMessage("File can`t be empty");

        RuleFor(x => x.File).ValidPhotoFile();
    }
}

