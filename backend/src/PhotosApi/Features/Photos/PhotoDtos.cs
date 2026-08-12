using FluentValidation;

namespace PhotosApi.Features.Photos;

public record PhotoDto(
    Guid PhotoId,
    DateTime UploadedAt
);

public record PhotosByAlbumRequest(Guid AlbumId);

public record UploadPhotoRequest(
    List<IFormFile> Files, 
    Guid AlbumId
);

public class PhotosByAlbumValidator : AbstractValidator<PhotosByAlbumRequest>
{
    public  PhotosByAlbumValidator()
    {
        RuleFor(x => x.AlbumId).NotEmpty().WithMessage("AlbumId can`t be empty");
    }
}

public class UploadPhotoDtoValidator : AbstractValidator<UploadPhotoRequest>
{
    public UploadPhotoDtoValidator()
    {
        RuleFor(x => x.AlbumId).NotEmpty().WithMessage("AlbumId can`t be empty");
        
        RuleFor(x => x.Files)
            .NotNull().WithMessage("File can`t be null")
            .Must(x => x is { Count: > 0 } ).WithMessage("At lest one file must be supplied");

        RuleForEach(x => x.Files)
            .ChildRules(file =>
            {
                RuleForEach(x => x.Files)
                    .ChildRules(file =>
                    {
                        file.RuleFor(f => f)
                            .NotNull().WithMessage("File can`t be null")
                            .Must(x => x.Length > 0).WithMessage("File must be supplied")
                            .ValidPhotoFile();
                    });
            });
    }
}

